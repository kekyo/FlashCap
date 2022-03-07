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
using System.Windows.Forms;

namespace FlashCap.WindowsForms
{
    public partial class Form1 : Form
    {
        private CaptureDevice? captureDevice;

        public Form1() =>
            InitializeComponent();

        private void Form1_Load(object sender, EventArgs e)
        {
            var descriptors = CaptureDevices.Devices.ToArray();

            if (descriptors.FirstOrDefault() is { } descriptor0)
            {
                this.captureDevice = descriptor0.Open();
                this.captureDevice.Captured += this.CaptureDevice_Captured;

                this.captureDevice.Start();
            }
        }

        private void CaptureDevice_Captured(object sender, CapturedEventArgs e)
        {
            //using var stream = e.ExtractStream();

            //var bitmap = Bitmap.FromStream(stream);
            //this.BackgroundImage = bitmap;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.captureDevice?.Dispose();
            this.captureDevice = null;
        }
    }
}