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
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using FlashCap.Internal.MediaFoundation;
#if !NET9_0_OR_GREATER
using Lock = System.Object;
#endif

namespace FlashCap.Devices;

[SupportedOSPlatform("windows6.1")]
public sealed class MediaFoundationDevice : CaptureDevice
{
    private readonly Lock sync = new();
    private readonly string symbolicLink;
    private readonly MediaFoundationInterop.FormatKey formatKey;

    private FrameProcessor? frameProcessor;
    private TranscodeFormats transcodeFormat;
    private IntPtr bitmapHeader;
    private byte[]? repackBuffer;
    private CancellationTokenSource? stopSource;
    private Task captureTask = Task.CompletedTask;
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

    protected override Task OnInitializeAsync(
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
        return Task.CompletedTask;
    }

    protected override async Task OnDisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }
        this.disposed = true;

        Exception? stopFailure = null;
        try
        {
            await this.OnStopAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            stopFailure = exception;
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

        if (stopFailure is not null)
        {
            ExceptionDispatchInfo.Capture(stopFailure).Throw();
        }
    }

    protected override async Task OnStartAsync(CancellationToken ct)
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(nameof(MediaFoundationDevice));
        }
        Task previousCapture;
        bool alreadyRunning;
        lock (this.sync)
        {
            previousCapture = this.captureTask;
            alreadyRunning = this.IsRunning &&
                this.stopSource is { IsCancellationRequested: false };
        }
        if (alreadyRunning)
        {
            return;
        }
        await MediaFoundationHelpers.WaitAsync(previousCapture, ct).ConfigureAwait(false);

        var startup = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var stopSource = new CancellationTokenSource();
        lock (this.sync)
        {
            this.stopSource?.Dispose();
            this.stopSource = stopSource;
            this.captureTask = Task.Factory.StartNew(
                () => this.Capture(startup, stopSource.Token),
                CancellationToken.None,
                TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach,
                TaskScheduler.Default);
        }

        try
        {
            await MediaFoundationHelpers.WaitAsync(startup.Task, ct).ConfigureAwait(false);
        }
        catch
        {
            await this.OnStopAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    protected override async Task OnStopAsync(CancellationToken ct)
    {
        this.PrepareStop(out var captureTask);

        try
        {
            await MediaFoundationHelpers.WaitAsync(captureTask, ct).ConfigureAwait(false);
        }
        finally
        {
            if (captureTask.IsCompleted)
            {
                lock (this.sync)
                {
                    if (ReferenceEquals(this.captureTask, captureTask))
                    {
                        this.stopSource?.Dispose();
                        this.stopSource = null;
                        this.captureTask = Task.CompletedTask;
                    }
                }
            }
        }
    }

    private void PrepareStop(out Task captureTask)
    {
        lock (this.sync)
        {
            this.stopSource?.Cancel();
            captureTask = this.captureTask;
        }
    }

    private unsafe void Capture(TaskCompletionSource<bool> startup, CancellationToken stopToken)
    {
        // Runs synchronously on a dedicated MTA thread so COM initialization,
        // Media Foundation lifetime, and COM uninitialization remain on the same thread.
        CaptureSession? session = null;
        bool initialized = false;
        bool startupCompleted = false;
        try
        {
            MediaFoundationInterop.Initialize();
            initialized = true;

            session = CaptureSession.Open(this.symbolicLink, this.formatKey, this.OnFrame);

            if (!session.Start())
            {
                stopToken.ThrowIfCancellationRequested();
                throw new InvalidOperationException(
                    "FlashCap: The Media Foundation session was stopped before it could start.");
            }
            this.IsRunning = true;
            startupCompleted = true;
            startup.TrySetResult(true);

            var stopTask = Task.Delay(Timeout.Infinite, stopToken);
            _ = Task.WaitAny(session.StopRequested, stopTask);
            session.Stop();
            session.Completion.GetAwaiter().GetResult();
        }
        catch (OperationCanceledException) when (stopToken.IsCancellationRequested)
        {
            if (!startupCompleted)
            {
                startup.TrySetCanceled(stopToken);
            }
        }
        catch (Exception exception)
        {
            if (!startupCompleted)
            {
                startup.TrySetException(exception);
            }
            else if (session?.FlushFailure is { } flushFailure)
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
            this.IsRunning = false;
            session?.Dispose();
            if (initialized)
            {
                MediaFoundationInterop.Uninitialize();
            }
            if (!startupCompleted)
            {
                startup.TrySetException(new InvalidOperationException(
                    "FlashCap: Media Foundation capture ended during startup."));
            }
        }
    }

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
            new ReadOnlySpan<byte>(data, length),
            managedBuffer.AsSpan(0, layout.TargetLength),
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
