////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace FlashCap

open System.Threading
open System

[<AutoOpen>]
module public CaptureDeviceExtension =

    type public CaptureDevice with

        member self.startAsync(?ct: CancellationToken) =
            self.InternalStartAsync(asCT ct) |> Async.AwaitTask

        member self.stopAsync(?ct: CancellationToken) =
            self.InternalStopAsync(asCT ct) |> Async.AwaitTask

        [<Obsolete("start method will be deprecated. Switch to use startAsync method.")>]
        member self.start() =
            self.InternalStartAsync(CancellationToken()) |> ignore

        [<Obsolete("stop method will be deprecated. Switch to use stopAsync method.")>]
        member self.stop() =
            self.InternalStopAsync(CancellationToken()) |> ignore
