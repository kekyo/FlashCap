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

namespace FlashCap.Devices
{
    public sealed class VideoForWindowsDevices : ICaptureDevices
    {
        public IEnumerable<CaptureDeviceDescription> Descriptions
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
                        var n = name.ToString().Trim();
                        var d = description.ToString().Trim();

                        yield return new CaptureDeviceDescription(
                            index, n, d);
                    }
                }
            }
        }

        public VideoForWindowsDevice Open(
            CaptureDeviceDescription description, bool holdRawData = false)
        {
            var identity = (int)description.Identity;
            var handle = NativeMethods.capCreateCaptureWindow(
                $"FlashCap_{identity}", NativeMethods.WS_POPUPWINDOW,
                -100, -100, 10, 10, IntPtr.Zero, 0);
            if (handle == IntPtr.Zero)
            {
                var code = Marshal.GetLastWin32Error();
                Marshal.ThrowExceptionForHR(code);
            }

            var extyles = NativeMethods.GetWindowLong(
                handle,
                NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(
                handle,
                NativeMethods.GWL_EXSTYLE,
                extyles | NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_TRANSPARENT);

            return new VideoForWindowsDevice(handle, identity, holdRawData);
        }

        ICaptureDevice ICaptureDevices.Open(
            CaptureDeviceDescription description, bool holdRawData) =>
            this.Open(description, holdRawData);
    }
}
