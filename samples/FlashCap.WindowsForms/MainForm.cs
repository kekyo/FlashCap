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
using System.Windows.Forms;

namespace FlashCap.WindowsForms
{
    public partial class MainForm : Form
    {
        // Execute with limited only 1 task. (using FlashCap.Utilities)
        private LimitedExecutor limitedExecutor = new();

        // Constructed capture device.
        private ICaptureDevice? captureDevice;

        // Preallocated pixel buffer.
        private PixelBuffer buffer = new();


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
                //Where(d => d.DeviceType == DeviceTypes.DirectShow).
                ToArray();

            // Use first device.
            if (descriptors.ElementAtOrDefault(0) is { } descriptor0)
            {
#if true
                // Request video characteristics strictly:
                var characteristics = new VideoCharacteristics(
                    PixelFormats.JPEG, 1920, 1080, 60);
#else
                // Or, you could choice from device descriptor:
                // Hint: Show up video characteristics into ComboBox and like.
                var characteristics = descriptor0.Characteristics[0];
#endif
                // Video characteristics tips:
                // * DirectShow:
                //   Stable supported on listing video characteristics,
                //   will raise exception when use invalid parameter combination.
                // * Video for Windows:
                //   Will ignore silently when use invalid parameter combination.

                // Open capture device:
                this.captureDevice = descriptor0.Open(characteristics);

                // Hook frame arrived event:
                this.captureDevice.FrameArrived += this.OnFrameArrived!;

                // Start capturing.
                this.captureDevice.Start();
            }
        }

        private void OnFrameArrived(object sender, FrameArrivedEventArgs e)
        {
            ////////////////////////////////////////////////
            // Image frame has arrived

            // Windows Forms is too slow, so there's making throttle with LimitedExecutor class.
            this.limitedExecutor.ExecuteAndOffload(

                // Just now section:
                //   Capture into a pixel buffer:
                () => this.captureDevice?.Capture(e, this.buffer),

                // Offloaded section:
                //   Caution: Offloaded section is on the worker thread context.
                //   You have to switch main thread context before manipulates user interface.
                () =>
                {
#if false
                    // Get image data binary:
                    byte[] image = this.buffer.ExtractImage();
#else
                    // Or, refer image data binary directly.
                    // (Advanced manipulation, see README.)
                    ArraySegment<byte> image = this.buffer.ReferImage();
#endif
                    // Convert to Stream (using FlashCap.Utilities)
                    using var stream = image.AsStream();

                    // Decode image data to a bitmap:
                    var bitmap = Image.FromStream(stream);

                    // Switch to UI thread:
                    this.Invoke(() =>
                    {
                        // HACK: on .NET Core, will be leaked (or delayed GC?)
                        //   So we could release manually before updates.
                        if (this.BackgroundImage is { } oldImage)
                        {
                            this.BackgroundImage = null;
                            oldImage.Dispose();
                        }

                        // Update a bitmap.
                        this.BackgroundImage = bitmap;
                    });
                });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Discard capture device.
            this.captureDevice?.Dispose();
            this.captureDevice = null;
        }
    }
}