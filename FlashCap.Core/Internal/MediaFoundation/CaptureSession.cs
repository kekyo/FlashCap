#if FLASHCAP_MEDIAFOUNDATION
////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////
using System;
using System.Runtime.InteropServices;
#if NET8_0_OR_GREATER
using System.Runtime.InteropServices.Marshalling;
#endif
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Media.MediaFoundation;
using Windows.Win32.System.Com;
using static FlashCap.Internal.MediaFoundation.MediaFoundationInterop;

namespace FlashCap.Internal.MediaFoundation;

[SupportedOSPlatform("windows6.1")]
internal sealed unsafe partial class CaptureSession : IDisposable
{
    private IMFActivate* activate;
    private IMFMediaSource* mediaSource;
    private IMFSourceReader* reader;
    private int? defaultStride;
    private FrameHandler? frameHandler;
    private SourceReaderCallback? callback;
    private AsyncSourceReaderState? state;
    private long? firstTimestamp;
    private long frameIndex;

    private CaptureSession()
    {
    }

    internal Task Completion => this.state?.Completion ?? Task.CompletedTask;
    internal Task StopRequested => this.state?.StopRequested ?? Task.CompletedTask;
    internal Exception? FlushFailure => this.state?.FlushFailure;

    internal static CaptureSession Open(string symbolicLink, FormatKey formatKey, FrameHandler frameHandler)
    {
        var session = new CaptureSession();
        try
        {
            session.frameHandler = frameHandler;
            session.callback = new SourceReaderCallback(session);
            session.activate = FindActivate(symbolicLink);
            session.mediaSource = ActivateMediaSource(session.activate);
            var callbackPointer = NativeMethods_MediaFoundation.GetComInterfaceForObject(session.callback);
            try
            {
                session.reader = CreateSourceReader(session.mediaSource, (IUnknown*)callbackPointer);
            }
            finally
            {
                NativeMethods_MediaFoundation.ReleaseComInterface(callbackPointer);
            }
            session.defaultStride = ConfigureReader(session.reader, formatKey);
            session.state = new AsyncSourceReaderState(session.RequestSample, session.Flush);
            return session;
        }
        catch
        {
            session.Dispose();
            throw;
        }
    }

    internal bool Start() => this.state?.Start() ??
        throw new ObjectDisposedException(nameof(CaptureSession));

    internal void Stop() => this.state?.Stop();

    private void RequestSample()
    {
        MediaFoundationHelpers.ThrowIfFailed(
            this.reader->ReadSample(
                VideoStreamIndex,
                0,
                null,
                null,
                null,
                null),
            "IMFSourceReader.ReadSample(async)");
    }

    private void Flush()
    {
        MediaFoundationHelpers.ThrowIfFailed(
            this.reader->Flush(VideoStreamIndex),
            "IMFSourceReader.Flush(async)");
    }

    private void OnReadSample(int status, uint flags, long timestamp, IMFSample* sample)
    {
        var state = this.state;
        if (state is null || !state.EnterCallback(out var processSample))
        {
            return;
        }

        Exception? failure = null;
        try
        {
            if (!processSample)
            {
                return;
            }
            if (status < 0)
            {
                throw new COMException("FlashCap: Media Foundation asynchronous read failed.", status);
            }

            var readerFlags = (MF_SOURCE_READER_FLAG)flags;
            if ((readerFlags & (MF_SOURCE_READER_FLAG.MF_SOURCE_READERF_ERROR |
                                MF_SOURCE_READER_FLAG.MF_SOURCE_READERF_ENDOFSTREAM |
                                MF_SOURCE_READER_FLAG.MF_SOURCE_READERF_NATIVEMEDIATYPECHANGED |
                                MF_SOURCE_READER_FLAG.MF_SOURCE_READERF_CURRENTMEDIATYPECHANGED)) != 0)
            {
                throw new InvalidOperationException(
                    $"FlashCap: Media Foundation capture stream changed state ({readerFlags}).");
            }
            if (sample is not null &&
                (readerFlags & MF_SOURCE_READER_FLAG.MF_SOURCE_READERF_STREAMTICK) == 0)
            {
                this.firstTimestamp ??= timestamp;
                ProcessSample(
                    sample,
                    this.defaultStride,
                    Math.Max(0, timestamp - this.firstTimestamp.Value) / 10,
                    this.frameIndex++,
                    this.frameHandler ?? throw new ObjectDisposedException(nameof(CaptureSession)));
            }
        }
        catch (Exception exception)
        {
            failure = exception;
        }
        finally
        {
            state.ExitCallback(failure, requestNextSample: true);
        }
    }

    private void OnFlush()
    {
        var state = this.state;
        if (state is null || !state.EnterCallback(out _))
        {
            return;
        }
        try
        {
            state.OnFlushed();
        }
        finally
        {
            state.ExitCallback(null, requestNextSample: false);
        }
    }

    private void OnEvent()
    {
        var state = this.state;
        if (state is null || !state.EnterCallback(out _))
        {
            return;
        }
        state.ExitCallback(null, requestNextSample: false);
    }

    private static int? ConfigureReader(IMFSourceReader* reader, FormatKey formatKey)
    {
        MediaFoundationHelpers.ThrowIfFailed(
            reader->SetStreamSelection(unchecked((uint)MF_SOURCE_READER_CONSTANTS.MF_SOURCE_READER_ALL_STREAMS), false),
            "IMFSourceReader.SetStreamSelection(all)");
        MediaFoundationHelpers.ThrowIfFailed(
            reader->SetStreamSelection(VideoStreamIndex, true),
            "IMFSourceReader.SetStreamSelection(video)");

        IMFMediaType* mediaType = null;
        try
        {
            MediaFoundationHelpers.ThrowIfFailed(
                reader->GetNativeMediaType(VideoStreamIndex, formatKey.MediaTypeIndex, &mediaType),
                "IMFSourceReader.GetNativeMediaType");
            if (mediaType is null ||
                !TryCreateFormat(mediaType, formatKey.MediaTypeIndex, out var selected) ||
                selected.Key != formatKey)
            {
                throw new InvalidOperationException(
                    "FlashCap: The selected Media Foundation format is no longer available.");
            }

            MediaFoundationHelpers.ThrowIfFailed(
                reader->SetCurrentMediaType(VideoStreamIndex, mediaType),
                "IMFSourceReader.SetCurrentMediaType");
            return mediaType->GetUINT32(in PInvoke.MF_MT_DEFAULT_STRIDE, out var stride).Succeeded ?
                unchecked((int)stride) : null;
        }
        finally
        {
            Release(mediaType);
        }
    }

    private static void ProcessSample(
        IMFSample* sample,
        int? defaultStride,
        long timestampMicroseconds,
        long frameIndex,
        FrameHandler frameHandler)
    {
        IMFMediaBuffer* buffer = null;
        MediaFoundationHelpers.ThrowIfFailed(
            sample->ConvertToContiguousBuffer(&buffer),
            "IMFSample.ConvertToContiguousBuffer");
        if (buffer is null)
        {
            throw new InvalidOperationException("FlashCap: Media Foundation returned no sample buffer.");
        }

        byte* data = null;
        bool locked = false;
        try
        {
            uint currentLength = 0;
            MediaFoundationHelpers.ThrowIfFailed(buffer->Lock(&data, null, &currentLength), "IMFMediaBuffer.Lock");
            locked = true;
            if (data is null || currentLength == 0 || currentLength > int.MaxValue)
            {
                throw new InvalidOperationException(
                    "FlashCap: Media Foundation returned an invalid frame buffer.");
            }

            frameHandler(
                data,
                checked((int)currentLength),
                defaultStride,
                timestampMicroseconds,
                frameIndex);
        }
        finally
        {
            if (locked)
            {
                _ = buffer->Unlock();
            }
            Release(buffer);
        }
    }

    public void Dispose()
    {
        this.callback?.Detach();
        this.callback = null;
        this.state = null;
        this.frameHandler = null;
        if (this.reader is null && this.mediaSource is not null)
        {
            _ = this.mediaSource->Shutdown();
        }
        Release(this.reader);
        this.reader = null;
        if (this.activate is not null)
        {
            _ = this.activate->ShutdownObject();
        }
        Release(this.mediaSource);
        this.mediaSource = null;
        Release(this.activate);
        this.activate = null;
    }

    internal static IMFActivate* FindActivate(string symbolicLink)
    {
        IMFActivate** devices = null;
        uint count = 0;
        try
        {
            devices = EnumerateDeviceSources(out count);
            for (uint index = 0; index < count; index++)
            {
                var activate = devices[index];
                if (activate is not null && string.Equals(
                        GetAllocatedString(activate, in PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK).Trim(),
                        symbolicLink, StringComparison.OrdinalIgnoreCase))
                {
                    devices[index] = null;
                    return activate;
                }
            }
        }
        finally
        {
            FreeActivateArray(devices, count);
        }
        throw new InvalidOperationException("FlashCap: The Media Foundation device is no longer available.");
    }

#if NET8_0_OR_GREATER
    [GeneratedComClass]
#else
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
#endif
    private sealed partial class SourceReaderCallback : NativeMethods_MediaFoundation.IMFSourceReaderCallbackInterop
    {
        private CaptureSession? owner;

        internal SourceReaderCallback(CaptureSession owner) => this.owner = owner;

        public int OnReadSample(int status, uint streamIndex, uint streamFlags, long timestamp, IntPtr sample)
        {
            Volatile.Read(ref this.owner)?.OnReadSample(
                status,
                streamFlags,
                timestamp,
                (IMFSample*)sample);
            return 0;
        }

        public int OnFlush(uint streamIndex)
        {
            Volatile.Read(ref this.owner)?.OnFlush();
            return 0;
        }

        public int OnEvent(uint streamIndex, IntPtr mediaEvent)
        {
            Volatile.Read(ref this.owner)?.OnEvent();
            return 0;
        }

        internal void Detach() => Interlocked.Exchange(ref this.owner, null);
    }
}
#endif
