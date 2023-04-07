using System.Collections.Generic;

namespace FlashCap
{
    public enum VideoProcessingAmplifierProperty
    {
        Brightness,
        Contrast,
        Hue,
        Saturation,
        Sharpness,
        Gamma,
        ColorEnable,
        WhiteBalance,
        BacklightCompensation,
        Gain
    }

    public sealed class CaptureDeviceProperties : Dictionary<VideoProcessingAmplifierProperty, CaptureDeviceProperty>
    {
    }

    public abstract class CaptureDeviceProperty
    {
        public VideoProcessingAmplifierProperty Property { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }
        public int Step { get; private set; }

        protected CaptureDeviceProperty(VideoProcessingAmplifierProperty property, int min, int max, int step)
        {
            Property = property;
            Min = min;
            Max = max;
            Step = step;
        }

        internal bool IsPropertyValueValid(object? obj)
        {
            var value = obj as int?;

            return value != null && value <= Max && value >= Min && value % Step == 0;
        }
    }
}
