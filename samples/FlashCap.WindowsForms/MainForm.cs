////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlashCap.WindowsForms
{
    public partial class MainForm : Form
    {
        private ICaptureDevice? captureDevice;
        private PixelBuffer buffer = new();
        private bool isin;

        public MainForm() =>
            InitializeComponent();

        private void Form1_Load(object sender, EventArgs e)
        {
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

        private async void OnFrameArrived(object sender, FrameArrivedEventArgs e)
        {
            // Windows Forms is too slow, so there's making throttling...

            if (!this.isin)
            {
                this.isin = true;
                try
                {
                    this.captureDevice?.Capture(e, this.buffer);

                    await TaskEx.Delay(100);

                    var image = this.buffer.ExtractImage();

                    var bitmap = Bitmap.FromStream(new MemoryStream(image));

                    // HACK: on .NET Core, will be leaked (or delayed GC?)
                    //   updating background image with new Bitmap's.
                    if (this.BackgroundImage is { } oldImage)
                    {
                        this.BackgroundImage = null;
                        oldImage.Dispose();
                    }

                    this.BackgroundImage = bitmap;
                }
                finally
                {
                    this.isin = false;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.captureDevice?.Dispose();
            this.captureDevice = null;
        }
    }
}