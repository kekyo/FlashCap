////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
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

        member self.showPropertyPage(parentWindow: nativeint, ?ct: CancellationToken) =
            self.InternalShowPropertyPageAsync(parentWindow, asCT ct) |> Async.AwaitTask
