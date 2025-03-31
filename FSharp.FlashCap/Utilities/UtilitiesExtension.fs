////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace FlashCap.Utilities

open System
open System.IO
open System.Runtime.CompilerServices

#nowarn 3261

[<Extension>]
type UtilitiesExtension =
    
    [<Extension>]
    static member asStream(self: ArraySegment<byte>) =
        match self.Array with
        | null -> new MemoryStream(ArrayEx.Empty<byte>())
        | _ -> new MemoryStream(self.Array, self.Offset, self.Count)

    [<Extension>]
    static member asStream(self: byte[]) =
        match self with
        | null -> new MemoryStream(ArrayEx.Empty<byte>())
        | _ -> new MemoryStream(self)
