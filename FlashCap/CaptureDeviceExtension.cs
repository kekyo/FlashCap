////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace FlashCap;

public static class CaptureDeviceExtension
{
    public static void Start(this CaptureDevice captureDevice) =>
        captureDevice.InternalStart();

    public static void Stop(this CaptureDevice captureDevice) =>
        captureDevice.InternalStop();
}
