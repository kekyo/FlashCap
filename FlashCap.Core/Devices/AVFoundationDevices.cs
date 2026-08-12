////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Yoh Deadfall (@YohDeadfall)
// Copyright (c) Felipe Ferreira Quintella (@ffquintella)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using FlashCap.Internal;
using FlashCap.Utilities;
using static FlashCap.Internal.AVFoundation.LibAVFoundation;

namespace FlashCap.Devices;

[SupportedOSPlatform("macos")]
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
    
    [UnconditionalSuppressMessage("Trimming", "IL2046", Justification = "This backend does not require unreferenced code")]
    protected override IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors()
    {
        if (!NativeMethods.IsMacOS())
        {
            throw new PlatformNotSupportedException("AVFoundation capture requires macOS.");
        }

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
                try
                {
                    device.ActiveFormat = format;
                }
                finally
                {
                    device.UnlockForConfiguration();
                }

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

            yield return new AVFoundationDeviceDescriptor(
                device.UniqueID,
                device.ModelID,
                device.LocalizedName,
                characteristics.ToArray(), 
                this.DefaultBufferPool);
        }
    }
}
