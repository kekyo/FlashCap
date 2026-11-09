////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace FlashCap;

/// <summary>
/// Kind of a camera control, such as the brightness or the zoom.
/// </summary>
public enum CaptureDeviceControlTypes
{
    Brightness,
    Contrast,
    Hue,
    Saturation,
    Sharpness,
    Gamma,
    /// <summary>
    /// White balance as a color temperature in kelvins.
    /// </summary>
    WhiteBalance,
    BacklightCompensation,
    Gain,
    /// <summary>
    /// Anti flicker filter: 0 disabled, 1 50 Hz, 2 60 Hz, 3 auto (V4L2 only).
    /// </summary>
    PowerLineFrequency,
    Pan,
    Tilt,
    Zoom,
    /// <summary>
    /// Exposure time in the native unit of the device:
    /// the binary logarithm of seconds on DirectShow and Media Foundation, 100 µs on V4L2.
    /// </summary>
    Exposure,
    Focus,
}

/// <summary>
/// Camera control the device offers, with its range in the native unit of the device.
/// </summary>
public sealed class CaptureDeviceControl
{
    public readonly CaptureDeviceControlTypes Type;
    public readonly int Minimum;
    public readonly int Maximum;
    public readonly int Step;
    public readonly int DefaultValue;
    public readonly bool SupportsAuto;

    public CaptureDeviceControl(
        CaptureDeviceControlTypes type,
        int minimum, int maximum, int step, int defaultValue,
        bool supportsAuto)
    {
        this.Type = type;
        this.Minimum = minimum;
        this.Maximum = maximum;
        this.Step = step;
        this.DefaultValue = defaultValue;
        this.SupportsAuto = supportsAuto;
    }

    public override string ToString() =>
        $"{this.Type}: {this.Minimum}..{this.Maximum}, step={this.Step}, default={this.DefaultValue}{(this.SupportsAuto ? ", auto" : "")}";
}

/// <summary>
/// Current value of a camera control.
/// </summary>
public sealed class CaptureDeviceControlValue
{
    public readonly int Value;
    public readonly bool IsAuto;

    public CaptureDeviceControlValue(int value, bool isAuto)
    {
        this.Value = value;
        this.IsAuto = isAuto;
    }

    public override string ToString() =>
        this.IsAuto ? $"{this.Value} (auto)" : $"{this.Value}";
}
