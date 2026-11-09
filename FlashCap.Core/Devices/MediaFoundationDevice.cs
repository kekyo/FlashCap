#if FLASHCAP_MEDIAFOUNDATION
////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using FlashCap.Internal.MediaFoundation;

namespace FlashCap.Devices;

[SupportedOSPlatform("windows6.1")]
public sealed class MediaFoundationDevice : CaptureDevice
{
    private readonly string symbolicLink;
    private readonly MediaFoundationInterop.FormatKey formatKey;

    private FrameProcessor? frameProcessor;
    private TranscodeFormats transcodeFormat;
    private IntPtr bitmapHeader;
    private byte[]? repackBuffer;
    private MediaFoundationWorker? worker;
    private CaptureSource? source;
    private CaptureSession? captureSession;
    private Task captureTask = TaskCompat.CompletedTask;
    private bool disposed;

    internal MediaFoundationDevice(
        string symbolicLink,
        string name,
        MediaFoundationInterop.FormatKey formatKey) :
        base(symbolicLink, name)
    {
        this.symbolicLink = symbolicLink;
        this.formatKey = formatKey;
    }

    protected override async Task OnInitializeAsync(
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!NativeMethods.GetCompressionAndBitCount(
            characteristics.PixelFormat, out var compression, out var bitCount))
        {
            throw new ArgumentException("FlashCap: Unsupported Media Foundation format.", nameof(characteristics));
        }

        this.Characteristics = characteristics;
        this.transcodeFormat = transcodeFormat;
        this.frameProcessor = frameProcessor;
        this.bitmapHeader = NativeMethods.AllocateMemory(
            new IntPtr(MarshalEx.SizeOf<NativeMethods.BITMAPINFOHEADER>()));

        unsafe
        {
            var header = (NativeMethods.BITMAPINFOHEADER*)this.bitmapHeader;
            *header = default;
            header->biSize = MarshalEx.SizeOf<NativeMethods.BITMAPINFOHEADER>();
            header->biWidth = characteristics.Width;
            header->biHeight = characteristics.Height;
            header->biPlanes = 1;
            header->biBitCount = bitCount;
            header->biCompression = compression;
            header->biSizeImage = header->CalculateImageSize();
        }
        this.worker = new MediaFoundationWorker(MediaFoundationInterop.Initialize, MediaFoundationInterop.Uninitialize);
        await this.worker.InvokeAsync(() =>
            this.source = CaptureSource.Open(this.symbolicLink), ct).ConfigureAwait(false);
    }

    protected override async Task OnDisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }
        this.disposed = true;

        try
        {
            await this.OnStopAsync(CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            try
            {
                if (this.worker is { } worker)
                {
                    try
                    {
                        await worker.InvokeAsync(() =>
                        {
                            this.source?.Dispose();
                            this.source = null;
                        }, CancellationToken.None).ConfigureAwait(false);
                    }
                    finally
                    {
                        await worker.DisposeAsync().ConfigureAwait(false);
                        this.worker = null;
                    }
                }
            }
            finally
            {
                try
                {
                    if (this.frameProcessor is not null)
                    {
                        await this.frameProcessor.DisposeAsync().ConfigureAwait(false);
                        this.frameProcessor = null;
                    }
                }
                finally
                {
                    if (this.bitmapHeader != IntPtr.Zero)
                    {
                        NativeMethods.FreeMemory(this.bitmapHeader);
                        this.bitmapHeader = IntPtr.Zero;
                    }
                }
            }
        }
    }

    protected override async Task OnStartAsync(CancellationToken ct)
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(nameof(MediaFoundationDevice));
        }
        // A canceled StopAsync may have requested a flush without awaiting cleanup.
        // Wait for that session before restarting, even while IsRunning still reports true.
        if (this.captureSession?.IsRunning == true)
        {
            return;
        }
        await MediaFoundationHelpers.WaitAsync(this.captureTask, ct).ConfigureAwait(false);
        var worker = this.GetWorker();
        var session = await worker.InvokeAsync(this.StartCapture, ct).ConfigureAwait(false);
        this.captureTask = this.ObserveCaptureAsync(worker, session);
    }

    protected override async Task OnStopAsync(CancellationToken ct)
    {
        if (this.worker is { } worker)
        {
            await worker.InvokeAsync(() => this.captureSession?.Stop(), ct).ConfigureAwait(false);
        }
        await MediaFoundationHelpers.WaitAsync(this.captureTask, ct).ConfigureAwait(false);
    }

    private MediaFoundationWorker GetWorker()
    {
        if (this.disposed || this.worker is null)
        {
            throw new ObjectDisposedException(nameof(MediaFoundationDevice));
        }
        return this.worker;
    }

    private unsafe CaptureSession StartCapture()
    {
        // The persistent worker owns COM initialization, source lifetime and uninitialization.
        // A new reader gets fresh callback state and timestamps for each capture run.
        var session = this.source!.OpenSession(this.formatKey, this.OnFrame);
        try
        {
            if (!session.Start())
            {
                throw new InvalidOperationException(
                    "FlashCap: The Media Foundation session was stopped before it could start.");
            }
            this.captureSession = session;
            this.IsRunning = true;
            return session;
        }
        catch
        {
            session.Dispose();
            throw;
        }
    }

    private async Task ObserveCaptureAsync(MediaFoundationWorker worker, CaptureSession session)
    {
        try
        {
            // Error callbacks can request a stop independently of the public device lock.
            // Reader disposal is queued with camera commands, and never disposes the source.
            await TaskCompat.WhenAny(session.StopRequested, session.Completion).ConfigureAwait(false);
            await worker.InvokeAsync(session.Stop, CancellationToken.None).ConfigureAwait(false);
            await session.Completion.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            if (session.FlushFailure is { } flushFailure)
            {
                throw flushFailure;
            }
            else
            {
                MediaFoundationHelpers.TraceFailure("capture", exception);
            }
        }
        finally
        {
            await worker.InvokeAsync(() =>
            {
                this.IsRunning = false;
                this.captureSession = null;
                session.Dispose();
            }, CancellationToken.None).ConfigureAwait(false);
        }
    }

    protected override Task<CaptureDeviceControl[]> OnGetControlsAsync(CancellationToken ct) =>
        this.GetWorker().InvokeAsync(() => this.source!.Controls.GetControls(), ct);

    protected override Task<CaptureDeviceControlValue?> OnGetControlValueAsync(CaptureDeviceControlTypes type, CancellationToken ct) =>
        this.GetWorker().InvokeAsync(() => this.source!.Controls.GetValue(type), ct);

    protected override Task<bool> OnSetControlValueAsync(CaptureDeviceControlTypes type, int value, bool isAuto, CancellationToken ct) =>
        this.GetWorker().InvokeAsync(() => this.source!.Controls.SetValue(type, value, isAuto), ct);

    internal unsafe void OnFrame(
        byte* data,
        int length,
        int? defaultStride,
        long timestampMicroseconds,
        long frameIndex)
    {
        try
        {
            var frame = this.NormalizeFrame(data, length, defaultStride);
            if (this.frameProcessor is null)
            {
                throw new InvalidOperationException("FlashCap: The frame processor is not initialized.");
            }
            if (frame.Pointer != IntPtr.Zero)
            {
                this.frameProcessor.OnFrameArrived(
                    this, frame.Pointer, frame.Length, timestampMicroseconds, frameIndex);
            }
            else
            {
                fixed (byte* repacked = this.repackBuffer!)
                {
                    this.frameProcessor.OnFrameArrived(
                        this, (IntPtr)repacked, frame.Length, timestampMicroseconds, frameIndex);
                }
            }
        }
        catch (Exception exception)
        {
            MediaFoundationHelpers.TraceFailure("frame callback", exception);
        }
    }

    private unsafe FrameMemory NormalizeFrame(byte* data, int length, int? defaultStride)
    {
        var format = this.Characteristics.PixelFormat;
        if (format == PixelFormats.JPEG)
        {
            return new FrameMemory((IntPtr)data, length);
        }

        MediaFoundationInterop.FrameLayout layout;
        try
        {
            layout = MediaFoundationInterop.GetFrameLayout(
                format,
                this.Characteristics.Width,
                this.Characteristics.Height,
                defaultStride,
                length);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidOperationException(
                "FlashCap: Media Foundation returned an invalid frame buffer.", exception);
        }

        if (layout.SourceStride == layout.TargetStride &&
            (!(format is PixelFormats.RGB15 or PixelFormats.RGB16 or
                PixelFormats.RGB24 or PixelFormats.RGB32 or PixelFormats.ARGB32) || layout.BottomUp))
        {
            return new FrameMemory((IntPtr)data, layout.TargetLength);
        }

        if (this.repackBuffer is null || this.repackBuffer.Length < layout.TargetLength)
        {
            this.repackBuffer = new byte[layout.TargetLength];
        }
        var managedBuffer = this.repackBuffer;
        var reverseRows = !layout.BottomUp &&
            format is PixelFormats.RGB15 or PixelFormats.RGB16 or
                PixelFormats.RGB24 or PixelFormats.RGB32 or PixelFormats.ARGB32;
        MediaFoundationInterop.RepackFrame(
            data,
            length,
            managedBuffer,
            layout,
            reverseRows);
        return new FrameMemory(IntPtr.Zero, layout.TargetLength);
    }

    protected override void OnCapture(
        IntPtr pData,
        int size,
        long timestampMicroseconds,
        long frameIndex,
        PixelBuffer buffer)
    {
        buffer.CopyIn(this.bitmapHeader, pData, size, timestampMicroseconds, frameIndex, this.transcodeFormat);
    }

    private readonly record struct FrameMemory(IntPtr Pointer, int Length);
}
#endif
