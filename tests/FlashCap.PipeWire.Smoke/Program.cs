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
    private CaptureDeviceDescriptor[] cameras = null!;

    /// <summary>Finds the host C930e through both default Linux backends.</summary>
    [OneTimeSetUp]
    public async Task EnumerateCameraAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        TestContext.Progress.WriteLine("Enumerating cameras through the default Linux backends...");
        var descriptors = await new CaptureDevices().GetDescriptorsAsync(timeout.Token);
        foreach (var descriptor in descriptors)
            TestContext.Progress.WriteLine($"DEVICE: {descriptor.DeviceType}, {descriptor.Name}, {descriptor.Identity}, formats={descriptor.Characteristics.Length}");
        this.cameras = descriptors.Where(descriptor => descriptor.Characteristics.Length != 0 &&
            descriptor.Name.IndexOf("C930e", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
        Assert.That(this.cameras.Select(camera => camera.DeviceType), Is.EquivalentTo(new[] { DeviceTypes.V4L2, DeviceTypes.PipeWire }));
        foreach (var camera in this.cameras)
            foreach (var group in camera.Characteristics.GroupBy(format => format.PixelFormat))
                TestContext.Progress.WriteLine($"FORMATS: {camera.DeviceType}, {group.Key}, count={group.Count()}");
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
