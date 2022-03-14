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
using System.Runtime.InteropServices;

namespace FlashCap.Devices
{
    public sealed class VideoForWindowsDeviceDescriptor : CaptureDeviceDescriptor
    {
        private readonly int index;

        internal VideoForWindowsDeviceDescriptor(
            int index, string name, string description,
            VideoCharacteristics[] characteristics) :
            base(name, description, characteristics) =>
            this.index = index;

        public override object Identity =>
            this.index;

        public override DeviceTypes DeviceType =>
            DeviceTypes.VideoForWindows;

        public override ICaptureDevice Open(
            VideoCharacteristics characteristics,
            bool transcodeIfYUV = true)
        {
            // HACK: VFW couldn't operate without any attached window resources.
            //   * It's hider for moving outsite of desktop.
            //   * And will make up transparent tool window.
            var handle = NativeMethods_VideoForWindows.capCreateCaptureWindow(
                $"FlashCap_{this.index}", NativeMethods_VideoForWindows.WS_POPUPWINDOW,
                -100, -100, 10, 10, IntPtr.Zero, 0);
            if (handle == IntPtr.Zero)
            {
                var code = Marshal.GetLastWin32Error();
                Marshal.ThrowExceptionForHR(code);
            }

            var extyles = NativeMethods_VideoForWindows.GetWindowLong(
                handle,
                NativeMethods_VideoForWindows.GWL_EXSTYLE);
            NativeMethods_VideoForWindows.SetWindowLong(
                handle,
                NativeMethods_VideoForWindows.GWL_EXSTYLE,
                extyles | NativeMethods_VideoForWindows.WS_EX_TOOLWINDOW | NativeMethods_VideoForWindows.WS_EX_TRANSPARENT);

            return new VideoForWindowsDevice(
                handle, this.index, characteristics, transcodeIfYUV);
        }
    }
}
