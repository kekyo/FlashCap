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

[<AutoOpen>]
module public CaptureDevicesExtension =

    type public CaptureDevices with

        member self.enumerateDescriptors() : seq<_> =
            self.InternalEnumerateDescriptors()

        member self.getDescriptors() =
            self.InternalEnumerateDescriptors() |> Seq.toArray

        /// Enumerates available descriptors asynchronously; ct cancels enumeration.
        member self.enumerateDescriptorsAsync(?ct: CancellationToken) =
            self.InternalEnumerateDescriptorsAsync(asCT ct)
