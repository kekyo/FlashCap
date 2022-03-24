////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Linq;

namespace FlashCap
{
    public static class CaptureDevicesExtension
    {
        public static CaptureDeviceDescriptor[] GetDescriptors(
            this CaptureDevices captureDevices) =>
            captureDevices.EnumerateDescriptors().ToArray();
    }
}
