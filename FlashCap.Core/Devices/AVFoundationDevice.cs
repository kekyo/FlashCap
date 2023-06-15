using System;
using System.Threading;
using System.Threading.Tasks;
using static FlashCap.Internal.NativeMethods_AVFoundation;

namespace FlashCap.Devices;

public sealed class AVFoundationDevice : CaptureDevice
{
    private readonly string uniqueID;

    private AVCaptureDevice? device;
    private AVCaptureDeviceInput? deviceInput;
    private AVCaptureDeviceOutput? deviceOutput;
    private AVCaptureSession? session;

    public AVFoundationDevice(string uniqueID, string modelID) :
        base(uniqueID, modelID)
    {
        this.uniqueID = uniqueID;
    }

    protected override Task OnDisposeAsync()
    {
        this.device?.Dispose();
        this.deviceInput?.Dispose();
        this.deviceOutput?.Dispose();
        this.session?.Dispose();

        return base.OnDisposeAsync();
    }

    protected override async Task OnInitializeAsync(VideoCharacteristics characteristics, bool transcodeIfYUV, FrameProcessor frameProcessor, CancellationToken ct)
    {
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

        this.session = new AVCaptureSession();
        this.device = AVCaptureDevice.DeviceWithUniqueID(uniqueID);

        if (this.device is null)
        {
            throw new InvalidOperationException(
                $"FlashCap: Couldn't find device: UniqueID={this.uniqueID}");
        }

        this.deviceInput = new AVCaptureDeviceInput(this.device);
        this.deviceOutput = new AVCaptureDeviceOutput();
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
    }
}
