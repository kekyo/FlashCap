////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Yoh Deadfall (@YohDeadfall)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlashCap.Internal;
using FlashCap.Utilities;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibAVFoundation;

namespace FlashCap.Devices;

public sealed class AVFoundationDevices : CaptureDevices
{
    
    public AVFoundationDevices() :
        this(new DefaultBufferPool())
    {
    }
    
    public AVFoundationDevices(BufferPool defaultBufferPool) :
        base(defaultBufferPool)
    {
    }
    
    protected override IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors()
    {
        if (AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video) != AVAuthorizationStatus.Authorized)
        {
            TaskCompletionSource<bool> tcs = new();
            AVCaptureDevice.RequestAccessForMediaType(AVMediaType.Video, status => tcs.SetResult(status));

            tcs.Task.GetAwaiter().GetResult();
        }

        using var discovery = AVCaptureDeviceDiscoverySession.DiscoverySessionWithVideoDevices();
        foreach (var device in discovery.Devices)
        {
   
            using var deviceOutput = new AVCaptureVideoDataOutput();

            var characteristics = new List<VideoCharacteristics>();

            foreach (var format in device.Formats)
            {
                device.LockForConfiguration();
                device.ActiveFormat = format;
                device.UnlockForConfiguration();

                var pixelFormatsNative = deviceOutput.AvailableVideoCVPixelFormatTypes;
                var pixelFormatsMapped = NativeMethods_AVFoundation.PixelFormatMap
                    .Where(pair => pixelFormatsNative.Contains(pair.Value));

                foreach (var pixelFormat in pixelFormatsMapped)
                {
                    var description = format.FormatDescription;
                    var dimensions = description.Dimensions;

                    foreach (var frameDurationRange in format.VideoSupportedFrameRateRanges)
                    {
                        var frameMinDuration = frameDurationRange.MinFrameDuration;
                        var frameMaxDuration = frameDurationRange.MaxFrameDuration;

                        var minFps = new Fraction(frameMinDuration.TimeScale, (int)frameMinDuration.Value);
                        var maxFps = new Fraction(frameMaxDuration.TimeScale, (int)frameMaxDuration.Value);

                        var availableFps = NativeMethods.DefactoStandardFramesPerSecond
                            .Where(fps => fps >= minFps && fps <= maxFps)
                            .Concat(new[] { minFps, maxFps })
                            .Distinct()
                            .OrderByDescending(fps => fps);

                        foreach (var fps in availableFps)
                        {
                            characteristics.Add(
                                new VideoCharacteristics(
                                    pixelFormat.Key,
                                    dimensions.Width,
                                    dimensions.Height,
                                    fps));
                        }
                    }
                }
            }
            
            // session.Dispose();

            yield return new AVFoundationDeviceDescriptor(
                device.UniqueID.ToString(),
                device.ModelID,
                device.LocalizedName,
                characteristics.ToArray(), 
                this.DefaultBufferPool);
        }
    }
}
