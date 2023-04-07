using FlashCap.Internal;

namespace FlashCap.Devices
{
    public sealed class DirectShowProperty : CaptureDeviceProperty
    {
        public DirectShowProperty(VideoProcessingAmplifierProperty property, int min, int max, int step) : base(property, min, max, step) { }

        internal static NativeMethods_DirectShow.VideoProcAmpProperty FromVideoProcessingAmplifierProperty(VideoProcessingAmplifierProperty property)
        {
            return (NativeMethods_DirectShow.VideoProcAmpProperty)(int)property;
        }
    }
}
