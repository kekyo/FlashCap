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
using System.Collections.Generic;
using System.Reflection;
#if !(NET35 || NET40 || NET45 || NETSTANDARD1_3)
using System.Threading;
#endif

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
            var hasAsyncEnumeration = typeof(CaptureDevices).GetMethod(
                "OnEnumerateDescriptorsAsync", BindingFlags.Instance | BindingFlags.NonPublic) != null;
#if NET35 || NET40 || NET45
            if (hasAsyncEnumeration) return 1;
#else
            if (!hasAsyncEnumeration) return 1;
            var enumerator = new EmptyDevices().GetDescriptorsAsync().GetAsyncEnumerator();
            try
            {
                if (enumerator.MoveNextAsync().GetAwaiter().GetResult()) return 1;
            }
            finally
            {
                enumerator.DisposeAsync().GetAwaiter().GetResult();
            }
#endif
            return MediaFoundationTypes.Length == 3 ? 0 : 1;
        }
#endif

#if !(NET35 || NET40 || NET45 || NETSTANDARD1_3)
        private sealed class EmptyDevices : CaptureDevices
        {
            protected override IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors() =>
                new CaptureDeviceDescriptor[0];

            public IAsyncEnumerable<CaptureDeviceDescriptor> GetDescriptorsAsync() =>
                base.OnEnumerateDescriptorsAsync(CancellationToken.None);
        }
#endif
    }
}
