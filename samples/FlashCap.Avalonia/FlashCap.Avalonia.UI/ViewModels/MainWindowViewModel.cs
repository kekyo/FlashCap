////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using Epoxy;
using FlashCap.Utilities;
using SkiaSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

// NOTE: This sample application may crash when exit on .NET Framework (net48) configruation.
//   Maybe related Avalonia's this issue (in 0.10.13).
//   Avalonia app crashes on exit (.net framework only)
//   https://github.com/AvaloniaUI/Avalonia/issues/7579

namespace FlashCap.Avalonia.ViewModels
{
    [ViewModel]
    public sealed class MainWindowViewModel
    {
        // Constructed capture device.
        private CaptureDevice? captureDevice;

        // Binding members.
        public Command? Opened { get; }
        public SKBitmap? Image { get; private set; }
        public string? Device { get; private set; }
        public string? Characteristics { get; private set; }

        public MainWindowViewModel()
        {
            // Window shown:
            this.Opened = CommandFactory.Create(async () =>
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
 
        private async Task OnPixelBufferArrivedAsync(PixelBufferScope bufferScope)
        {
            ////////////////////////////////////////////////
            // Pixel buffer has arrived.
            // NOTE: Perhaps this thread context is NOT UI thread.
#if false
            // Get image data binary:
            byte[] image = bufferScope.Buffer.ExtractImage();
#else
            // Or, refer image data binary directly.
            ArraySegment<byte> image = bufferScope.Buffer.ReferImage();
#endif
            // Decode image data to a bitmap:
            var bitmap = SKBitmap.Decode(image);

            // `bitmap` is copied, so we can release pixel buffer now.
            bufferScope.ReleaseNow();

            // Switch to UI thread:
            if (await UIThread.TryBind())
            {
                // Update a bitmap.
                this.Image = bitmap;
            }
        }
    }
}
