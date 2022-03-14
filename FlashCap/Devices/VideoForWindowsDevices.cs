////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FlashCap.Devices
{
    public sealed class VideoForWindowsDevices : ICaptureDevices
    {
        private static unsafe VideoCharacteristics GetCharacteristics(int index)
        {
            var handle = NativeMethods_VideoForWindows.CreateVideoSourceWindow(index);
            try
            {
                NativeMethods_VideoForWindows.capDriverConnect(handle, index);

                // Try to set 30fps, but VFW API may cause ignoring it silently...
                NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out var cp);
                cp.dwRequestMicroSecPerFrame = (int)(1_000_000.0 / 30.0);
                NativeMethods_VideoForWindows.capCaptureSetSetup(handle, cp);
                NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out cp);

                NativeMethods_VideoForWindows.capGetVideoFormat(handle, out var pih);
                try
                {
                    var pBih = (NativeMethods.RAW_BITMAPINFOHEADER*)pih.ToPointer();
                    var characteristics = NativeMethods.CreateVideoCharacteristics(
                        pih, (int)(1_000_000_000.0 / cp.dwRequestMicroSecPerFrame));
                    return characteristics;
                }
                finally
                {
                    Marshal.FreeCoTaskMem(pih);
                }
            }
            finally
            {
                NativeMethods_VideoForWindows.capDriverDisconnect(handle, index);
                NativeMethods_VideoForWindows.DestroyWindow(handle);
            }
        }

        public IEnumerable<ICaptureDeviceDescriptor> EnumerateDescriptors() =>
            Enumerable.Range(0, NativeMethods_VideoForWindows.MaxVideoForWindowsDevices).
            Collect(index =>
            {
                var name = new StringBuilder(256);
                var description = new StringBuilder(256);

                if (NativeMethods_VideoForWindows.capGetDriverDescription(
                    (uint)index, name, name.Length, description, description.Length))
                {
                    var n = name.ToString().Trim();
                    var d = description.ToString().Trim();

                    return (ICaptureDeviceDescriptor)new VideoForWindowsDeviceDescriptor(   // Requires casting on net20
                        index,
                        string.IsNullOrEmpty(n) ? "Default" : n,
                        string.IsNullOrEmpty(d) ? "VideoForWindows default" : d,
                        new[] {
                            GetCharacteristics(index),
                        });
                }
                else
                {
                    return null;
                }
            });
    }
}
