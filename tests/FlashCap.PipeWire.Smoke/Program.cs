////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using NUnitLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.PipeWire.Smoke;

internal static class Program
{
    private static int Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        Console.WriteLine($"Flatpak: {File.Exists("/.flatpak-info")}");
        return new AutoRun().Execute(args);
    }
}

[TestFixture]
#if NET5_0_OR_GREATER
[SupportedOSPlatform("linux")]
#endif
internal sealed class C930eCaptureTests
{
    private DeviceTypes[] expectedBackends = null!;
    private CaptureDeviceDescriptor[] descriptors = null!;
    private CaptureDeviceDescriptor[] cameras = null!;

    /// <summary>Finds the host C930e through the available default Linux backends.</summary>
    [OneTimeSetUp]
    public async Task EnumerateCameraAsync()
    {
        var backends = TestContext.Parameters.Get("backends", "V4L2,PipeWire");
        this.expectedBackends = backends == "none" ? Array.Empty<DeviceTypes>() :
            backends.Split(',').Select(value => (DeviceTypes)Enum.Parse(typeof(DeviceTypes), value)).ToArray();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        TestContext.Progress.WriteLine("Enumerating cameras through the default Linux backends...");
        this.descriptors = await new CaptureDevices().GetDescriptorsAsync(timeout.Token);
        foreach (var descriptor in this.descriptors)
            TestContext.Progress.WriteLine($"DEVICE: {descriptor.DeviceType}, {descriptor.Name}, {descriptor.Identity}, formats={descriptor.Characteristics.Length}");
        this.cameras = this.descriptors.Where(descriptor => descriptor.Characteristics.Length != 0 &&
            descriptor.Name.IndexOf("C930e", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
        foreach (var camera in this.cameras)
            foreach (var group in camera.Characteristics.GroupBy(format => format.PixelFormat))
                TestContext.Progress.WriteLine($"FORMATS: {camera.DeviceType}, {group.Key}, count={group.Count()}");
    }

    /// <summary>Checks that permissions control enumeration without producing an error.</summary>
    [Test]
    public void EnumeratesOnlyAvailableBackends()
    {
        Assert.That(this.descriptors.Select(camera => camera.DeviceType).Distinct(), Is.EquivalentTo(this.expectedBackends));
        Assert.That(this.cameras.Select(camera => camera.DeviceType), Is.EquivalentTo(this.expectedBackends));
        var synchronous = new CaptureDevices().EnumerateDescriptors().ToArray();
        Assert.That(synchronous.Select(camera => camera.DeviceType).Distinct(),
            Is.EquivalentTo(this.expectedBackends.Where(backend => backend == DeviceTypes.V4L2)));
    }

    /// <summary>Changes and restores C930e controls while verifying capture survives rejected requests.</summary>
    /// <param name="deviceType">The capture backend.</param>
    [TestCase(DeviceTypes.V4L2)]
    [TestCase(DeviceTypes.PipeWire)]
    public async Task ControlsAndCaptureAsync(DeviceTypes deviceType)
    {
        if (!this.expectedBackends.Contains(deviceType))
            Assert.Ignore($"{deviceType} is intentionally unavailable in this permission scenario.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var camera = this.cameras.Single(candidate => candidate.DeviceType == deviceType);
        var format = camera.Characteristics.First(candidate => candidate.PixelFormat == PixelFormats.YUYV &&
            candidate.Width == 640 && candidate.Height == 480);
        var received = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var frames = 0;
        var device = await camera.OpenAsync(format, TranscodeFormats.DoNotTranscode, _ =>
        {
            if (Interlocked.Increment(ref frames) >= 10) received.TrySetResult(true);
        }, timeout.Token);
        CaptureDeviceControlValue? originalBrightness = null;
        CaptureDeviceControlValue? originalFocus = null;
        var brightnessChanged = false;
        try
        {
            var controls = await device.GetControlsAsync(timeout.Token);
            var availableBeforeCapture = controls.Length != 0;
            if (controls.Length == 0 && deviceType == DeviceTypes.PipeWire)
            {
                // A portal/session manager may expose the source node only once
                // the capture stream is created. Missing controls must be harmless.
                await device.StartAsync(timeout.Token);
                controls = await device.GetControlsAsync(timeout.Token);
            }
            foreach (var control in controls)
                TestContext.Progress.WriteLine($"CONTROL: {deviceType}, {control}, current={await device.GetControlValueAsync(control.Type, timeout.Token)}");
            var brightness = controls.Single(control => control.Type == CaptureDeviceControlTypes.Brightness);
            originalBrightness = await device.GetControlValueAsync(brightness.Type, timeout.Token);
            Assert.That(originalBrightness, Is.Not.Null);
            var changed = originalBrightness!.Value < brightness.Maximum ?
                originalBrightness.Value + brightness.Step : originalBrightness.Value - brightness.Step;
            var writable = await device.SetControlValueAsync(brightness.Type, changed, false, timeout.Token);
            brightnessChanged = writable;
            // A sandbox's direct PipeWire connection can have RX permission only,
            // even with the socket and /dev/video* exposed. Verify that route too.
            Assert.That(writable || (File.Exists("/.flatpak-info") && deviceType == DeviceTypes.PipeWire), Is.True);
            Assert.That((await device.GetControlValueAsync(brightness.Type, timeout.Token))!.Value,
                Is.EqualTo(writable ? changed : originalBrightness.Value));
            await device.StartAsync(timeout.Token);
            Assert.That(await device.SetControlValueAsync(brightness.Type, originalBrightness.Value, false, timeout.Token), Is.EqualTo(writable));
            if (writable)
            {
                var focus = controls.Single(control => control.Type == CaptureDeviceControlTypes.Focus);
                Assert.That(focus.SupportsAuto, Is.True);
                originalFocus = await device.GetControlValueAsync(focus.Type, timeout.Token);
                Assert.That(originalFocus, Is.Not.Null);
                Assert.That(await device.SetControlValueAsync(focus.Type, originalFocus!.Value, !originalFocus.IsAuto, timeout.Token), Is.True);
                Assert.That((await device.GetControlValueAsync(focus.Type, timeout.Token))!.IsAuto, Is.EqualTo(!originalFocus.IsAuto));
            }
            // Invalid values are rejected by the PipeWire adapter before reaching the camera.
            if (deviceType == DeviceTypes.PipeWire)
                Assert.That(await device.SetControlValueAsync(brightness.Type, brightness.Maximum + 1, false, timeout.Token), Is.False);
            Assert.That(await device.SetControlValueAsync(CaptureDeviceControlTypes.Roll, 1, false, timeout.Token), Is.False);
            Interlocked.Exchange(ref frames, 0);
            received = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (timeout.Token.Register(() => received.TrySetCanceled())) await received.Task;
            Assert.That(device.IsRunning, Is.True);
            await device.StopAsync(timeout.Token);
            Assert.That(await device.SetControlValueAsync(brightness.Type, changed, false, timeout.Token), Is.EqualTo(writable));
            TestContext.Progress.WriteLine($"CONTROLS PASSED: {deviceType}, writable={writable}, availableBeforeCapture={availableBeforeCapture}, capture survives rejected requests");
        }
        finally
        {
            try
            {
                // Restore settings even if an assertion fails or the main timeout expires.
                using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try
                {
                    if (originalFocus is not null)
                        Assert.That(await device.SetControlValueAsync(CaptureDeviceControlTypes.Focus, originalFocus.Value, originalFocus.IsAuto, cleanup.Token), Is.True);
                }
                finally
                {
                    if (brightnessChanged && originalBrightness is not null)
                        Assert.That(await device.SetControlValueAsync(CaptureDeviceControlTypes.Brightness, originalBrightness.Value, originalBrightness.IsAuto, cleanup.Token), Is.True);
                }
            }
            finally { await device.DisposeAsync(); }
        }
    }

    /// <summary>Checks captured images, timestamps, and restarting the same camera.</summary>
    /// <param name="deviceType">The capture backend.</param>
    /// <param name="pixelFormat">The camera's pixel format.</param>
    /// <param name="width">The capture width in pixels.</param>
    /// <param name="height">The capture height in pixels.</param>
    /// <param name="transcode">The requested output conversion.</param>
    [TestCase(DeviceTypes.V4L2, PixelFormats.YUYV, 640, 480, TranscodeFormats.DoNotTranscode)]
    [TestCase(DeviceTypes.V4L2, PixelFormats.YUYV, 640, 480, TranscodeFormats.Auto)]
    [TestCase(DeviceTypes.V4L2, PixelFormats.JPEG, 1920, 1080, TranscodeFormats.DoNotTranscode)]
    [TestCase(DeviceTypes.PipeWire, PixelFormats.YUYV, 640, 480, TranscodeFormats.DoNotTranscode)]
    [TestCase(DeviceTypes.PipeWire, PixelFormats.YUYV, 640, 480, TranscodeFormats.Auto)]
    [TestCase(DeviceTypes.PipeWire, PixelFormats.JPEG, 1920, 1080, TranscodeFormats.DoNotTranscode)]
    public async Task CaptureAndRestartAsync(DeviceTypes deviceType, PixelFormats pixelFormat, int width, int height, TranscodeFormats transcode)
    {
        if (!this.expectedBackends.Contains(deviceType))
            Assert.Ignore($"{deviceType} is intentionally unavailable in this permission scenario.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var camera = this.cameras.Single(candidate => candidate.DeviceType == deviceType);
        var format = camera.Characteristics.Single(candidate =>
            candidate.PixelFormat == pixelFormat && candidate.Width == width && candidate.Height == height &&
            candidate.FramesPerSecond.Numerator == 30 * candidate.FramesPerSecond.Denominator);
        TestContext.Progress.WriteLine($"OPEN: {deviceType}, {camera.Name}, {format}, {transcode}");
        var sync = new object();
        var timestamps = new List<TimeSpan>();
        var indices = new List<long>();
        var completed = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        var accepting = true;
        var device = await camera.OpenAsync(format, transcode, scope =>
        {
            lock (sync)
            {
                if (!accepting) return;
                timestamps.Add(scope.Buffer.Timestamp);
                indices.Add(scope.Buffer.FrameIndex);
                if (indices.Count == 60)
                {
                    accepting = false;
                    completed.TrySetResult(scope.Buffer.CopyImage());
                }
            }
        }, timeout.Token);
        try
        {
            for (var run = 1; run <= 2; run++)
            {
                await device.StartAsync(timeout.Token);
                Assert.That(device.IsRunning, Is.True);
                byte[] image;
                using (timeout.Token.Register(() => completed.TrySetCanceled()))
                    image = await completed.Task;
                await device.StopAsync(timeout.Token);
                Assert.That(device.IsRunning, Is.False);
                long[] receivedIndices;
                TimeSpan[] receivedTimestamps;
                lock (sync)
                {
                    receivedIndices = indices.ToArray();
                    receivedTimestamps = timestamps.ToArray();
                }
                Assert.That(receivedIndices, Has.Length.EqualTo(60));
                Assert.That(receivedIndices.Zip(receivedIndices.Skip(1), (previous, next) => next > previous).All(value => value), Is.True);
                Assert.That(receivedTimestamps.Zip(receivedTimestamps.Skip(1), (previous, next) => next > previous).All(value => value), Is.True);
                if (pixelFormat == PixelFormats.JPEG)
                {
                    Assert.That(image.Take(2), Is.EqualTo(new byte[] { 0xff, 0xd8 }));
                    Assert.That(image.Skip(image.Length - 2), Is.EqualTo(new byte[] { 0xff, 0xd9 }));
                }
                else
                {
                    Assert.That(image.Take(2), Is.EqualTo(new byte[] { 0x42, 0x4d }));
                    Assert.That(BitConverter.ToInt32(image, 18), Is.EqualTo(width));
                    Assert.That(BitConverter.ToInt32(image, 22), Is.EqualTo(height));
                    Assert.That(BitConverter.ToInt16(image, 28), Is.EqualTo(transcode == TranscodeFormats.DoNotTranscode ? 16 : 24));
                    Assert.That(image.Length, Is.EqualTo(54 + width * height * (transcode == TranscodeFormats.DoNotTranscode ? 2 : 3)));
                }
                var name = $"c930e-{deviceType}-{pixelFormat}-{width}x{height}-{transcode}-run{run}" +
                    (pixelFormat == PixelFormats.JPEG ? ".jpg" : ".bmp");
                var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, name);
                File.WriteAllBytes(path, image);
                var duration = receivedTimestamps.Last() - receivedTimestamps.First();
                var fps = (receivedTimestamps.Length - 1) / duration.TotalSeconds;
                TestContext.Progress.WriteLine($"CAPTURED: run={run}, frames={receivedIndices.Length}, fps={fps:F2}, bytes={image.Length}, file={path}");
                lock (sync)
                {
                    timestamps.Clear();
                    indices.Clear();
                    completed = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
                    accepting = true;
                }
            }
        }
        finally { await device.DisposeAsync(); }
    }
}
