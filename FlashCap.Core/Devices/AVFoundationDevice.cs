using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FlashCap.Internal;
using static FlashCap.Internal.NativeMethods;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibAVFoundation;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibC;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibCoreMedia;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibCoreVideo;

namespace FlashCap.Devices;

public sealed class AVFoundationDevice : CaptureDevice
{
    private readonly string uniqueID;

    private AVCaptureDevice? device;
    private AVCaptureDeviceInput? deviceInput;
    private AVCaptureVideoDataOutput? deviceOutput;
    private AVCaptureSession? session;
    private FrameProcessor? frameProcessor;
    private IntPtr bitmapHeader;
    private bool transcodeIfYUV;

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

        Marshal.FreeHGlobal(this.bitmapHeader);

        if (frameProcessor is { })
        {
            await frameProcessor.DisposeAsync().ConfigureAwait(false);
        }

        await base.OnDisposeAsync().ConfigureAwait(false);
    }

    protected override Task OnInitializeAsync(VideoCharacteristics characteristics, bool transcodeIfYUV, FrameProcessor frameProcessor, CancellationToken ct)
    {
        this.frameProcessor = frameProcessor;
        this.transcodeIfYUV = transcodeIfYUV;
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

        this.device = AVCaptureDevice.DeviceWithUniqueID(uniqueID);

        if (this.device is null)
        {
            throw new InvalidOperationException(
                $"FlashCap: Couldn't find device: UniqueID={this.uniqueID}");
        }

        this.device.LockForConfiguration();
        this.device.ActiveFormat = this.device.Formats
            .FirstOrDefault(format =>
                format.FormatDescription.Dimensions is var dimensions &&
                dimensions.Width == characteristics.Width &&
                dimensions.Height == characteristics.Height)
            ?? throw new InvalidOperationException(
                $"FlashCap: Couldn't set video format: UniqueID={this.uniqueID}");

        var frameDuration = CMTimeMake(
            characteristics.FramesPerSecond.Numerator,
            characteristics.FramesPerSecond.Denominator);

        this.device.ActiveVideoMinFrameDuration = frameDuration;
        this.device.ActiveVideoMaxFrameDuration = frameDuration;
        this.device.UnlockForConfiguration();

        this.deviceInput = new AVCaptureDeviceInput(this.device);
        this.deviceOutput = new AVCaptureVideoDataOutput();
        this.deviceOutput.SetPixelFormatType(pixelFormatType);
        this.deviceOutput.SetSampleBufferDelegate(
            new VideoBufferHandler(this),
            GetGlobalQueue(DispatchQualityOfService.Background, flags: default));

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
        buffer.CopyIn(this.bitmapHeader, pData, size, timestampMicroseconds, frameIndex, this.transcodeIfYUV);
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
            }
        }
    }
}
