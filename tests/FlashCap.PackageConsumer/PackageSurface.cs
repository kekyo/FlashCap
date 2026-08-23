////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Devices;
using System;

namespace FlashCap.PackageConsumer
{
    public static class PackageSurface
    {
        public static readonly Type[] MediaFoundationTypes =
        {
            typeof(MediaFoundationDevices),
            typeof(MediaFoundationDeviceDescriptor),
            typeof(MediaFoundationDevice),
        };

#if NETFRAMEWORK
        public static int Main()
        {
            _ = new MediaFoundationDevices();
            return MediaFoundationTypes.Length == 3 ? 0 : 1;
        }
#endif
    }
}
