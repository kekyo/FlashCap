////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using Epoxy;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace FlashCap.Wpf.ViewModels;

[ViewModel]
public sealed class MainWindowViewModel
{
    private enum States
    {
        NotShown,
        Ready,
        Show,
    }

    private States state = States.NotShown;
    private long countFrames;

    // Constructed capture device.
    private CaptureDevice? captureDevice;

    // Binding members.
    public Command Loaded { get; }
    public SKBitmap? Image { get; private set; }
    public bool IsEnabled { get; private set; }
    public Command StartCapture { get; }
    public Command StopCapture { get; }
    public Command ShowPropertyPage { get; }
    public bool IsEnabledStartCapture { get; private set; }
    public bool IsEnabledStopCapture { get; private set; }
    public bool IsEnabledShowPropertyPage { get; private set; }

    public Pile<Window> WindowPile { get; } = Pile.Factory.Create<Window>();

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

            foreach (var descriptor in devices.EnumerateDescriptors().
                // You could filter by device type and characteristics.
                //Where(d => d.DeviceType == DeviceTypes.DirectShow).  // Only DirectShow device.
                Where(d => d.Characteristics.Length >= 1))             // One or more valid video characteristics.
            {
                this.DeviceList.Add(descriptor);
            }

            this.Device = this.DeviceList.FirstOrDefault();

            this.IsEnabled = true;

            return default;
        });

        // Clicked start capture button.
        this.StartCapture = Command.Factory.Create(async () =>
        {
            // Erase preview.
            this.Image = null;
            this.Statistics1 = null;
            this.Statistics2 = null;
            this.Statistics3 = null;
            this.countFrames = 0;

            await this.captureDevice!.StartAsync();

            this.UpdateCurrentState(States.Show);
        });

        // Clicked stop capture button.
        this.StopCapture = Command.Factory.Create(async () =>
        {
            await this.captureDevice!.StopAsync();

            this.UpdateCurrentState(States.Ready);
        });

        // Clicked show property page button.
        this.ShowPropertyPage = Command.Factory.Create(async () =>
        {
            if (this.captureDevice is { } captureDevice &&
                captureDevice.HasPropertyPage == true)
            {
                // Partially rent Window object from the anchor.
                await this.WindowPile.RentAsync(async window =>
                {
                    // Take Win32 parent window handle and show with relation.
                    var handle = new WindowInteropHelper(window).Handle;
                    await captureDevice.ShowPropertyPageAsync(handle);
                });
            }
        });
    }

    // Buttons enabling control.
    private void UpdateCurrentState(States state)
    {
        this.state = state;

        switch (this.state)
        {
            case States.Ready:
                this.IsEnabledStartCapture = true;
                this.IsEnabledStopCapture = false;
                break;
            case States.Show:
                this.IsEnabledStartCapture = false;
                this.IsEnabledStopCapture = true;
                break;
            default:
                this.IsEnabledStartCapture = false;
                this.IsEnabledStopCapture = false;
                break;
        }

        this.IsEnabledShowPropertyPage = this.captureDevice?.HasPropertyPage ?? false;
    }

    // Devices combo box was changed.
    [PropertyChanged(nameof(Device))]
    private ValueTask OnDeviceChangedAsync(CaptureDeviceDescriptor? descriptor)
    {
        Debug.WriteLine($"OnDeviceChangedAsync: Enter: {descriptor?.ToString() ?? "(null)"}");

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
            if (this.Characteristics != null)
            {
                this.UpdateCurrentState(States.Ready);
            }
        }
        else
        {
            this.CharacteristicsList.Clear();
            this.Characteristics = null;

            this.UpdateCurrentState(States.NotShown);
        }

        Debug.WriteLine($"OnDeviceChangedAsync: Leave: {descriptor?.ToString() ?? "(null)"}");

        return default;
    }

    // Characteristics combo box was changed.
    [PropertyChanged(nameof(Characteristics))]
    private async ValueTask OnCharacteristicsChangedAsync(VideoCharacteristics? characteristics)
    {
        Debug.WriteLine($"OnCharacteristicsChangedAsync: Enter: {characteristics?.ToString() ?? "(null)"}");

        this.IsEnabled = false;
        try
        {
            // Close when already opened.
            if (this.captureDevice is { } captureDevice)
            {
                this.captureDevice = null;

                this.UpdateCurrentState(States.NotShown);

                Debug.WriteLine($"OnCharacteristicsChangedAsync: Stopping: {captureDevice.Name}");
                await captureDevice.StopAsync();

                Debug.WriteLine($"OnCharacteristicsChangedAsync: Disposing: {captureDevice.Name}");
                await captureDevice.DisposeAsync();
            }

            // Erase preview.
            this.Image = null;
            this.Statistics1 = null;
            this.Statistics2 = null;
            this.Statistics3 = null;
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

                this.UpdateCurrentState(States.Ready);
            }
        }
        finally
        {
            this.IsEnabled = true;

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
}
