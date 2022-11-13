////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Utilities;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace FlashCap.WindowsForms;

public partial class MainForm : Form
{
    private SynchronizationContext synchContext;

    // Constructed capture device.
    private CaptureDevice captureDevice;

    public MainForm() =>
        this.InitializeComponent();

    private async void MainForm_Load(object sender, EventArgs e)
    {
        this.synchContext = SynchronizationContext.Current;

        ////////////////////////////////////////////////
        // Initialize and start capture device

        // Enumerate capture devices:
        var devices = new CaptureDevices();
        var descriptors = devices.EnumerateDescriptors().
            // You could filter by device type and characteristics.
            //Where(d => d.DeviceType == DeviceTypes.DirectShow).  // Only DirectShow device.
            Where(d => d.Characteristics.Length >= 1).             // One or more valid video characteristics.
            ToArray();

        // Use first device.
        var descriptor0 = descriptors.ElementAtOrDefault(0);
        if (descriptor0 != null)
        {
#if false
            // Request video characteristics strictly:
            // Will raise exception when parameters are not accepted.
            var characteristics = new VideoCharacteristics(
                PixelFormats.JPEG, 1920, 1080, 60);
#else
            // Or, you could choice from device descriptor:
            // Hint: Show up video characteristics into ComboBox and like.
            var characteristics = descriptor0.Characteristics[0];
#endif
            // Show status.
            this.deviceLabel.Text = descriptor0.ToString();
            this.characteristicsLabel.Text = characteristics.ToString();

            // Open capture device:
            this.captureDevice = await descriptor0.OpenAsync(
                characteristics,
                this.OnPixelBufferArrived);

            // Start capturing.
            this.captureDevice.Start();
        }
    }

    private void OnPixelBufferArrived(PixelBufferScope bufferScope)
    {
        ////////////////////////////////////////////////
        // Pixel buffer has arrived.
        // NOTE: Perhaps this thread context is NOT UI thread.
#if false
        // Get image data binary:
        byte[] image = bufferScope.Buffer.ExtractImage();
#else
        // Or, refer image data binary directly.
        // (Advanced manipulation, see README.)
        ArraySegment<byte> image = bufferScope.Buffer.ReferImage();
#endif
        // Convert to Stream (using FlashCap.Utilities)
        using (var stream = image.AsStream())
        {
            // Decode image data to a bitmap:
            var bitmap = Image.FromStream(stream);

            // `bitmap` is copied, so we can release pixel buffer now.
            bufferScope.ReleaseNow();

            // Switch to UI thread.
            // HACK: Here is using `SynchronizationContext.Post()` instead of `Control.Invoke()`.
            // Because in sensitive states when the form is closing,
            // `Control.Invoke()` can fail with exception.
            this.synchContext.Post(_ =>
            {
                // HACK: on .NET Core, will be leaked (or delayed GC?)
                //   So we could release manually before updates.
                var oldImage = this.BackgroundImage;
                if (oldImage != null)
                {
                    this.BackgroundImage = null;
                    oldImage.Dispose();
                }

                // Update a bitmap.
                this.BackgroundImage = bitmap;
            }, null);
        }
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        // Discard capture device.
        this.captureDevice?.Dispose();
        this.captureDevice = null;
    }
}
