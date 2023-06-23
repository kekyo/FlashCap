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
using FlashCap.Internal;
using FlashCap.Utilities;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibAVFoundation;
using static FlashCap.Internal.NativeMethods_AVFoundation.LibCoreVideo;

namespace FlashCap.Devices;

public sealed class AVFoundationDevices : CaptureDevices
{
    protected override IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors()
    {
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

                            var frameDurationRange = format.VideoSupportedFrameRateRanges;
                            var frameMinDuration = frameDurationRange.MinFrameDuration;
                            var frameMaxDuration = frameDurationRange.MaxFrameDuration;

                            var minFps = new Fraction((int)frameMinDuration.Value, frameMinDuration.TimeScale);
                            var maxFps = new Fraction((int)frameMaxDuration.Value, frameMaxDuration.TimeScale);

                            return NativeMethods.DefactoStandardFramesPerSecond
                                .Where(fps => fps >= minFps && fps <= maxFps)
                                .OrderByDescending(fps => fps)
                                .SelectMany(fps =>
                                    availablePixelFormats.Select(
                                        format => new VideoCharacteristics(format.Key, dimensions.Width, dimensions.Height, fps, format.Value, isDiscrete: true, format.Value)));
                        })
                        .ToArray());
            });
    }
}
