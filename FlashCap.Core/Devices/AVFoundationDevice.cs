using System;
using System.Threading;
using System.Threading.Tasks;
using static FlashCap.Internal.NativeMethods_AVFoundation;

namespace FlashCap.Devices;

public sealed class AVFoundationDevice : CaptureDevice
{
	private readonly string uniqueId;

	private AVCaptureDevice? device;
	private AVCaptureSession? session;
	
	public AVFoundationDevice(string uniqueID, string modelID) :
		base(uniqueID, modelID)
	{
		this.uniqueId = uniqueID;
	}

    protected override Task OnDisposeAsync()
    {
		this.device?.Dispose();
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
					$"FlashCap: Couldn't authorize: UniqueId={this.uniqueId}");
			}
		}

		this.session = new AVCaptureSession();
        this.device = AVCaptureDevice.DeviceWithUniqueID(uniqueId);
    }

    protected override Task OnStartAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    protected override Task OnStopAsync(CancellationToken ct)
    {
    	return Task.CompletedTask;
    }

    protected override void OnCapture(IntPtr pData, int size, long timestampMicroseconds, long frameIndex, PixelBuffer buffer)
    {
    }
}
