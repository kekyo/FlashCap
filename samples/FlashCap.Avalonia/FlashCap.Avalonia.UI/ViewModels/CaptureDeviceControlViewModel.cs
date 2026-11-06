////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using Epoxy;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FlashCap.Avalonia.ViewModels;

// A choice of a camera control with named values.
public sealed class CaptureDeviceControlChoice
{
    public CaptureDeviceControlChoice(int value, string name)
    {
        this.Value = value;
        this.Name = name;
    }

    public int Value { get; }
    public string Name { get; }

    public override string ToString() =>
        this.Name;
}

// A camera control shown as a slider, a check box or a choice, with an auto check box.
[ViewModel]
public sealed class CaptureDeviceControlViewModel
{
    // Values of the power line frequency, the anti flicker filter.
    private static readonly CaptureDeviceControlChoice[] powerLineFrequencies =
    {
        new(0, "Disabled"),
        new(1, "50 Hz"),
        new(2, "60 Hz"),
        new(3, "Auto"),
    };

    private readonly CaptureDevice captureDevice;
    private readonly CaptureDeviceControlTypes type;
    private bool isUpdating;

    public CaptureDeviceControlViewModel(
        CaptureDevice captureDevice,
        CaptureDeviceControl control,
        CaptureDeviceControlValue? value)
    {
        this.captureDevice = captureDevice;
        this.type = control.Type;

        this.Name = control.Type.ToString();
        this.Minimum = control.Minimum;
        this.Maximum = control.Maximum;
        this.Step = Math.Max(1, control.Step);
        this.SupportsAuto = control.SupportsAuto;

        // Show each control by its kind of values.
        this.Choices = control.Type == CaptureDeviceControlTypes.PowerLineFrequency ?
            powerLineFrequencies.Where(c => c.Value >= control.Minimum && c.Value <= control.Maximum).ToArray() :
            new CaptureDeviceControlChoice[0];
        this.IsChoice = this.Choices.Length >= 1;
        this.IsSwitch = !this.IsChoice && control.Minimum == 0 && control.Maximum == 1;
        this.IsSlider = !this.IsChoice && !this.IsSwitch;

        this.Update(value ?? new CaptureDeviceControlValue(control.DefaultValue, false));
    }

    // Binding members.
    public string Name { get; }
    public double Minimum { get; }
    public double Maximum { get; }
    public double Step { get; }
    public bool SupportsAuto { get; }
    public bool IsSlider { get; }
    public bool IsSwitch { get; }
    public bool IsChoice { get; }
    public CaptureDeviceControlChoice[] Choices { get; }

    public double Value { get; set; }
    public bool IsOn { get; set; }
    public CaptureDeviceControlChoice? Choice { get; set; }
    public bool IsAuto { get; set; }
    public bool IsManual { get; private set; } = true;

    // Slider was moved, the device gets the value rounded to its step.
    [PropertyChanged(nameof(Value))]
    private ValueTask OnValueChangedAsync(double value) =>
        this.SetAsync((int)(this.Minimum + Math.Round((value - this.Minimum) / this.Step) * this.Step), false);

    // Check box was changed.
    [PropertyChanged(nameof(IsOn))]
    private ValueTask OnIsOnChangedAsync(bool isOn) =>
        this.SetAsync(isOn ? 1 : 0, false);

    // Choice was selected.
    [PropertyChanged(nameof(Choice))]
    private ValueTask OnChoiceChangedAsync(CaptureDeviceControlChoice? choice) =>
        choice is { } ? this.SetAsync(choice.Value, false) : default;

    // Auto check box was changed.
    [PropertyChanged(nameof(IsAuto))]
    private ValueTask OnIsAutoChangedAsync(bool isAuto) =>
        this.SetAsync((int)Math.Round(this.Value), isAuto);

    private async ValueTask SetAsync(int value, bool isAuto)
    {
        if (this.isUpdating)
        {
            return;
        }

        // Setting a value turns the automatic mode off.
        await this.captureDevice.SetControlValueAsync(this.type, value, isAuto);

        // Show the value the device really has.
        if (await this.captureDevice.GetControlValueAsync(this.type) is { } current &&
            await UIThread.TryBind())
        {
            this.Update(current);
        }
    }

    private void Update(CaptureDeviceControlValue value)
    {
        this.isUpdating = true;
        try
        {
            this.Value = value.Value;
            this.IsOn = value.Value != 0;
            this.Choice = this.Choices.FirstOrDefault(c => c.Value == value.Value);
            this.IsAuto = value.IsAuto;
            this.IsManual = !value.IsAuto;
        }
        finally
        {
            this.isUpdating = false;
        }
    }
}
