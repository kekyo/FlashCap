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
        public readonly object Identity;
        public readonly string Name;
        public readonly string Description;

        public CaptureDeviceDescription(
            object identity, string name, string description)
        {
            this.Identity = identity;
            this.Name = name;
            this.Description = description;
        }

        public override string ToString() =>
            $"{this.Identity}: {this.Name}: {this.Description}";
    }
}
