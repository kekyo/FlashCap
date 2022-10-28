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
module public CaptureDeviceExtension =

    type public CaptureDevice with

        member self.start() =
            self.InternalStart()

        member self.stop() =
            self.InternalStop()
