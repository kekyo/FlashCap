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
        this.queue?.Dispose();

        Marshal.FreeHGlobal(this.bitmapHeader);

        if (frameProcessor is not null)
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
        
        device.ActiveVideoMinFrameDuration = frameDuration;
        device.ActiveVideoMaxFrameDuration = frameDuration;
        
        device.UnlockForConfiguration();

        this.deviceInput = new AVCaptureDeviceInput(device);
        this.deviceOutput = new AVCaptureVideoDataOutput();
        
        if (this.deviceOutput.AvailableVideoCVPixelFormatTypes?.Any() == true)
        {
            var validPixelFormat = this.deviceOutput.AvailableVideoCVPixelFormatTypes.First();
            this.deviceOutput.SetPixelFormatType(validPixelFormat);
        }
        else
        {
            // Fallback to the mapped pixel format if no available list is provided
            this.deviceOutput.SetPixelFormatType(pixelFormatType);
        }
        
        //this.deviceOutput.SetPixelFormatType(pixelFormatType);
        //this.deviceOutput.SetPixelFormatType(deviceOutput.AvailableVideoCVPixelFormatTypes[1]);
        
        this.deviceOutput.SetSampleBufferDelegate(new VideoBufferHandler(this), this.queue);
        this.deviceOutput.AlwaysDiscardsLateVideoFrames = true;

        this.session = new AVCaptureSession();
        this.session.AddInput(this.deviceInput);
        
        if(session.CanAddOutput(deviceOutput)) session.AddOutput(this.deviceOutput);
        else
        {
            throw new Exception("Can't add video output");
        }

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
            Console.WriteLine("Dropped");
            var valid = CMSampleBufferIsValid(sampleBuffer);
        }

        public override void DidOutputSampleBuffer(IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection)
        {
            lock (this) // Or use a proper thread synchronization mechanism
            {
                //Check if is valid 

                CFRetain(sampleBuffer);

                var valid = CMSampleBufferIsValid(sampleBuffer);
                // Add diagnostic logging
                Console.WriteLine($"Sample buffer is valid: {valid}");

                if (!valid)
                {
                    CFRelease(sampleBuffer);
                    return;
                }
                

                // Optionally, inspect attachments to determine if any configuration might be missing
                var attachments = CMSampleBufferGetSampleAttachmentsArray(sampleBuffer, false);
                Console.WriteLine($"[Debug] Attachments present: {(attachments != IntPtr.Zero ? "Yes" : "No")}");

                // Now try to get the image buffer
                var pixelBuffer = CMSampleBufferGetImageBuffer(sampleBuffer);
                Console.WriteLine($"[Debug] Pixel buffer address: {pixelBuffer}");

                if (pixelBuffer == IntPtr.Zero)
                {
                    Console.WriteLine("[Error] CMSampleBufferGetImageBuffer returned 0x0.");
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

                // Release the sample buffer once done
                CFRelease(sampleBuffer);
            }

        }
    }
}
