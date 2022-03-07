////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace FlashCap
{
    public sealed class CaptureDeviceDescription
    {
        public readonly int DeviceId;
        public readonly string Name;
        public readonly string Description;

        public CaptureDeviceDescription(
            int deviceId, string name, string description)
        {
            this.DeviceId = deviceId;
            this.Name = name;
            this.Description = description;
        }

        public CaptureDevice Open() =>
            CaptureDevices.Open(this.DeviceId);

        public override string ToString() =>
            $"{this.DeviceId}: {this.Name}: {this.Description}";
    }
}
