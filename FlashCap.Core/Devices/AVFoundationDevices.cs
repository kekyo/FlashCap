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
using static FlashCap.Internal.NativeMethods_AVFoundation;

namespace FlashCap.Devices;

public sealed class AVFoundationDevices : CaptureDevices
{
    protected override IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors()
    {
        using var device = AVCaptureDevice.GetDefaultDevice("Video");
        return device is null
            ? Enumerable.Empty<AVFoundationDeviceDescriptor>()
            : new[]
            {
                new AVFoundationDeviceDescriptor(
                    device.UniqueID,
                    device.ModelID,
                    device.LocalizedName,
                    device.Formats
                        .SelectMany(static format =>
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
                                .Select(fps => new VideoCharacteristics(default, dimensions.Width, dimensions.Height, fps));
                        })
                        .ToArray())
            };
    }
}
