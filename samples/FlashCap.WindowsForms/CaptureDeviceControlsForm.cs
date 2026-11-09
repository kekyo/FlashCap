////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace FlashCap.WindowsForms;

// Shows the camera controls the device offers, such as the brightness or the zoom.
public partial class CaptureDeviceControlsForm : Form
{
    private readonly CaptureDevice captureDevice;

    public CaptureDeviceControlsForm(CaptureDevice captureDevice)
    {
        this.captureDevice = captureDevice;
        this.InitializeComponent();
    }

    private async void CaptureDeviceControlsForm_Load(object sender, EventArgs e)
    {
        // A row for each control, the device decides which ones it offers.
        foreach (var control in await this.captureDevice.GetControlsAsync())
        {
            var value = await this.captureDevice.GetControlValueAsync(control.Type);
            this.controlsPanel.Controls.Add(new CaptureDeviceControlView(this.captureDevice, control, value));
        }
    }
}
