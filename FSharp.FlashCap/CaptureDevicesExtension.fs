////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace FlashCap

[<AutoOpen>]
module public CaptureDevicesExtension =

    type public CaptureDevices with

        member self.enumerateDescriptors() : seq<_> =
            self.InternalEnumerateDescriptors()

        member self.getDescriptors() =
            self.InternalEnumerateDescriptors() |> Seq.toArray
