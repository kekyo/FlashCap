////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Yoh Deadfall (@YohDeadfall)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using static FlashCap.Internal.NativeMethods_AVFoundation;

namespace FlashCap.Devices;

public sealed class AVFoundationDevices : CaptureDevices
{
    protected override IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors()
    {
        using var device = AVCaptureDevice.GetDefaultDevice("Video");
        return device is null
            ? Enumerable.Empty<AVFoundationDeviceDescriptor>()
            : new[] { new AVFoundationDeviceDescriptor(device.UniqueID, device.ModelID, device.LocalizedName, Array.Empty<VideoCharacteristics>()) };
    }
}
