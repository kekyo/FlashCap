////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace FlashCap

open System
open System.Threading

[<AutoOpen>]
module public ObservableCaptureDeviceExtension =

    type public ObservableCaptureDevice with

        member self.start(?ct: CancellationToken) =
            self.InternalStartAsync(asCT ct) |> Async.AwaitTask

        member self.stop(?ct: CancellationToken) =
            self.InternalStopAsync(asCT ct) |> Async.AwaitTask

        member self.subscribe(observer: IObserver<PixelBufferScope>) =
            self.InternalSubscribe(observer)
