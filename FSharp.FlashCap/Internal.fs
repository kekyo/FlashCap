﻿////////////////////////////////////////////////////////////////////////////
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
module internal Internal =
    
    let inline public asCT (ct: CancellationToken option) =
        match ct with
        | Some(ct) -> ct
        | _ -> CancellationToken()
