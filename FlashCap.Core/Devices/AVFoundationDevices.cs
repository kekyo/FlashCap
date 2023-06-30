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
using static FlashCap.Internal.NativeMethods_AVFoundation.LibCoreVideo;

namespace FlashCap.Devices;

public sealed class AVFoundationDevices : CaptureDevices
{
    protected override IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors()
    {
        if (AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video) != AVAuthorizationStatus.Authorized)
        {
            TaskCompletionSource<bool> tcs = new();
            AVCaptureDevice.RequestAccessForMediaType(AVMediaType.Video, status => tcs.SetResult(status));

            tcs.Task.GetAwaiter().GetResult();
        }

        using var discovery = AVCaptureDeviceDiscoverySession.DiscoverySessionWithVideoDevices();
        return discovery.Devices
            .Select(static device =>
            {
                using var deviceInput = new AVCaptureDeviceInput(device);
                using var deviceOutput = new AVCaptureVideoDataOutput();

                using var session = new AVCaptureSession();

                session.AddInput(deviceInput);
                session.AddOutput(deviceOutput);

                var supportedPixelFormats = NativeMethods_AVFoundation.PixelFormatMap;
                var availablePixelFormats = (
                    from available in deviceOutput.AvailableVideoCVPixelFormatTypes
                    join supported in supportedPixelFormats on available equals supported.Value into temp
                    from supported in temp.Select(format => format.Key).DefaultIfEmpty(PixelFormats.Unknown)
                    select new KeyValuePair<PixelFormats, string>(supported, GetFourCCName(available)))
                    .ToArray();

                return new AVFoundationDeviceDescriptor(
                    device.UniqueID,
                    device.ModelID,
                    device.LocalizedName,
                    device.Formats
                        .SelectMany(format =>
                        {
                            var description = format.FormatDescription;
                            var dimensions = description.Dimensions;

                            return format.VideoSupportedFrameRateRanges
                                .SelectMany(frameDurationRange =>
                                {
                                    var frameMinDuration = frameDurationRange.MinFrameDuration;
                                    var frameMaxDuration = frameDurationRange.MaxFrameDuration;

                                    var minFps = new Fraction(frameMinDuration.TimeScale, (int)frameMinDuration.Value);
                                    var maxFps = new Fraction(frameMaxDuration.TimeScale, (int)frameMaxDuration.Value);

                                    return NativeMethods.DefactoStandardFramesPerSecond
                                        .Where(fps => fps >= minFps && fps <= maxFps)
                                        .OrderByDescending(fps => fps)
                                        .SelectMany(fps =>
                                            availablePixelFormats.Select(
                                                format => new VideoCharacteristics(format.Key, dimensions.Width, dimensions.Height, fps, format.Value, isDiscrete: true, format.Value)));
                                });
                        })
                        .ToArray());
            });
    }
}
