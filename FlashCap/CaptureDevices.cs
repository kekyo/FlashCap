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
using System.Runtime.InteropServices;
using System.Text;

namespace FlashCap
{
    public static class CaptureDevices
    {
        public static CaptureDevice Open(int deviceId)
        {
            var handle = NativeMethods.capCreateCaptureWindow(
                $"FlashCap_{deviceId}", NativeMethods.WS_POPUPWINDOW,
                0, 0, 640, 480, IntPtr.Zero, 0);
            if (handle == IntPtr.Zero)
            {
                var code = Marshal.GetLastWin32Error();
                Marshal.ThrowExceptionForHR(code);
            }

            return new CaptureDevice(handle, deviceId);
        }

        public static IEnumerable<CaptureDeviceDescription> Devices
        {
            get
            {
                for (var index = 0; index < NativeMethods.MaxDevices; index++)
                {
                    var name = new StringBuilder(256);
                    var description = new StringBuilder(256);

                    if (NativeMethods.capGetDriverDescription(
                        (uint)index, name, name.Length, description, description.Length))
                    {
                        yield return new CaptureDeviceDescription(
                            index, name.ToString(), description.ToString());
                    }
                }
            }
        }
    }
}
