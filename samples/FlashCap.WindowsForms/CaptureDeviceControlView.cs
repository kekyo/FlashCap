////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Windows.Forms;

namespace FlashCap.WindowsForms;

// A camera control shown as a slider, a check box or a choice, with an auto check box.
public partial class CaptureDeviceControlView : UserControl
{
    // A choice of a camera control with named values.
    private sealed class Choice
    {
        public Choice(int value, string name)
        {
            this.Value = value;
            this.Name = name;
        }

        public int Value { get; }
        public string Name { get; }

        public override string ToString() =>
            this.Name;
    }

    // Values of the power line frequency, the anti flicker filter.
    private static readonly Choice[] powerLineFrequencies =
    {
        new(0, "Disabled"),
        new(1, "50 Hz"),
        new(2, "60 Hz"),
        new(3, "Auto"),
    };

    private readonly CaptureDevice captureDevice;
    private readonly CaptureDeviceControl control;
    private readonly Control editor;
    private readonly int step;
    private bool isUpdating;

    public CaptureDeviceControlView(
        CaptureDevice captureDevice,
        CaptureDeviceControl control,
        CaptureDeviceControlValue value)
    {
        this.captureDevice = captureDevice;
        this.control = control;
        this.step = Math.Max(1, control.Step);

        this.InitializeComponent();

        this.valueSlider.Minimum = control.Minimum;
        this.valueSlider.Maximum = control.Maximum;
        this.valueSlider.SmallChange = this.step;
        // Clicking the slider moves it by a tenth of the range.
        this.valueSlider.LargeChange = Math.Max(this.step, (control.Maximum - control.Minimum) / 10 / this.step * this.step);
        this.autoCheckBox.Visible = control.SupportsAuto;

        // Show each control by its kind of values.
        var choices = control.Type == CaptureDeviceControlTypes.PowerLineFrequency ?
            powerLineFrequencies.Where(c => c.Value >= control.Minimum && c.Value <= control.Maximum).ToArray() :
            new Choice[0];
        this.choiceComboBox.Items.AddRange(choices);
        this.editor = choices.Length >= 1 ? this.choiceComboBox :
            control.Minimum == 0 && control.Maximum == 1 ? this.onCheckBox :
            this.valueSlider;
        this.valueSlider.Visible = this.editor == this.valueSlider;
        this.onCheckBox.Visible = this.editor == this.onCheckBox;
        this.choiceComboBox.Visible = this.editor == this.choiceComboBox;

        this.ShowValue(value ?? new CaptureDeviceControlValue(control.DefaultValue, false));
    }

    private void ShowValue(CaptureDeviceControlValue value)
    {
        this.isUpdating = true;
        try
        {
            this.nameLabel.Text = $"{this.control.Type} {value.Value}";
            this.valueSlider.Value = Math.Max(this.control.Minimum, Math.Min(this.control.Maximum, value.Value));
            this.onCheckBox.Checked = value.Value != 0;
            this.choiceComboBox.SelectedItem = this.choiceComboBox.Items.Cast<Choice>().FirstOrDefault(c => c.Value == value.Value);
            this.autoCheckBox.Checked = value.IsAuto;
            this.editor.Enabled = !value.IsAuto;
        }
        finally
        {
            this.isUpdating = false;
        }
    }

    private async void SetValue(int value, bool isAuto)
    {
        if (this.isUpdating)
        {
            return;
        }

        // Setting a value turns the automatic mode off.
        await this.captureDevice.SetControlValueAsync(this.control.Type, value, isAuto);

        // Show the value the device really has.
        if (await this.captureDevice.GetControlValueAsync(this.control.Type) is { } current)
        {
            this.ShowValue(current);
        }
    }

    // Slider was moved, the device gets the value rounded to its step.
    private void valueSlider_ValueChanged(object sender, EventArgs e) =>
        this.SetValue(
            this.control.Minimum + (this.valueSlider.Value - this.control.Minimum + this.step / 2) / this.step * this.step,
            false);

    // Check box was changed.
    private void onCheckBox_CheckedChanged(object sender, EventArgs e) =>
        this.SetValue(this.onCheckBox.Checked ? 1 : 0, false);

    // Choice was selected.
    private void choiceComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (this.choiceComboBox.SelectedItem is Choice choice)
        {
            this.SetValue(choice.Value, false);
        }
    }

    // Auto check box was changed.
    private void autoCheckBox_CheckedChanged(object sender, EventArgs e) =>
        this.SetValue(this.valueSlider.Value, this.autoCheckBox.Checked);
}
