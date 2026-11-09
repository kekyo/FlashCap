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
    /// Anti flicker filter: 0 disabled, 1 50 Hz, 2 60 Hz, 3 auto.
    /// The available values depend on the device.
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
    /// <summary>
    /// Camera rotation about its viewing axis, in degrees (DirectShow and Media Foundation).
    /// This does not rotate or resize the captured image in software.
    /// </summary>
    Roll,
    /// <summary>
    /// Iris aperture in the device's native unit. Automatic iris and exposure modes may be coupled.
    /// </summary>
    Iris,
    /// <summary>
    /// Color output: 0 black and white, 1 color. On V4L2 this selects the black-and-white or no color effect.
    /// </summary>
    ColorEnable,
    /// <summary>
    /// Allows automatic exposure to reduce the frame rate in low light: 0 fixed frame rate, 1 variable frame rate.
    /// This is effective when exposure is automatic; it does not enable automatic exposure itself.
    /// </summary>
    AutoExposurePriority,
}

/// <summary>
/// Camera control the device offers, with its range in the native unit of the device.
/// </summary>
public sealed class CaptureDeviceControl
{
    /// <summary>The control exposed by the device through the selected backend.</summary>
    public readonly CaptureDeviceControlTypes Type;
    /// <summary>The inclusive lower bound.</summary>
    public readonly int Minimum;
    /// <summary>The inclusive upper bound. Equal bounds represent a fixed value.</summary>
    public readonly int Maximum;
    /// <summary>The native increment between values.</summary>
    public readonly int Step;
    /// <summary>The device default, or the minimum when the driver does not report a usable default.</summary>
    public readonly int DefaultValue;
    /// <summary>Whether the backend reports that automatic and manual operation can be selected.</summary>
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
