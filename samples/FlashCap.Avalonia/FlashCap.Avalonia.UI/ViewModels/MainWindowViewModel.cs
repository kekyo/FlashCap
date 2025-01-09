////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using Avalonia.Controls;
//using Epoxy;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive;
using Avalonia.Threading;
using ReactiveUI;

// NOTE: This sample application may crash when exit on .NET Framework (net48) configruation.
//   Maybe related Avalonia's this issue (in 0.10.13).
//   Avalonia app crashes on exit (.net framework only)
//   https://github.com/AvaloniaUI/Avalonia/issues/7579

namespace FlashCap.Avalonia.ViewModels;

//[ViewModel]
public sealed class MainWindowViewModel: ReactiveObject
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
    
    private SKBitmap? _image;

    public SKBitmap? Image
    {
        get => _image;
        private set => this.RaiseAndSetIfChanged(ref _image, value);
    }
    
    private bool _isEnabled;

    public bool IsEnabled
    {
        get => _isEnabled; 
        private set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }

    public ReactiveCommand<Unit, Unit> ShowPropertyPage { get; }
    
    private bool _isEnabledStartCapture;
    public bool IsEnabledStartCapture 
    { 
        get => _isEnabledStartCapture; 
        private set => this.RaiseAndSetIfChanged(ref _isEnabledStartCapture, value); 
    }
    
    private bool _isEnabledStopCapture;

    public bool IsEnabledStopCapture
    {
        get => _isEnabledStopCapture;
        private set => this.RaiseAndSetIfChanged(ref _isEnabledStopCapture, value);
    }

    private bool _isEnabledShowPropertyPage;
    public bool IsEnabledShowPropertyPage
    {
        get => _isEnabledShowPropertyPage;
        private set => this.RaiseAndSetIfChanged(ref _isEnabledShowPropertyPage, value);
    }

    //public Pile<Window> WindowPile { get; } = Pile.Factory.Create<Window>();
    
    public Window? ParentWindow { get; set; }
    
    private ObservableCollection<CaptureDeviceDescriptor?> _deviceList = new();
    
    public ObservableCollection<CaptureDeviceDescriptor?> DeviceList
    {
        get => _deviceList;
        set => this.RaiseAndSetIfChanged(ref _deviceList, value);
    }
    
    //public ObservableCollection<CaptureDeviceDescriptor?> DeviceList { get; } = new();
    
    private CaptureDeviceDescriptor? _device;
    
    public CaptureDeviceDescriptor? Device 
    { 
        get => _device;
        set
        {
            
            if (value != null && !value.Equals(_device))
            {
                this.RaiseAndSetIfChanged(ref _device, value);
                this.OnDeviceChangedAsync(value);
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _device, value);
            }
            
            
        }
    }

    private ObservableCollection<VideoCharacteristics> _characteristicsList = new();
    public ObservableCollection<VideoCharacteristics> CharacteristicsList 
    { 
        get => _characteristicsList;
        set => this.RaiseAndSetIfChanged(ref _characteristicsList, value);
    }
    
    private VideoCharacteristics? _characteristics;

    public VideoCharacteristics? Characteristics
    {
        get => _characteristics;
        set
        {
            if (value != null && !value.Equals(_characteristics))
            {
                _= this.OnCharacteristicsChangedAsync(value);
            }
            this.RaiseAndSetIfChanged(ref _characteristics, value);
        }
    }

    public string? Statistics1 { get; private set; }
    public string? Statistics2 { get; private set; }
    public string? Statistics3 { get; private set; }

    public MainWindowViewModel()
    {
        this.UpdateCurrentState(States.NotShown);
        
        // Clicked show property page button.
        this.ShowPropertyPage = ReactiveCommand.CreateFromTask(async () =>
        {
            if (this.captureDevice is { } captureDevice &&
                captureDevice.HasPropertyPage == true)
            {

                // Take Win32 parent window handle and show with relation.
                if (ParentWindow!.TryGetPlatformHandle()?.Handle is { } handle)
                {
                    await captureDevice.ShowPropertyPageAsync(handle);
                }
                
            }
        });
    }

    public void OpenedHandler(object sender, EventArgs e)
    {
        ////////////////////////////////////////////////
        // Initialize and start capture device

        // Enumerate capture devices:
        var devices = new CaptureDevices();

        // Store device list into the combo box.
        this.DeviceList.Clear();
            
        var deviceDescriptors = devices.EnumerateDescriptors().ToArray();

        foreach (var descriptor in deviceDescriptors.
                     // You could filter by device type and characteristics.
                     //Where(d => d.DeviceType == DeviceTypes.DirectShow).  // Only DirectShow device.
                     Where(d => d.Characteristics.Length >= 1))             // One or more valid video characteristics.
        {
            this.DeviceList.Add(descriptor);
        }

        this.Device = this.DeviceList.FirstOrDefault();

        this.IsEnabled = true;
    }
    
    public async Task StartCapture()
    {
        // Erase preview.
        this.Image = null;
        this.Statistics1 = null;
        this.Statistics2 = null;
        this.Statistics3 = null;
        this.countFrames = 0;

        /*await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await this.captureDevice!.StartAsync();
            UpdateCurrentState(States.Show);
        });*/
        
        await this.captureDevice!.StartAsync();
        UpdateCurrentState(States.Show);
    }

    public async Task StopCapture()
    {
        await this.captureDevice!.StopAsync();

        this.UpdateCurrentState(States.Ready);
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
    private ValueTask OnDeviceChangedAsync(CaptureDeviceDescriptor? descriptor)
    {
        Debug.WriteLine($"OnDeviceChangedAsync: Enter: {descriptor?.ToString() ?? "(null)"}");

        // Use selected device.
        if (descriptor is not null)
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
            if (this.Device is not null && characteristics is not null)
            {
                var descriptor = this.Device;
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
        
        await Dispatcher.UIThread.InvokeAsync( () =>
        {
            // Update a bitmap.
            this.Image = bitmap;

            // Update statistics.
            var realFps = countFrames / timestamp.TotalSeconds;
            var fpsByIndex = frameIndex / timestamp.TotalSeconds;
            this.Statistics1 = $"Frame={countFrames}/{frameIndex}";
            this.Statistics2 = $"FPS={realFps:F3}/{fpsByIndex:F3}";
            this.Statistics3 = $"SKBitmap={bitmap.Width}x{bitmap.Height} [{bitmap.ColorType}]";
        });
        
        //return Task.CompletedTask;
    }
}
