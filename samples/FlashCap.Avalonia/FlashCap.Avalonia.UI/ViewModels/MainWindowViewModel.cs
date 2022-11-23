////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using Epoxy;
using Epoxy.Synchronized;
using Nito.AsyncEx;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

// NOTE: This sample application may crash when exit on .NET Framework (net48) configruation.
//   Maybe related Avalonia's this issue (in 0.10.13).
//   Avalonia app crashes on exit (.net framework only)
//   https://github.com/AvaloniaUI/Avalonia/issues/7579

namespace FlashCap.Avalonia.ViewModels;

[ViewModel]
public sealed class MainWindowViewModel
{
    private readonly AsyncLock locker = new();

    // Constructed capture device.
    private CaptureDevice? captureDevice;

    // Binding members.
    public Command? Opened { get; }
    public SKBitmap? Image { get; private set; }
    public bool IsEnbaled { get; private set; }

    public ObservableCollection<CaptureDeviceDescriptor?> DeviceList { get; } = new();
    public CaptureDeviceDescriptor? Device { get; set; }

    public ObservableCollection<VideoCharacteristics> CharacteristicsList { get; } = new();
    public VideoCharacteristics? Characteristics { get; set; }

    public MainWindowViewModel()
    {
        // Window shown:
        this.Opened = Command.Factory.CreateSync(() =>
        {
            ////////////////////////////////////////////////
            // Initialize and start capture device

            // Enumerate capture devices:
            var devices = new CaptureDevices();

            // Store device list into the combo box.
            this.DeviceList.Clear();
            this.DeviceList.Add(null);

            foreach (var descriptor in devices.EnumerateDescriptors().
                // You could filter by device type and characteristics.
                //Where(d => d.DeviceType == DeviceTypes.DirectShow).  // Only DirectShow device.
                Where(d => d.Characteristics.Length >= 1))             // One or more valid video characteristics.
            {
                this.DeviceList.Add(descriptor);
            }

            this.IsEnbaled = true;
        });
    }

    // Devices combo box was changed.
    [PropertyChanged(nameof(Device))]
    private ValueTask OnDeviceListChangedAsync(CaptureDeviceDescriptor? descriptor)
    {
        Debug.WriteLine($"OnDeviceListChangedAsync: Enter: {descriptor?.ToString() ?? "(null)"}");

        // Use first device.
        if (descriptor is { })
        {
#if false
            // Request video characteristics strictly:
            // Will raise exception when parameters are not accepted.
            var characteristics = new VideoCharacteristics(
                PixelFormats.JPEG, 1920, 1080, 60);
#else
            // Or, you could choice from device descriptor:
            this.CharacteristicsList.Clear();
            foreach (var characteristics in descriptor.Characteristics)
            {
                this.CharacteristicsList.Add(characteristics);
            }

            this.Characteristics = descriptor.Characteristics.FirstOrDefault();
#endif
        }
        else
        {
            this.CharacteristicsList.Clear();
            this.Characteristics = null;
        }

        Debug.WriteLine($"OnDeviceListChangedAsync: Leave: {descriptor?.ToString() ?? "(null)"}");

        return default;
    }

    // Characteristics combo box was changed.
    [PropertyChanged(nameof(Characteristics))]
    private async ValueTask OnCharacteristicsChangedAsync(VideoCharacteristics? characteristics)
    {
        Debug.WriteLine($"OnCharacteristicsChangedAsync: Enter: {characteristics?.ToString() ?? "(null)"}");

        using var _ = await this.locker.LockAsync();

        this.IsEnbaled = false;
        try
        {
            // Close when already opened.
            if (this.captureDevice is { } captureDevice)
            {
                this.captureDevice = null;

                Debug.WriteLine($"OnCharacteristicsChangedAsync: Stopping: {captureDevice.Name}");
                await captureDevice.StopAsync();

                Debug.WriteLine($"OnCharacteristicsChangedAsync: Disposing: {captureDevice.Name}");
                await captureDevice.DisposeAsync();
            }

            // Erase preview.
            this.Image = null;

            // Descriptor is assigned and set valid characteristics:
            if (this.Device is { } descriptor &&
                characteristics is { })
            {
                // Open capture device:
                Debug.WriteLine($"OnCharacteristicsChangedAsync: Opening: {descriptor.Name}");
                this.captureDevice = await descriptor.OpenAsync(
                    characteristics,
                    this.OnPixelBufferArrivedAsync);

                // Start capturing.
                Debug.WriteLine($"OnCharacteristicsChangedAsync: Starting: {descriptor.Name}");
                await this.captureDevice.StartAsync();
            }
        }
        finally
        {
            this.IsEnbaled = true;

            Debug.WriteLine($"OnCharacteristicsChangedAsync: Leave: {characteristics?.ToString() ?? "(null)"}");
        }
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
