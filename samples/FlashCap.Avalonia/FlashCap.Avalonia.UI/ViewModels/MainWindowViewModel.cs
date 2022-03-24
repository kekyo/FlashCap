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
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Epoxy;
using Epoxy.Synchronized;
using FlashCap.Utilities;

namespace FlashCap.Avalonia.ViewModels
{
    [ViewModel]
    public sealed class MainWindowViewModel
    {
        // Constructed capture device.
        private CaptureDevice? captureDevice;

        // Binding members.
        public Command? Opened { get; }
        public Bitmap? Image { get; private set; }
        public string? Device { get; private set; }
        public string? Characteristics { get; private set; }

        public MainWindowViewModel()
        {
            // Window shown:
            this.Opened = Command.Factory.Create<EventArgs>(async _ =>
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
                if (descriptors.ElementAtOrDefault(0) is { } descriptor0)
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
                    this.Device = descriptor0.ToString();
                    this.Characteristics = characteristics.ToString();

                    // Open capture device:
                    this.captureDevice = await descriptor0.OpenAsync(
                        characteristics,
                        this.OnPixelBufferArrivedAsync);

                    // Start capturing.
                    this.captureDevice.Start();
                }
                else
                {
                    this.Device = "(Device Not found)";
                }
            });
        }
 
        private async ValueTask OnPixelBufferArrivedAsync(PixelBuffer buffer)
        {
            ////////////////////////////////////////////////
            // Pixel buffer has arrived.
            // NOTE: Perhaps this thread context is NOT UI thread.
#if false
            // Get image data binary:
            byte[] image = buffer.ExtractImage();
#else
            // Or, refer image data binary directly.
            ArraySegment<byte> image = buffer.ReferImage();
#endif
            // Convert to Stream (using FlashCap.Utilities)
            using var stream = image.AsStream();

            // Decode image data to a bitmap:
            var bitmap = new Bitmap(stream);

            // Switch to UI thread:
            await UIThread.Bind();

            // Update a bitmap.
            this.Image = bitmap;
        }
    }
}
