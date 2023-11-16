using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FlashCap.Internal;
using static FlashCap.Internal.NativeMethods;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibAVFoundation;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibCoreFoundation;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibCoreMedia;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibCoreVideo;

namespace FlashCap.Devices;

public sealed class AVFoundationDevice : CaptureDevice
{
    private readonly string uniqueID;

    private DispatchQueue? queue;
    private AVCaptureDevice? device;
    private AVCaptureDeviceInput? deviceInput;
    private AVCaptureVideoDataOutput? deviceOutput;
    private AVCaptureSession? session;
    private FrameProcessor? frameProcessor;
    private IntPtr bitmapHeader;
    private TranscodeFormats transcodeFormat;

    public AVFoundationDevice(string uniqueID, string modelID) :
        base(uniqueID, modelID)
    {
        this.uniqueID = uniqueID;
    }

    protected override async Task OnDisposeAsync()
    {
        this.device?.Dispose();
        this.deviceInput?.Dispose();
        this.deviceOutput?.Dispose();
        this.session?.Dispose();
        this.queue?.Dispose();

        Marshal.FreeHGlobal(this.bitmapHeader);

        if (frameProcessor is { })
        {
            await frameProcessor.DisposeAsync().ConfigureAwait(false);
        }

        await base.OnDisposeAsync().ConfigureAwait(false);
    }

    protected override Task OnInitializeAsync(VideoCharacteristics characteristics, TranscodeFormats transcodeFormat, FrameProcessor frameProcessor, CancellationToken ct)
    {
        this.frameProcessor = frameProcessor;
        this.transcodeFormat = transcodeFormat;
        this.Characteristics = characteristics;

        if (!NativeMethods_AVFoundation.PixelFormatMap.TryGetValue(characteristics.PixelFormat, out var pixelFormatType) ||
            !NativeMethods.GetCompressionAndBitCount(characteristics.PixelFormat, out var compression, out var bitCount))
        {
            throw new ArgumentException(
                $"FlashCap: Couldn't set video format: UniqueID={this.uniqueID}");
        }

        this.bitmapHeader = NativeMethods.AllocateMemory(new IntPtr(Marshal.SizeOf<BITMAPINFOHEADER>()));

        try
        {
            unsafe
            {
                var pBih = (BITMAPINFOHEADER*)this.bitmapHeader.ToPointer();

                pBih->biSize = sizeof(NativeMethods.BITMAPINFOHEADER);
                pBih->biCompression = compression;
                pBih->biPlanes = 1;
                pBih->biBitCount = bitCount;
                pBih->biWidth = characteristics.Width;
                pBih->biHeight = characteristics.Height;
                pBih->biSizeImage = pBih->CalculateImageSize();
            }
        }
        catch
        {
            NativeMethods.FreeMemory(this.bitmapHeader);
            throw;
        }

        this.queue = new DispatchQueue(nameof(FlashCap));
        this.device = AVCaptureDevice.DeviceWithUniqueID(uniqueID)
            ?? throw new InvalidOperationException(
                $"FlashCap: Couldn't find device: UniqueID={this.uniqueID}");

        this.device.LockForConfiguration();
        this.device.ActiveFormat = this.device.Formats
            .FirstOrDefault(format =>
                format.FormatDescription.Dimensions is var dimensions &&
                dimensions.Width == characteristics.Width &&
                dimensions.Height == characteristics.Height)
            ?? throw new InvalidOperationException(
                $"FlashCap: Couldn't set video format: UniqueID={this.uniqueID}");

        var frameDuration = CMTimeMake(
            characteristics.FramesPerSecond.Denominator,
            characteristics.FramesPerSecond.Numerator);

        this.device.ActiveVideoMinFrameDuration = frameDuration;
        this.device.ActiveVideoMaxFrameDuration = frameDuration;
        this.device.UnlockForConfiguration();

        this.deviceInput = new AVCaptureDeviceInput(this.device);
        this.deviceOutput = new AVCaptureVideoDataOutput();
        this.deviceOutput.SetPixelFormatType(pixelFormatType);
        this.deviceOutput.SetSampleBufferDelegate(new VideoBufferHandler(this), this.queue);
        this.deviceOutput.AlwaysDiscardsLateVideoFrames = true;

        this.session = new AVCaptureSession();
        this.session.AddInput(this.deviceInput);
        this.session.AddOutput(this.deviceOutput);

        return Task.CompletedTask;
    }

    protected override Task OnStartAsync(CancellationToken ct)
    {
        this.session?.StartRunning();
        return Task.CompletedTask;
    }

    protected override Task OnStopAsync(CancellationToken ct)
    {
        this.session?.StopRunning();
        return Task.CompletedTask;
    }

    protected override void OnCapture(IntPtr pData, int size, long timestampMicroseconds, long frameIndex, PixelBuffer buffer)
    {
        buffer.CopyIn(this.bitmapHeader, pData, size, timestampMicroseconds, frameIndex, this.transcodeFormat);
    }

    private sealed class VideoBufferHandler : AVCaptureVideoDataOutputSampleBuffer
    {
        private readonly AVFoundationDevice device;
        private int frameIndex;

        public VideoBufferHandler(AVFoundationDevice device)
        {
            this.device = device;
        }

        public override void DidDropSampleBuffer(IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection)
        {
        }

        public override void DidOutputSampleBuffer(IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection)
        {
            var pixelBuffer = CMSampleBufferGetImageBuffer(sampleBuffer);
            if (pixelBuffer == IntPtr.Zero)
            {
                return;
            }

            var timeStamp = CMSampleBufferGetDecodeTimeStamp(sampleBuffer);
            var seconds = CMTimeGetSeconds(timeStamp);

            CVPixelBufferLockBaseAddress(pixelBuffer, PixelBufferLockFlags.ReadOnly);

            try
            {
                this.device.frameProcessor?.OnFrameArrived(
                    this.device,
                    CVPixelBufferGetBaseAddress(pixelBuffer),
                    (int)CVPixelBufferGetDataSize(pixelBuffer),
                    (long)(seconds * 1000),
                    this.frameIndex++);
            }
            finally
            {
                CVPixelBufferUnlockBaseAddress(pixelBuffer, PixelBufferLockFlags.ReadOnly);
                // Required to return the buffer to the queue of free buffers.
                CFRelease(sampleBuffer);
            }
        }
    }
}
