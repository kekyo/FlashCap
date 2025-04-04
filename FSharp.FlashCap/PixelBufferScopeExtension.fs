﻿////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace FlashCap

open System.Runtime.CompilerServices

[<AutoOpen>]
module public PixelBufferScopeExtension =

    type public PixelBufferScope with

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member self.releaseNow() =
            self.InternalReleaseNow()
