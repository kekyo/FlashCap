////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using FlashCap.Avalonia.ViewModels;
using FlashCap.Avalonia.Views;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

[assembly: AvaloniaTestApplication(typeof(FlashCap.Avalonia.Tests.TestAppBuilder))]

namespace FlashCap.Avalonia.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<Application>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

[TestFixture]
public sealed class WindowClosingTests
{
    [AvaloniaTest]
    public async Task CloseWaitsForRunningDeviceDisposal()
    {
        var descriptor = new TestDescriptor();
        descriptor.OpenAllowed.SetResult(true);
        var model = new MainWindowViewModel { Device = descriptor };
        var window = new MainWindow { DataContext = model };
        var closed = new TaskCompletionSource<bool>();
        window.Closed += (_, _) => closed.TrySetResult(true);
        try
        {
            await descriptor.OpenEntered.Task.WaitAsync(TimeSpan.FromSeconds(30));
            await descriptor.Device.StartAsync();
            window.Close();
            window.Close();
            Assert.That(closed.Task.IsCompleted, Is.False, "Keep the UI alive until camera cleanup completes.");
            await descriptor.Device.DisposeEntered.Task.WaitAsync(TimeSpan.FromSeconds(30));
            Assert.That(descriptor.Device.DisposeCount, Is.EqualTo(1));
            descriptor.Device.DisposeAllowed.SetResult(true);
            await closed.Task.WaitAsync(TimeSpan.FromSeconds(30));
            Assert.That(descriptor.Device.IsRunning, Is.False);
        }
        finally
        {
            descriptor.Device.DisposeAllowed.TrySetResult(true);
            await descriptor.Device.DisposeAsync();
            window.Close();
        }
    }

    [AvaloniaTest]
    public async Task CloseDuringOpeningReleasesTheDeviceWhenOpeningCompletes()
    {
        var descriptor = new TestDescriptor();
        var model = new MainWindowViewModel { Device = descriptor };
        var window = new MainWindow { DataContext = model };
        var closed = new TaskCompletionSource<bool>();
        window.Closed += (_, _) => closed.TrySetResult(true);
        try
        {
            await descriptor.OpenEntered.Task.WaitAsync(TimeSpan.FromSeconds(30));
            window.Close();
            Assert.That(closed.Task.IsCompleted, Is.False, "An in-flight open must not outlive the window.");
            descriptor.OpenAllowed.SetResult(true);
            await descriptor.Device.DisposeEntered.Task.WaitAsync(TimeSpan.FromSeconds(30));
            descriptor.Device.DisposeAllowed.SetResult(true);
            await closed.Task.WaitAsync(TimeSpan.FromSeconds(30));
            Assert.That(descriptor.Device.DisposeCount, Is.EqualTo(1));
        }
        finally
        {
            descriptor.OpenAllowed.TrySetResult(true);
            descriptor.Device.DisposeAllowed.TrySetResult(true);
            await descriptor.Device.DisposeAsync();
            window.Close();
        }
    }

    [AvaloniaTest]
    public async Task CloseWithoutADeviceCompletes()
    {
        var window = new MainWindow();
        var closed = new TaskCompletionSource<bool>();
        window.Closed += (_, _) => closed.TrySetResult(true);
        window.Close();
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(30));
    }

    private sealed class TestDescriptor : CaptureDeviceDescriptor
    {
        public readonly TestDevice Device = new();
        public readonly TaskCompletionSource<bool> OpenEntered = new();
        public readonly TaskCompletionSource<bool> OpenAllowed = new();

        public TestDescriptor() : base("Test camera", "", new[]
        {
            new VideoCharacteristics(PixelFormats.RGB24, 2, 2, 30),
        }, new DefaultBufferPool()) { }

        public override object Identity => "test";
        public override DeviceTypes DeviceType => DeviceTypes.PipeWire;

        protected override async Task<CaptureDevice> OnOpenWithFrameProcessorAsync(
            VideoCharacteristics characteristics, TranscodeFormats transcodeFormat,
            FrameProcessor frameProcessor, CancellationToken ct)
        {
            this.Device.Processor = frameProcessor;
            this.OpenEntered.TrySetResult(true);
            // Deliberately finish an already-started open even if closing cancels it.
            await this.OpenAllowed.Task;
            return this.Device;
        }
    }

    private sealed class TestDevice : CaptureDevice
    {
        public FrameProcessor? Processor;
        public int DisposeCount;
        public readonly TaskCompletionSource<bool> DisposeEntered = new();
        public readonly TaskCompletionSource<bool> DisposeAllowed = new();
        public TestDevice() : base("test", "Test camera") { }

        protected override async Task OnDisposeAsync()
        {
            if (this.Processor is not { } processor) return;
            this.Processor = null;
            this.DisposeCount++;
            this.DisposeEntered.TrySetResult(true);
            await this.DisposeAllowed.Task;
            this.IsRunning = false;
            await processor.DisposeAsync();
        }

        protected override Task OnInitializeAsync(VideoCharacteristics characteristics,
            TranscodeFormats transcodeFormat, FrameProcessor frameProcessor, CancellationToken ct) => Task.CompletedTask;
        protected override Task OnStartAsync(CancellationToken ct) { this.IsRunning = true; return Task.CompletedTask; }
        protected override Task OnStopAsync(CancellationToken ct) { this.IsRunning = false; return Task.CompletedTask; }
        protected override void OnCapture(IntPtr data, int size, long timestamp, long index, PixelBuffer buffer) { }
    }
}
