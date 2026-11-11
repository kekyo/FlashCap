////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////


#if !(NET35 || NET40 || NET45 || NETSTANDARD1_3)
using FlashCap.Internal;
using FlashCap.Internal.PipeWire;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Devices;

/// <summary>Captures camera frames through PipeWire.</summary>
/// <remarks>
/// Frames always conform to their advertised FlashCap pixel layout, including when
/// color conversion is disabled. PipeWire buffers remain owned by PipeWire.
/// </remarks>
[SupportedOSPlatform("linux")]
public sealed class PipeWireCaptureDevice : CaptureDevice
{
    private readonly PipeWireCamera camera;
    private readonly PipeWireFormat format;
    private readonly TimestampCounter clock = new();
    private PipeWireConnection? connection;
    private FrameProcessor? frameProcessor;
    private TranscodeFormats transcodeFormat;
    private IntPtr bitmapHeader;
    private byte[]? repackBuffer;
    private long frameIndex;
    private bool disposed;

    internal PipeWireCaptureDevice(object identity, PipeWireCamera camera, PipeWireFormat format) : base(identity, camera.Name)
    {
        this.camera = camera;
        this.format = format;
    }

    /// <inheritdoc/>
    protected override async Task OnInitializeAsync(VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat, FrameProcessor frameProcessor, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!NativeMethods.GetCompressionAndBitCount(characteristics.PixelFormat, out var compression, out var bitCount))
            throw new ArgumentException("Unsupported PipeWire pixel format.", nameof(characteristics));
        this.Characteristics = characteristics;
        this.transcodeFormat = transcodeFormat;
        this.frameProcessor = frameProcessor;
        this.bitmapHeader = NativeMethods.AllocateMemory((IntPtr)Marshal.SizeOf<NativeMethods.BITMAPINFOHEADER>());
        unsafe
        {
            var header = (NativeMethods.BITMAPINFOHEADER*)this.bitmapHeader;
            *header = default;
            header->biSize = Marshal.SizeOf<NativeMethods.BITMAPINFOHEADER>();
            header->biWidth = characteristics.Width;
            header->biHeight = characteristics.Height;
            header->biPlanes = 1;
            header->biBitCount = bitCount;
            header->biCompression = compression;
            header->biSizeImage = header->CalculateImageSize();
        }
        this.connection = await PipeWireConnection.OpenAsync(this.camera.UsePortal, ct).ConfigureAwait(false);
        this.connection.Disconnected += () => this.IsRunning = false;
    }

    /// <inheritdoc/>
    protected override async Task OnStartAsync(CancellationToken ct)
    {
        if (this.disposed) throw new ObjectDisposedException(nameof(PipeWireCaptureDevice));
        ct.ThrowIfCancellationRequested();
        if (this.IsRunning) return;
        this.clock.Restart();
        this.frameIndex = 0;
        this.IsRunning = true;
        try { await this.connection!.StartAsync(this.camera, this.format, this.OnFrame, ct).ConfigureAwait(false); }
        catch
        {
            this.IsRunning = false;
            throw;
        }
    }

    /// <inheritdoc/>
    protected override Task OnStopAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        this.connection?.Stop();
        this.IsRunning = false;
        return TaskCompat.CompletedTask;
    }

    private unsafe void OnFrame(PipeWirePlane[] planes)
    {
        var characteristics = this.Characteristics;
        var frame = PipeWireFrames.Normalize(characteristics.PixelFormat, characteristics.Width,
            characteristics.Height, planes, false, ref this.repackBuffer);
        // CopyIn sets the actual payload size; the negotiated image size is only a hint.
        ((NativeMethods.BITMAPINFOHEADER*)this.bitmapHeader)->biSizeImage = frame.Length;
        var timestamp = this.clock.ElapsedMicroseconds;
        var index = this.frameIndex++;
        if (frame.Pointer != IntPtr.Zero)
            this.frameProcessor!.OnFrameArrived(this, frame.Pointer, frame.Length, timestamp, index);
        else
            fixed (byte* pointer = this.repackBuffer!)
                this.frameProcessor!.OnFrameArrived(this, (IntPtr)pointer, frame.Length, timestamp, index);
    }

    /// <inheritdoc/>
    protected override void OnCapture(IntPtr pData, int size, long timestampMicroseconds, long frameIndex, PixelBuffer buffer) =>
        buffer.CopyIn(this.bitmapHeader, pData, size, timestampMicroseconds, frameIndex, this.transcodeFormat);

    /// <inheritdoc/>
    protected override async Task OnDisposeAsync()
    {
        if (this.disposed) return;
        this.disposed = true;
        try
        {
            this.connection?.Dispose();
            this.connection = null;
            this.IsRunning = false;
            if (this.frameProcessor is { } processor)
            {
                this.frameProcessor = null;
                await processor.DisposeAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            if (this.bitmapHeader != IntPtr.Zero)
            {
                NativeMethods.FreeMemory(this.bitmapHeader);
                this.bitmapHeader = IntPtr.Zero;
            }
            this.repackBuffer = null;
        }
    }
}
#endif
