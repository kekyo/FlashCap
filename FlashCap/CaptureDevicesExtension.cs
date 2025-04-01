////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

namespace FlashCap;

public static class CaptureDevicesExtension
{
    public static IEnumerable<CaptureDeviceDescriptor> EnumerateDescriptors(
        this CaptureDevices captureDevices) =>
        captureDevices.InternalEnumerateDescriptors();

    public static CaptureDeviceDescriptor[] GetDescriptors(
        this CaptureDevices captureDevices) =>
        captureDevices.InternalEnumerateDescriptors().ToArray();
}
