////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;

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

        public override unsafe ICaptureDevice Open(
            VideoCharacteristics characteristics,
            bool transcodeIfYUV = true)
        {
            var handle = NativeMethods_VideoForWindows.CreateVideoSourceWindow(this.index);

            return new VideoForWindowsDevice(
                handle, this.index, characteristics, transcodeIfYUV);
        }
    }
}
