////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;

namespace FlashCap
{
    public static class ObservableCaptureDeviceExtension
    {
        public static void Start(this ObservableCaptureDevice observableCaptureDevice) =>
            observableCaptureDevice.InternalStart();

        public static void Stop(this ObservableCaptureDevice observableCaptureDevice) =>
            observableCaptureDevice.InternalStop();

        public static IDisposable Subscribe(
            this ObservableCaptureDevice observableCaptureDevice,
            IObserver<PixelBufferScope> observer) =>
            observableCaptureDevice.InternalSubscribe(observer);
    }
}
