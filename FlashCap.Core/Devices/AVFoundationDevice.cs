using System;
using System.Threading;
using System.Threading.Tasks;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibAVFoundation;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibC;

namespace FlashCap.Devices;

public sealed class AVFoundationDevice : CaptureDevice
{
    private readonly string uniqueID;

    private AVCaptureDevice? device;
    private AVCaptureDeviceInput? deviceInput;
    private AVCaptureVideoDataOutput? deviceOutput;
    private AVCaptureSession? session;
    private FrameProcessor? frameProcessor;
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

        if (frameProcessor is { })
        {
            await frameProcessor.DisposeAsync().ConfigureAwait(false);
        }
        
        await base.OnDisposeAsync().ConfigureAwait(false);
    }

    protected override async Task OnInitializeAsync(VideoCharacteristics characteristics, bool transcodeIfYUV, FrameProcessor frameProcessor, CancellationToken ct)
    {
        this.frameProcessor = frameProcessor;
        this.transcodeIfYUV = transcodeIfYUV;

        const string MediaType = "Video";

        if (AVCaptureDevice.GetAuthorizationStatus(MediaType) != AVAuthorizationStatus.Authorized)
        {
            var tcs = new TaskCompletionSource<bool>();

            AVCaptureDevice.RequestAccessForMediaType(MediaType, authorized => tcs.SetResult(authorized));

            if (!await tcs.Task)
            {
                throw new InvalidOperationException(
                    $"FlashCap: Couldn't authorize: UniqueId={this.uniqueID}");
            }
        }

        this.device = AVCaptureDevice.DeviceWithUniqueID(uniqueID);

        if (this.device is null)
        {
            throw new InvalidOperationException(
                $"FlashCap: Couldn't find device: UniqueID={this.uniqueID}");
        }

        this.deviceInput = new AVCaptureDeviceInput(this.device);
        this.deviceOutput = new AVCaptureVideoDataOutput();
        this.deviceOutput.SetSampleBufferDelegate(
            new VideoBufferHandler(this),
            GetGlobalQueue(DispatchQualityOfService.Background, flags: default));

        this.session = new AVCaptureSession();
        this.session.AddInput(this.deviceInput);
        this.session.AddOutput(this.deviceOutput);
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
        buffer.CopyIn(/* this.pBih */ default, pData, size, timestampMicroseconds, frameIndex, this.transcodeIfYUV);
    }

    private sealed class VideoBufferHandler : AVCaptureVideoDataOutputSampleBuffer
    {
        private AVFoundationDevice device;

        public VideoBufferHandler(AVFoundationDevice device)
        {
            this.device = device;
        }

        public override void DidDropSampleBuffer(IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection)
        {
        }

        public override void DidOutputSampleBuffer(IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection)
        {
            this.device.frameProcessor?.OnFrameArrived(this.device, default, default, default, default);
        }
    }
}
