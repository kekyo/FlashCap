////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.FrameProcessors;

namespace FlashCap
{
    public abstract class CaptureDeviceDescriptor : ICaptureDeviceDescriptor
    {
        protected CaptureDeviceDescriptor(
            string name, string description,
            VideoCharacteristics[] characteristics)
        {
            this.Name = name;
            this.Description = description;
            this.Characteristics = characteristics;
        }

        public abstract object Identity { get; }
        public abstract DeviceTypes DeviceType { get; }
        public string Name { get; }
        public string Description { get; }
        public VideoCharacteristics[] Characteristics { get; }

        public abstract ICaptureDevice OpenWithFrameProcessor(
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            FrameProcessor frameProcessor);

        public override string ToString() =>
            $"{this.Name}: {this.Description}, Characteristics={this.Characteristics.Length}";
    }
}
