////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// NOTE: This sample application may crash when exit on .NET Framework (net48) configruation.
//   Maybe related Avalonia's this issue (in 0.10.13).
//   Avalonia app crashes on exit (.net framework only)
//   https://github.com/AvaloniaUI/Avalonia/issues/7579

namespace FlashCap.Avalonia.ViewModels;

public sealed class MainWindowViewModel:ReactiveObject
{
    private long countFrames;

    // Constructed capture device.
    private CaptureDevice? captureDevice;

    [Reactive]
    public SKObject? Image { get; private set; }
    [Reactive]
    public bool IsEnbaled { get; private set; }

    [Reactive]
    public ObservableCollection<CaptureDeviceDescriptor?> DeviceList { get; private set; } = new();
    [Reactive]
    public CaptureDeviceDescriptor? Device { get; set; }

    [Reactive]
    public ObservableCollection<VideoCharacteristics> CharacteristicsList { get; private set; } = new();
    [Reactive]
    public VideoCharacteristics? Characteristics { get; set; }

    [Reactive]
    public string? Statistics1 { get; private set; }
    [Reactive]
    public string? Statistics2 { get; private set; }
    [Reactive]
    public string? Statistics3 { get; private set; }

    public MainWindowViewModel()
    {
        this.PropertyChanged += MainWindowViewModel_PropertyChanged;
    }


    private async void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Device))
        {
           await OnDeviceListChangedAsync(Device);
        }
        else if (e.PropertyName == nameof(Characteristics))
        {
            await OnCharacteristicsChangedAsync(Characteristics);
        }
    }

    public void Open()
    {
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
    }

    // Devices combo box was changed.
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
                    characteristics,false,
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

    //private async Task OnPixelBufferArrivedAsync(PixelBufferScope bufferScope)
    private void OnPixelBufferArrivedAsync(PixelBufferScope bufferScope)
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
        //var bitmap = SKBitmap.Decode(image);
        var bitmap = SKImage.FromEncodedData(image);

        // Capture statistics variables.
        var countFrames = Interlocked.Increment(ref this.countFrames);
        var frameIndex = bufferScope.Buffer.FrameIndex;
        var timestamp = bufferScope.Buffer.Timestamp;

        // `bitmap` is copied, so we can release pixel buffer now.
        bufferScope.ReleaseNow();
        // Switch to UI thread:
         // Update a bitmap.
        Dispatcher.UIThread.Post(() =>
        {
            this.Image = bitmap;
            // Update statistics.
            var realFps = countFrames / timestamp.TotalSeconds;
            var fpsByIndex = frameIndex / timestamp.TotalSeconds;
            this.Statistics1 = $"Frame={countFrames}/{frameIndex}";
            this.Statistics2 = $"FPS={realFps:F3}/{fpsByIndex:F3}";
            this.Statistics3 = $"{bitmap.GetType()}={bitmap.Width}x{bitmap.Height} [{bitmap.ColorType}]";
        }, DispatcherPriority.MaxValue);
    }
}
