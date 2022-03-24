////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Threading.Tasks;

namespace FlashCap.Devices
{
    public sealed class V4L2DeviceDescriptor : CaptureDeviceDescriptor
    {
        private readonly string devicePath;

        internal V4L2DeviceDescriptor(
            string devicePath, string name, string description,
            VideoCharacteristics[] characteristics) :
            base(name, description, characteristics) =>
            this.devicePath = devicePath;

        public override object Identity =>
            this.devicePath;

        public override DeviceTypes DeviceType =>
            DeviceTypes.V4L2;

        public override ICaptureDevice OpenWithFrameProcessor(
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            FrameProcessor frameProcessor) =>
            new V4L2Device(
                devicePath, characteristics, transcodeIfYUV, frameProcessor);

#if NET40_OR_GREATER || NETSTANDARD || NETCOREAPP
        public override Task<ICaptureDevice> OpenWithFrameProcessorAsync(
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            FrameProcessor frameProcessor) =>
            TaskEx.FromResult(this.OpenWithFrameProcessor(          // TODO:
                characteristics, transcodeIfYUV, frameProcessor));
#endif
    }
}
