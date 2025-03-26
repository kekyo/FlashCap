////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Yoh Deadfall (@YohDeadfall)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FlashCap.Internal;
using static FlashCap.Internal.AVFoundation.LibAVFoundation;
using static FlashCap.Internal.NativeMethods;
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

    public AVFoundationDevice(string uniqueID, string modelID) :
        base(uniqueID, modelID)
    {
        this.uniqueID = uniqueID;
    }

    protected override async Task OnDisposeAsync()
    {
        if (this.session != null)
        {
            this.session.StopRunning();
            this.session.Dispose();
        }
        
        this.device?.Dispose();
        this.deviceInput?.Dispose();
        this.deviceOutput?.Dispose();
        this.queue?.Dispose();

        Marshal.FreeHGlobal(this.bitmapHeader);

        if (frameProcessor is not null)
        {
            await frameProcessor.DisposeAsync().ConfigureAwait(false);
        }

        await base.OnDisposeAsync().ConfigureAwait(false);
    }

    protected override Task OnInitializeAsync(VideoCharacteristics characteristics, TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor, CancellationToken ct)
    {
        
        this.frameProcessor = frameProcessor;
        this.Characteristics = characteristics;

        if (!NativeMethods_AVFoundation.PixelFormatMap.TryGetValue(characteristics.PixelFormat, out var pixelFormatType) ||
            !NativeMethods.GetCompressionAndBitCount(characteristics.PixelFormat, out var compression, out var bitCount))
        {
            throw new ArgumentException(
                $"FlashCap: Couldn't set video format: UniqueID={this.uniqueID}");
        }

        this.bitmapHeader = NativeMethods.AllocateMemory(new IntPtr(MarshalEx.SizeOf<BITMAPINFOHEADER>()));

        try
        {
            unsafe
            {
                var pBih = (BITMAPINFOHEADER*)this.bitmapHeader.ToPointer();

                pBih->biSize = MarshalEx.SizeOf<BITMAPINFOHEADER>();
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
        var formatSelected = this.device.Formats
            .FirstOrDefault(format =>
                format.FormatDescription.Dimensions is var dimensions &&
                format.FormatDescription.MediaType == CMMediaType.Video  &&
                dimensions.Width == characteristics.Width &&
                dimensions.Height == characteristics.Height)
            ?? throw new InvalidOperationException(
                $"FlashCap: Couldn't set video format: UniqueID={this.uniqueID}");

        this.device.ActiveFormat = formatSelected;

        var frameDuration = CMTimeMake(
            characteristics.FramesPerSecond.Denominator,
            characteristics.FramesPerSecond.Numerator);
        
        device.ActiveVideoMinFrameDuration = frameDuration;
        device.ActiveVideoMaxFrameDuration = frameDuration;

        this.deviceInput = new AVCaptureDeviceInput(device);
        
        this.deviceOutput = new AVCaptureVideoDataOutput();
        
        
         if (this.deviceOutput.AvailableVideoCVPixelFormatTypes?.Any() == true)
        {
            var validPixelFormat = this.deviceOutput.AvailableVideoCVPixelFormatTypes.FirstOrDefault(p => p == pixelFormatType);
            this.deviceOutput.SetPixelFormatType(validPixelFormat);
        }
        else
        {
            // Fallback to the mapped pixel format if no available list is provided
            this.deviceOutput.SetPixelFormatType(pixelFormatType);
        }
        
        this.deviceOutput.SetSampleBufferDelegate(new VideoBufferHandler(this), this.queue);
        this.deviceOutput.AlwaysDiscardsLateVideoFrames = true;

        this.device.UnlockForConfiguration();
        
        this.session = new AVCaptureSession();
        this.session.AddInput(this.deviceInput);
        
        if(session.CanAddOutput(deviceOutput)) session.AddOutput(this.deviceOutput);
        else
        {
            throw new Exception("Can't add video output");
        }
        
   		
        return TaskCompat.CompletedTask;
    }

    protected override Task OnStartAsync(CancellationToken ct)
    {
        this.session?.StartRunning();
        return TaskCompat.CompletedTask;
    }

    protected override Task OnStopAsync(CancellationToken ct)
    {
        this.session?.StopRunning();
        return TaskCompat.CompletedTask;
    }

    protected override void OnCapture(IntPtr pData, int size, long timestampMicroseconds, long frameIndex, PixelBuffer buffer)
    {
        buffer.CopyIn(this.bitmapHeader, pData, size, timestampMicroseconds, frameIndex, TranscodeFormats.Auto);
    }

    internal sealed class VideoBufferHandler : AVCaptureVideoDataOutputSampleBuffer
    {
        private readonly AVFoundationDevice device;
        private int frameIndex;

        public VideoBufferHandler(AVFoundationDevice device)
        {
            this.device = device;
        }

        public override void DidDropSampleBuffer(IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection)
        {
            Debug.WriteLine("Dropped");
            var valid = CMSampleBufferIsValid(sampleBuffer);
        }

        public void CaptureOutputCallback(IntPtr self, IntPtr _cmd, IntPtr output, IntPtr sampleBuffer,
            IntPtr connection)
        {

            var pixelBuffer = CMSampleBufferGetImageBuffer(sampleBuffer);
            
            if (pixelBuffer == IntPtr.Zero)
            {
                throw new Exception("Failed to get image buffer");
            }
            else
            {
                // Lock the base address for reading, process the buffer, etc.
                CVPixelBufferLockBaseAddress(pixelBuffer, PixelBufferLockFlags.ReadOnly);

                try
                {
                    // Process the pixel buffer as needed
                    this.device.frameProcessor?.OnFrameArrived(
                        this.device,
                        CVPixelBufferGetBaseAddress(pixelBuffer),
                        (int)CVPixelBufferGetDataSize(pixelBuffer),
                        (long)(CMTimeGetSeconds(CMSampleBufferGetDecodeTimeStamp(sampleBuffer)) * 1000),
                        frameIndex++);
                }
                finally
                {
                    CVPixelBufferUnlockBaseAddress(pixelBuffer, PixelBufferLockFlags.ReadOnly);
                }
            }
        }
    }
}
