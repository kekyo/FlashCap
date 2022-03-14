////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System.Linq;

namespace FlashCap.Devices
{
    public sealed class DirectShowDeviceDescriptor : CaptureDeviceDescriptor
    {
        private readonly string devicePath;

        internal DirectShowDeviceDescriptor(
            string devicePath, string name, string description,
            VideoCharacteristics[] characteristics) :
            base(name, description, characteristics) =>
            this.devicePath = devicePath;

        public override object Identity =>
            this.devicePath;

        public override DeviceTypes DeviceType =>
            DeviceTypes.DirectShow;

        public override ICaptureDevice Open(
            VideoCharacteristics characteristics,
            bool transcodeIfYUV = true)
        {
            if (NativeMethods_DirectShow.EnumerateDeviceMoniker(
                NativeMethods_DirectShow.CLSID_VideoInputDeviceCategory).
                Where(moniker =>
                    moniker.GetPropertyBag() is { } pb &&
                    pb.SafeReleaseBlock(pb =>
                        pb.GetValue("DevicePath", default(string))?.Trim() is { } devicePath &&
                        devicePath.Equals(this.devicePath))).
                Collect(moniker =>
                    moniker.BindToObject(null, null, in NativeMethods_DirectShow.IID_IBaseFilter, out var bf) == 0 ?
                    bf as NativeMethods_DirectShow.IBaseFilter : null).
                FirstOrDefault() is { } baseFilter)
            {
                baseFilter.SafeReleaseBlock(baseFilter =>
                {
                });
            }

            return null!;
        }
    }
}
