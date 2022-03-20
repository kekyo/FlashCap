////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using Avalonia.Media.Imaging;
using Epoxy;
using Epoxy.Synchronized;
using FlashCap.Utilities;

namespace FlashCap.Avalonia.ViewModels
{
    [ViewModel]
    public sealed class MainWindowViewModel
    {
        // Execute with limited only 1 task. (using FlashCap.Utilities)
        private readonly LimitedExecutor limitedExecutor = new();

        // Preallocated pixel buffer.
        private readonly PixelBuffer buffer = new();

        // Constructed capture device.
        private ICaptureDevice? captureDevice;

        // Binding members.
        public Command? Opened { get; }
        public Bitmap? Image { get; private set; }

        public MainWindowViewModel()
        {
            // Window shown:
            this.Opened = Command.Factory.CreateSync<EventArgs>(_ =>
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
#if false
                    // Request video characteristics strictly:
                    var characteristics = new VideoCharacteristics(
                        PixelFormats.MJPG, 24, 1920, 1080, 30);
#else
                    // Or, you could choice from device descriptor:
                    // Hint: Show up video characteristics into ComboBox and like.
                    var characteristics = descriptor0.Characteristics[0];
#endif
                    // Video characteristics tips:
                    // * DirectShow:
                    //   Supported only listing video characteristics,
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
            });
        }
 
        private void OnFrameArrived(object sender, FrameArrivedEventArgs e)
        {
            ////////////////////////////////////////////////
            // Image frame has arrived

            // User interface system is too slow,
            // so there's making throttle with LimitedExecutor class.
            this.limitedExecutor.ExecuteAndOffload(

                // Just now section:
                //   Capture into a pixel buffer:
                () => this.captureDevice?.Capture(e, this.buffer),

                // Offloaded section:
                //   Caution: Offloaded section is on the worker thread context.
                //   You have to switch main thread context before manipulates user interface.
                async () =>
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
                    var bitmap = new Bitmap(stream);

                    // Switch to UI thread:
                    await UIThread.Bind();

                    // Update a bitmap.
                    this.Image = bitmap;
                });
        }
    }
}
