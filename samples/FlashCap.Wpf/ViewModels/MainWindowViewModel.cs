////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using Epoxy;
using FlashCap.Devices;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Wpf.ViewModels;

[ViewModel]
public sealed class MainWindowViewModel
{
    private long countFrames;

    // Constructed capture device.
    private CaptureDevice? captureDevice;

    // Binding members.
    public Command? Loaded { get; }
    public SKBitmap? Image { get; private set; }
    public bool IsEnbaled { get; private set; }

    public ObservableCollection<CaptureDeviceDescriptor?> DeviceList { get; } = new();
    public CaptureDeviceDescriptor? Device { get; set; }

    public ObservableCollection<VideoCharacteristics> CharacteristicsList { get; } = new();
    public VideoCharacteristics? Characteristics { get; set; }

    public string? Statistics1 { get; private set; }
    public string? Statistics2 { get; private set; }
    public string? Statistics3 { get; private set; }

    public MainWindowViewModel()
    {
        // Window shown:
        this.Loaded = Command.Factory.Create(() =>
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

            return default;
        });
    }

    // Devices combo box was changed.
    [PropertyChanged(nameof(Device))]
    private ValueTask OnDeviceListChangedAsync(CaptureDeviceDescriptor? descriptor)
    {
        Debug.WriteLine($"OnDeviceListChangedAsync: Enter: {descriptor?.ToString() ?? "(null)"}");

        // Use selected device.
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
                if (characteristics.PixelFormat != PixelFormats.Unknown)
                {
                    this.CharacteristicsList.Add(characteristics);
                }
            }

            this.Characteristics = this.CharacteristicsList.FirstOrDefault();
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
            this.Statistics1 = null;
            this.Statistics2 = null;
            this.countFrames = 0;

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

        // Capture statistics variables.
        var countFrames = Interlocked.Increment(ref this.countFrames);
        var frameIndex = bufferScope.Buffer.FrameIndex;
        var timestamp = bufferScope.Buffer.Timestamp;

        // `bitmap` is copied, so we can release pixel buffer now.
        bufferScope.ReleaseNow();

        // Switch to UI thread:
        if (await UIThread.TryBind())
        {
            // Update a bitmap.
            this.Image = bitmap;

            // Update statistics.
            var realFps = countFrames / timestamp.TotalSeconds;
            var fpsByIndex = frameIndex / timestamp.TotalSeconds;
            this.Statistics1 = $"Frame={countFrames}/{frameIndex}";
            this.Statistics2 = $"FPS={realFps:F3}/{fpsByIndex:F3}";
            this.Statistics3 = $"SKBitmap={bitmap.Width}x{bitmap.Height} [{bitmap.ColorType}]";
        }
    }

    internal void ShowProperties()
    {
        if( captureDevice is DirectShowDevice dsDevice )
            dsDevice.ShowPropertyPage(IntPtr.Zero);
    }

    internal Task Start()
    {
        return Task.Run(async () =>
        {
            // Start capturing.
            if (this.captureDevice != null)
                await this.captureDevice.StartAsync();
        });
    }
    internal Task Stop()
    {
        return Task.Run(async () =>
        {
            // Stop capturing.
            if (this.captureDevice != null)
                await this.captureDevice.StopAsync();
        });
    }
}
