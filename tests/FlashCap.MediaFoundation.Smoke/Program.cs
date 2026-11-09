////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Devices;
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

namespace FlashCap.MediaFoundation.Smoke;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows6.1")]
#endif
internal static class Program
{
    private static int Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        return new AutoRun().Execute(args);
    }
}

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows6.1")]
#endif
[TestFixture]
internal sealed class CameraControlsTests
{
    private static readonly CaptureDeviceDescriptor[] descriptors = new MediaFoundationDevices().GetDescriptors();

    private static IEnumerable<TestCaseData> Cameras => descriptors.Select(descriptor =>
        new TestCaseData(descriptor).SetName($"Camera controls: {descriptor.Name}"));

    [Test]
    public void DetectsTwoCameras() =>
        Assert.That(descriptors.Length, Is.GreaterThanOrEqualTo(2), "This hardware test requires two cameras.");

    [TestCaseSource(nameof(Cameras))]
    public async Task VerifyAsync(CaptureDeviceDescriptor descriptor)
    {
        var output = TestContext.CurrentContext.WorkDirectory;
        Directory.CreateDirectory(output);
        var format = descriptor.Characteristics
            .Where(format => format.PixelFormat == PixelFormats.JPEG)
            .OrderBy(format => Math.Abs(format.Width - 640) + Math.Abs(format.Height - 480))
            .ThenBy(format => Math.Abs((double)format.FramesPerSecond - 30))
            .First();
        Console.WriteLine($"OPEN: {descriptor.Name}: {format}");
        var sync = new object();
        var frameCount = 0;
        var targetFrames = int.MaxValue;
        var nextFrames = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        var device = await descriptor.OpenAsync(format, buffer =>
        {
            lock (sync)
            {
                frameCount++;
                if (frameCount >= targetFrames)
                {
                    nextFrames.TrySetResult(buffer.Buffer.CopyImage());
                    targetFrames = int.MaxValue;
                }
            }
        });
        var originals = new Dictionary<CaptureDeviceControlTypes, CaptureDeviceControlValue>();
        try
        {
            var controls = await device.GetControlsAsync();
            Assert.That(controls.Length > 0, "No controls were exposed before StartAsync.");
            foreach (var control in controls)
            {
                var value = await device.GetControlValueAsync(control.Type);
                Assert.That(value is not null, $"Cannot read {control.Type}.");
                originals.Add(control.Type, value!);
                Console.WriteLine($"CONTROL: {control}: current={value}");
            }
            Assert.That(await device.GetControlValueAsync((CaptureDeviceControlTypes)999) is null, "Unknown control must return null.");
            Assert.That(!await device.SetControlValueAsync((CaptureDeviceControlTypes)999, 0), "Unknown control must return false.");
            var brightness = controls.First(control => control.Type == CaptureDeviceControlTypes.Brightness && control.Maximum > control.Minimum);
            var originalBrightness = originals[brightness.Type].Value;
            var step = Math.Max(1, brightness.Step);
            var changedBrightness = originalBrightness + (long)step <= brightness.Maximum ? originalBrightness + step : originalBrightness - step;

            await SetAndReadAsync(device, brightness.Type, changedBrightness, false, "before-start");
            await device.StartAsync();
            await SaveFramesAsync("started");
            Assert.That((await device.GetControlValueAsync(brightness.Type))!.Value == changedBrightness, "StartAsync lost the pre-start value.");
            await SetAndReadAsync(device, brightness.Type, originalBrightness, false, "running");

            foreach (var control in controls.Where(control => control.SupportsAuto))
            {
                // Auto ignores the supplied value; exercise a value outside every ordinary range.
                await SetAndReadAsync(device, control.Type, int.MaxValue, true, "auto");
                var value = originals[control.Type].Value;
                value = Math.Max(control.Minimum, Math.Min(control.Maximum, value));
                value = control.Minimum + (value - control.Minimum) / Math.Max(1, control.Step) * Math.Max(1, control.Step);
                await SetAndReadAsync(device, control.Type, value, false, "manual");
            }
            await SaveFramesAsync("controls-applied");

            // Device operations must remain usable when StopAsync is queued with control reads.
            var reads = Enumerable.Range(0, 8).Select(_ => device.GetControlValueAsync(brightness.Type)).ToArray();
            await device.StopAsync();
            await Task.WhenAll(reads);
            Assert.That(!device.IsRunning, "StopAsync did not stop the device.");
            Assert.That((await device.GetControlsAsync()).Length == controls.Length, "StopAsync lost the controls.");
            await SetAndReadAsync(device, brightness.Type, changedBrightness, false, "stopped");
            await device.StartAsync();
            await SaveFramesAsync("restarted");
            Assert.That((await device.GetControlValueAsync(brightness.Type))!.Value == changedBrightness, "Restart lost the stopped value.");
            await device.StopAsync();
        }
        finally
        {
            try
            {
                foreach (var original in originals)
                {
                    Assert.That(await device.SetControlValueAsync(original.Key, original.Value.Value, original.Value.IsAuto),
                        $"Cannot restore {original.Key}.");
                }
            }
            finally
            {
                await device.DisposeAsync();
            }
        }

        async Task SaveFramesAsync(string stage)
        {
            Task<byte[]> frames;
            lock (sync)
            {
                nextFrames = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
                targetFrames = frameCount + 6;
                frames = nextFrames.Task;
            }
            using var timeout = new CancellationTokenSource();
            var deadline = Task.Delay(TimeSpan.FromSeconds(30), timeout.Token);
            Assert.That(await Task.WhenAny(frames, deadline) == frames, $"No frames arrived during {stage}.");
            timeout.Cancel();
            var image = await frames;
            var name = string.Concat(descriptor.Name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
            File.WriteAllBytes(Path.Combine(output, $"{name}-{stage}.jpg"), image);
            Console.WriteLine($"FRAMES: {stage}: count={frameCount}, image={image.Length} bytes");
        }
    }

    private static async Task SetAndReadAsync(CaptureDevice device, CaptureDeviceControlTypes type, int value, bool isAuto, string stage)
    {
        Assert.That(await device.SetControlValueAsync(type, value, isAuto), $"{stage}: Set {type} was refused.");
        var actual = await device.GetControlValueAsync(type);
        Assert.That(actual is not null && actual.IsAuto == isAuto && (isAuto || actual.Value == value),
            $"{stage}: {type}: expected {value}, auto={isAuto}; actual {actual}.");
        Console.WriteLine($"SET/GET: {stage}: {type}: {actual}");
    }
}
