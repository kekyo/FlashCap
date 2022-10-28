////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace FlashCap

open System

[<AutoOpen>]
module public ObservableCaptureDeviceExtension =

    type public ObservableCaptureDevice with

        member self.start() =
            self.InternalStart()

        member self.stop() =
            self.InternalStop()

        member self.subscribe(observer: IObserver<PixelBufferScope>) =
            self.InternalSubscribe(observer)
