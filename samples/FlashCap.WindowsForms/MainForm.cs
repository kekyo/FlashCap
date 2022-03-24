////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Synchronized;
using FlashCap.Utilities;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlashCap.WindowsForms
{
    public partial class MainForm : Form
    {
        // Constructed capture device.
        private CaptureDevice captureDevice;

        public MainForm() =>
            InitializeComponent();

        private void Form1_Load(object sender, EventArgs e)
        {
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
                // (Non asynchronous version, using FlashCap.Synchronized)
                this.captureDevice = descriptor0.Open(
                    characteristics,
                    this.OnPixelBufferArrived);

                // Start capturing.
                this.captureDevice.Start();
            }
        }

        private void OnPixelBufferArrived(PixelBuffer buffer)
        {
            ////////////////////////////////////////////////
            // Pixel buffer has arrived.
            // NOTE: Perhaps this thread context is NOT UI thread.
#if false
            // Get image data binary:
            byte[] image = buffer.ExtractImage();
#else
            // Or, refer image data binary directly.
            // (Advanced manipulation, see README.)
            ArraySegment<byte> image = buffer.ReferImage();
#endif
            // Convert to Stream (using FlashCap.Utilities)
            using (var stream = image.AsStream())
            {
                // Decode image data to a bitmap:
                var bitmap = Image.FromStream(stream);

                try
                {
                    // Switch to UI thread:
                    this.Invoke(new Action(() =>
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
                    }));
                }
                catch (ObjectDisposedException)
                {
                    // NOTE: WinForms sometimes will raise ObjectDisposedException in shutdown sequence.
                    // Because it is race condition between this thread context and UI thread context.
                    // We can safely ignore when terminating user interface.
                    // (Or you can avoid it with graceful shutdown technics.)
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Discard capture device.
            this.captureDevice?.Dispose();
            this.captureDevice = null;
        }
    }
}
