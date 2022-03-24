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

        protected override CaptureDevice OnOpenWithFrameProcessor(
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            FrameProcessor frameProcessor) =>
            new DirectShowDevice(
                this.devicePath, characteristics, transcodeIfYUV, frameProcessor);

#if NET35_OR_GREATER || NETSTANDARD || NETCOREAPP
        public override Task<CaptureDevice> OpenWithFrameProcessorAsync(
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            FrameProcessor frameProcessor) =>
            TaskEx.FromResult(this.OnOpenWithFrameProcessor(          // TODO:
                characteristics, transcodeIfYUV, frameProcessor));
#endif
    }
}
