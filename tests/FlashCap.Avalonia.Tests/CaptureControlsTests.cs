////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using Avalonia.Headless.NUnit;
using FlashCap.Avalonia.ViewModels;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Avalonia.Tests;

[TestFixture]
public sealed class CaptureControlsTests
{
    [AvaloniaTest]
    public async Task CaptureExposesLateControlsAndRestartKeepsOneEditor()
    {
        var camera = new CameraDescriptor(true);
        var model = new MainWindowViewModel { Device = camera };
        try
        {
            Assert.That(model.IsEnabledShowControls, Is.False);
            for (var run = 0; run < 2; run++)
            {
                Assert.That(model.IsEnabledStartCapture, Is.True);
                await model.StartCapture.ExecuteAsync(null);
                Assert.That(model.IsEnabledShowControls, Is.True);
                Assert.That(model.Controls, Has.Count.EqualTo(1));
                Assert.That(model.Controls[0].Name, Is.EqualTo("Brightness"));
                Assert.That(model.Controls[0].Value, Is.EqualTo(37));
                await model.StopCapture.ExecuteAsync(null);
            }
        }
        finally { await model.CloseAsync(); }
    }

    [AvaloniaTest]
    public async Task CameraWithoutControlsStillCaptures()
    {
        var camera = new CameraDescriptor(false);
        var model = new MainWindowViewModel { Device = camera };
        try
        {
            await model.StartCapture.ExecuteAsync(null);
            Assert.That(camera.Camera.IsRunning, Is.True);
            Assert.That(model.IsEnabledStopCapture, Is.True);
            Assert.That(model.IsEnabledShowControls, Is.False);
        }
        finally { await model.CloseAsync(); }
    }

    private sealed class CameraDescriptor(bool offersBrightness) : CaptureDeviceDescriptor(
        "Test camera", "", new[] { new VideoCharacteristics(PixelFormats.YUYV, 4, 2, 30) },
        new DefaultBufferPool())
    {
        public readonly CameraDevice Camera = new(offersBrightness);
        public override object Identity => "test";
        public override DeviceTypes DeviceType => DeviceTypes.PipeWire;

        protected override Task<CaptureDevice> OnOpenWithFrameProcessorAsync(
            VideoCharacteristics characteristics, TranscodeFormats transcodeFormat,
            FrameProcessor frameProcessor, CancellationToken ct)
        {
            this.Camera.Processor = frameProcessor;
            return Task.FromResult<CaptureDevice>(this.Camera);
        }
    }

    private sealed class CameraDevice(bool offersBrightness) : CaptureDevice("test", "Test camera")
    {
        public FrameProcessor? Processor;

        protected override Task<CaptureDeviceControl[]> OnGetControlsAsync(CancellationToken ct) =>
            Task.FromResult(offersBrightness && this.IsRunning ?
                new[] { new CaptureDeviceControl(CaptureDeviceControlTypes.Brightness, 0, 100, 1, 50, false) } :
                Array.Empty<CaptureDeviceControl>());
        protected override Task<CaptureDeviceControlValue?> OnGetControlValueAsync(CaptureDeviceControlTypes type, CancellationToken ct) =>
            Task.FromResult<CaptureDeviceControlValue?>(new CaptureDeviceControlValue(37, false));
        protected override Task OnInitializeAsync(VideoCharacteristics characteristics,
            TranscodeFormats transcodeFormat, FrameProcessor frameProcessor, CancellationToken ct) => Task.CompletedTask;
        protected override Task OnStartAsync(CancellationToken ct) { this.IsRunning = true; return Task.CompletedTask; }
        protected override Task OnStopAsync(CancellationToken ct) { this.IsRunning = false; return Task.CompletedTask; }
        protected override void OnCapture(IntPtr data, int size, long timestamp, long index, PixelBuffer buffer) { }
        protected override async Task OnDisposeAsync()
        {
            this.IsRunning = false;
            if (this.Processor is { } processor) await processor.DisposeAsync();
        }
    }
}
