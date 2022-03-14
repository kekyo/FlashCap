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
    public enum DeviceTypes
    {
        VideoForWindows,
        DirectShow,
        V2L2,  // TODO:
    }

    public interface ICaptureDeviceDescriptor
    {
        object Identity { get; }
        DeviceTypes DeviceType { get; }
        string Name { get; }
        string Description { get; }
        VideoCharacteristics[] Characteristics { get; }

        ICaptureDevice Open(
            VideoCharacteristics characteristics,
            bool transcodeIfYUV = true);
    }

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

        public abstract ICaptureDevice Open(
            VideoCharacteristics characteristics,
            bool transcodeIfYUV = true);

        public override string ToString() =>
            $"{this.Name}: {this.Description}, Characteristics={this.Characteristics.Length}";
    }
}
