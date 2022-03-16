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

namespace FlashCap.WindowsForms
{
    public partial class MainForm : Form
    {
        private ICaptureDevice? captureDevice;
        private int isin;

        // Preallocated pixel buffer.
        private PixelBuffer buffer = new();

        public MainForm() =>
            InitializeComponent();

        private void Form1_Load(object sender, EventArgs e)
        {
            // Enumerate devices:
            var devices = new CaptureDevices();
            var descriptors = devices.EnumerateDescriptors().
                //Where(d => d.DeviceType == DeviceTypes.DirectShow).
                Where(d => d.DeviceType == DeviceTypes.VideoForWindows).
                ToArray();

            if (descriptors.ElementAtOrDefault(0) is { } descriptor0)
            {
                this.captureDevice = descriptor0.Open(descriptor0.Characteristics[0]);
                this.captureDevice.FrameArrived += this.OnFrameArrived!;

                this.captureDevice.Start();
            }
        }

        private void OnFrameArrived(object sender, FrameArrivedEventArgs e)
        {
            // Windows Forms is too slow, so there's making throttle...
            if (Interlocked.Increment(ref this.isin) == 1)
            {
                // Capture into a pixel buffer:
                this.captureDevice?.Capture(e, this.buffer);

                // Caution: Perhaps `FrameArrived` event is on the worker thread context.
                // You have to switch main thread context before manipulates user interface.
                this.BeginInvoke(() =>
                {
                    try
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

                        // HACK: on .NET Core, will be leaked (or delayed GC?)
                        //   So we could release manually before updates.
                        if (this.BackgroundImage is { } oldImage)
                        {
                            this.BackgroundImage = null;
                            oldImage.Dispose();
                        }

                        // Update a bitmap.
                        this.BackgroundImage = bitmap;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref this.isin);
                    }
                });
            }
            else
            {
                Interlocked.Decrement(ref this.isin);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.captureDevice?.Dispose();
            this.captureDevice = null;
        }
    }
}