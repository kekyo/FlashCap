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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FlashCap.Wpf.ViewModels;

[ViewModel]
public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    // Constructed capture device.
    private CaptureDevice? captureDevice;

    // Binding members.
    public Command? Loaded { get; }
    public SKBitmap? Image { get; private set; }
    public string? Device { get; private set; }
    public string? Characteristics { get; private set; }

    // holds the list of devices, bound to the ComboBox view
    private ObservableCollection<CaptureDeviceDescriptor> deviceList;
    public ObservableCollection<CaptureDeviceDescriptor> DeviceList
    {
        get { return deviceList; }
        set
        {
            if (deviceList != value)
            {
                deviceList = value;
                OnPropertyChanged();
            }
        }
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public MainWindowViewModel()
    {
        deviceList = new ObservableCollection<CaptureDeviceDescriptor>();
        // Window shown:
        this.Loaded = Command.Factory.Create(async () =>
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

            DeviceList = new ObservableCollection<CaptureDeviceDescriptor>(descriptors);

            // Use first device.
            if (descriptors.ElementAtOrDefault(0) is { } descriptor0)
                await SelectDeviceInternal(descriptor0);
            else
            {
                this.Device = "(Devices are not found)";
            }
        });
    }

    private Task SelectDeviceInternal(CaptureDeviceDescriptor descriptor)
    {
        if (descriptor != null)
        {
            this.Device = descriptor.ToString();

#if false
                // Request video characteristics strictly:
                // Will raise exception when parameters are not accepted.
                var characteristics = new VideoCharacteristics(
                    PixelFormats.JPEG, 1920, 1080, 60);
#else
            // Or, you could choice from device descriptor:
            // Hint: Show up video characteristics into ComboBox and like.
            var characteristics = descriptor.Characteristics.
                FirstOrDefault(c => c.PixelFormat != PixelFormats.Unknown);
#endif
            if (characteristics != null)
            {
                // Show status.
                this.Characteristics = characteristics.ToString();

                // Open capture device:
                return Task.Run(async () =>
                {
                    this.captureDevice = await descriptor.OpenAsync(
                        characteristics,
                        this.OnPixelBufferArrivedAsync);
                });
            }
            else
            {
                this.Characteristics = "(Formats are not found)";
            }
        }
        return Task.CompletedTask;
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

    /* A user selected a different video source from the drop down menu */
    internal async void SelectDevice(CaptureDeviceDescriptor device)
    {
        await Stop();
        await SelectDeviceInternal(device);
        await Start();
    }
}
