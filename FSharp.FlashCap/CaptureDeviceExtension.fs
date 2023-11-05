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

        member self.start(?ct: CancellationToken) =
            self.InternalStartAsync(asCT ct) |> Async.AwaitTask

        member self.stop(?ct: CancellationToken) =
            self.InternalStopAsync(asCT ct) |> Async.AwaitTask

        [<Obsolete("This function is obsoleted, please use `start` instead.")>]
        member self.startAsync(?ct: CancellationToken) =
            self.InternalStartAsync(asCT ct) |> Async.AwaitTask

        [<Obsolete("This function is obsoleted, please use `stop` instead.")>]
        member self.stopAsync(?ct: CancellationToken) =
            self.InternalStopAsync(asCT ct) |> Async.AwaitTask
