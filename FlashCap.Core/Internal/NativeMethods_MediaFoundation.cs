#if FLASHCAP_MEDIAFOUNDATION
////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
#if NET8_0_OR_GREATER
using System.Runtime.InteropServices.Marshalling;
#endif

namespace FlashCap.Internal;

[SupportedOSPlatform("windows6.1")]
public static partial class NativeMethods_MediaFoundation
{
#if NET8_0_OR_GREATER
    [GeneratedComInterface]
#else
    [ComImport]
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
#endif
    [Guid("DEEC8D99-FA1D-4D82-84C2-2C8969944867")]
    public partial interface IMFSourceReaderCallbackInterop
    {
        [PreserveSig]
        int OnReadSample(int status, uint streamIndex, uint streamFlags, long timestamp, IntPtr sample);

        [PreserveSig]
        int OnFlush(uint streamIndex);

        [PreserveSig]
        int OnEvent(uint streamIndex, IntPtr mediaEvent);
    }
    public static IntPtr GetComInterfaceForObject(IMFSourceReaderCallbackInterop callback)
    {
#if NET8_0_OR_GREATER
        return ComWrappers.GetOrCreateComInterfaceForObject(callback, CreateComInterfaceFlags.None);
#else
        return Marshal.GetComInterfaceForObject(callback, typeof(IMFSourceReaderCallbackInterop));
#endif
    }

    public static void ReleaseComInterface(IntPtr pointer) => _ = Marshal.Release(pointer);

#if NET8_0_OR_GREATER
    private static readonly StrategyBasedComWrappers ComWrappers = new();
#endif
}
#endif