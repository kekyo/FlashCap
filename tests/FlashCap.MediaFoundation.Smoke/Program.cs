////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Devices;
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
    private static async Task<int> Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        var output = args.Length == 0 ? "camera-controls-results" : args[0];
        Directory.CreateDirectory(output);
        var descriptors = new MediaFoundationDevices().GetDescriptors();
        Console.WriteLine($"Cameras: {descriptors.Length}");
        if (descriptors.Length < 2)
        {
            Console.Error.WriteLine("This hardware test requires two cameras.");
            return 1;
        }
        var failures = 0;
        foreach (var descriptor in descriptors)
        {
            try
            {
                await VerifyAsync(descriptor, output);
                Console.WriteLine($"PASS: {descriptor.Name}");
            }
            catch (Exception exception)
            {
                failures++;
                Console.Error.WriteLine($"FAIL: {descriptor.Name}: {exception}");
            }
        }
        return failures == 0 ? 0 : 1;
    }

    private static async Task VerifyAsync(CaptureDeviceDescriptor descriptor, string output)
    {
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
            Require(controls.Length > 0, "No controls were exposed before StartAsync.");
            foreach (var control in controls)
            {
                var value = await device.GetControlValueAsync(control.Type);
                Require(value is not null, $"Cannot read {control.Type}.");
                originals.Add(control.Type, value!);
                Console.WriteLine($"CONTROL: {control}: current={value}");
            }
            Require(await device.GetControlValueAsync((CaptureDeviceControlTypes)999) is null, "Unknown control must return null.");
            Require(!await device.SetControlValueAsync((CaptureDeviceControlTypes)999, 0), "Unknown control must return false.");
            var brightness = controls.First(control => control.Type == CaptureDeviceControlTypes.Brightness && control.Maximum > control.Minimum);
            var originalBrightness = originals[brightness.Type].Value;
            var step = Math.Max(1, brightness.Step);
            var changedBrightness = originalBrightness + (long)step <= brightness.Maximum ? originalBrightness + step : originalBrightness - step;

            await SetAndReadAsync(device, brightness.Type, changedBrightness, false, "before-start");
            await device.StartAsync();
            await SaveFramesAsync("started");
            Require((await device.GetControlValueAsync(brightness.Type))!.Value == changedBrightness, "StartAsync lost the pre-start value.");
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
            Require(!device.IsRunning, "StopAsync did not stop the device.");
            Require((await device.GetControlsAsync()).Length == controls.Length, "StopAsync lost the controls.");
            await SetAndReadAsync(device, brightness.Type, changedBrightness, false, "stopped");
            await device.StartAsync();
            await SaveFramesAsync("restarted");
            Require((await device.GetControlValueAsync(brightness.Type))!.Value == changedBrightness, "Restart lost the stopped value.");
            await device.StopAsync();
        }
        finally
        {
            try
            {
                foreach (var original in originals)
                {
                    Require(await device.SetControlValueAsync(original.Key, original.Value.Value, original.Value.IsAuto),
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
            Require(await Task.WhenAny(frames, deadline) == frames, $"No frames arrived during {stage}.");
            timeout.Cancel();
            var image = await frames;
            var name = string.Concat(descriptor.Name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
            File.WriteAllBytes(Path.Combine(output, $"{name}-{stage}.jpg"), image);
            Console.WriteLine($"FRAMES: {stage}: count={frameCount}, image={image.Length} bytes");
        }
    }

    private static async Task SetAndReadAsync(CaptureDevice device, CaptureDeviceControlTypes type, int value, bool isAuto, string stage)
    {
        Require(await device.SetControlValueAsync(type, value, isAuto), $"{stage}: Set {type} was refused.");
        var actual = await device.GetControlValueAsync(type);
        Require(actual is not null && actual.IsAuto == isAuto && (isAuto || actual.Value == value),
            $"{stage}: {type}: expected {value}, auto={isAuto}; actual {actual}.");
        Console.WriteLine($"SET/GET: {stage}: {type}: {actual}");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
