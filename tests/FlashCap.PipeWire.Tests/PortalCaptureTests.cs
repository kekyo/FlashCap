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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.PipeWire.Tests;

[TestFixture]
[SupportedOSPlatform("linux")]
public sealed class PortalCaptureTests
{
    [TestCase("allow", false)]
    [TestCase("allow", true)]
    [TestCase("direct", false)]
    [TestCase("direct", true)]
    [TestCase("controls-readonly", false)]
    [TestCase("direct-controls-readonly", false)]
    [TestCase("controls-ignored", false)]
    [TestCase("controls-unavailable", false)]
    [TestCase("direct-empty", false)]
    [TestCase("partial", false)]
    [TestCase("partial", true)]
    [TestCase("partial-deny", false)]
    [TestCase("partial-readonly-deny", false)]
    [TestCase("readonly", false)]
    [TestCase("duplicate", false)]
    [TestCase("delayed", false)]
    [TestCase("deny", false)]
    [TestCase("deny", true)]
    [TestCase("cancel", false)]
    [TestCase("cancel", true)]
    [TestCase("wait", false)]
    [TestCase("vanish", false)]
    [TestCase("vanish", true)]
    [TestCase("absent", false)]
    [TestCase("absent", true)]
    public async Task CameraPortalAndNativeCapture(string mode, bool aggregate)
    {
        var nativeDirectory = Environment.GetEnvironmentVariable("FLASHCAP_PIPEWIRE_NATIVE_DIRECTORY");
        Assert.That(nativeDirectory, Is.Not.Null, "Run these integration tests with bash scripts/test-pipewire.sh.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var fixture = new Process
        {
            StartInfo = new ProcessStartInfo(Path.Combine(nativeDirectory!, "fixture"))
            {
                ArgumentList = { Path.Combine(nativeDirectory!, "pipewire.conf"), mode },
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            },
        };
        fixture.Start();
        var errors = fixture.StandardError.ReadToEndAsync();
        using var trace = new TextWriterTraceListener(TestContext.Out);
        Trace.Listeners.Add(trace);
        try
        {
            Assert.That(await fixture.StandardOutput.ReadLineAsync(timeout.Token), Is.EqualTo("READY"));
            TestContext.Out.WriteLine("The synthetic camera and portal are ready.");
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
            CaptureDevices devices = aggregate ? new CaptureDevices() : new PipeWireDevices();
            var enumeration = devices.GetDescriptorsAsync(cancellation.Token);
            if (mode == "wait")
            {
                Assert.That(await fixture.StandardOutput.ReadLineAsync(timeout.Token), Is.EqualTo("REQUESTED"));
                cancellation.Cancel();
                Assert.CatchAsync<OperationCanceledException>(async () => await enumeration);
                Assert.That(await fixture.StandardOutput.ReadLineAsync(timeout.Token), Is.EqualTo("CLOSED"));
            }
            else if (mode is "deny" or "cancel" or "vanish" or "absent" or "direct-empty")
            {
                var expected = aggregate ? new V4L2Devices().GetDescriptors() : Array.Empty<CaptureDeviceDescriptor>();
                Assert.That((await enumeration).Select(device => device.Identity),
                    Is.EquivalentTo(expected.Select(device => device.Identity)));
                var streamed = new List<CaptureDeviceDescriptor>();
                await foreach (var device in devices.EnumerateDescriptorsAsync(timeout.Token)) streamed.Add(device);
                Assert.That(streamed.Select(device => device.Identity),
                    Is.EquivalentTo(expected.Select(device => device.Identity)));
            }
            else
            {
                var descriptors = await enumeration;
                TestContext.Out.WriteLine("Camera enumeration completed.");
                if (aggregate)
                {
                    var v4l2 = new V4L2Devices();
                    var synchronous = v4l2.GetDescriptors().Select(descriptor => descriptor.Identity).ToArray();
                    var defaultSynchronous = devices.GetDescriptors();
                    Assert.That(defaultSynchronous.Select(descriptor => descriptor.DeviceType), Has.All.EqualTo(DeviceTypes.V4L2));
                    Assert.That(defaultSynchronous.Select(descriptor => descriptor.Identity), Is.EquivalentTo(synchronous));
                    var asynchronous = await v4l2.GetDescriptorsAsync(timeout.Token);
                    Assert.That(asynchronous.Select(descriptor => descriptor.DeviceType), Has.All.EqualTo(DeviceTypes.V4L2));
                    Assert.That(asynchronous.Select(descriptor => descriptor.Identity), Is.EquivalentTo(synchronous));
                    Assert.That(descriptors.Where(descriptor => descriptor.DeviceType == DeviceTypes.V4L2)
                        .Select(descriptor => descriptor.Identity), Is.EquivalentTo(synchronous));
                }
                else
                    Assert.That(descriptors, Has.Length.EqualTo(mode == "partial" ? 2 : 1));
                var pipeWireDescriptors = descriptors.Where(descriptor => descriptor.DeviceType == DeviceTypes.PipeWire).ToArray();
                Assert.That(pipeWireDescriptors, Has.Length.EqualTo(mode == "partial" ? 2 : 1));
                if (mode.StartsWith("partial", StringComparison.Ordinal))
                    Assert.That(await fixture.StandardOutput.ReadLineAsync(timeout.Token), Is.EqualTo("REQUESTED"));
                var descriptor = pipeWireDescriptors.OrderBy(device => device.Name).Last();
                Assert.That(descriptor.DeviceType, Is.EqualTo(DeviceTypes.PipeWire));
                Assert.That(descriptor.Characteristics, Has.Length.EqualTo(1));
                var format = descriptor.Characteristics.Single();
                Assert.That(format.PixelFormat, Is.EqualTo(PixelFormats.NV12));
                Assert.That(format.Width, Is.EqualTo(4));
                Assert.That(format.Height, Is.EqualTo(2));

                var frames = new List<byte[]>();
                var received = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var device = await descriptor.OpenAsync(format, TranscodeFormats.DoNotTranscode, scope =>
                {
                    lock (frames)
                    {
                        frames.Add(scope.Buffer.CopyImage());
                        if (frames.Count >= 4) received.TrySetResult(true);
                    }
                }, timeout.Token);
                TestContext.Out.WriteLine("Camera opened.");
                try
                {
                    for (var run = 0; run < 2; run++)
                    {
                        await device.StartAsync(timeout.Token);
                        TestContext.Out.WriteLine("Capture started.");
                        Assert.That(device.IsRunning, Is.True);
                        var controls = await device.GetControlsAsync(timeout.Token);
                        if (mode == "controls-unavailable")
                        {
                            Assert.That(controls, Is.Empty);
                            Assert.That(await device.GetControlValueAsync(CaptureDeviceControlTypes.Brightness, timeout.Token), Is.Null);
                            Assert.That(await device.SetControlValueAsync(CaptureDeviceControlTypes.Brightness, 128, false, timeout.Token), Is.False);
                        }
                        else
                        {
                            var brightness = controls.Single(control => control.Type == CaptureDeviceControlTypes.Brightness);
                            Assert.That(brightness.Minimum, Is.Zero);
                            Assert.That(brightness.Maximum, Is.EqualTo(255));
                            Assert.That(brightness.DefaultValue, Is.EqualTo(128));
                            var original = await device.GetControlValueAsync(brightness.Type, timeout.Token);
                            Assert.That(original, Is.Not.Null);
                            var writable = !mode.Contains("controls-readonly", StringComparison.Ordinal) && mode != "controls-ignored";
                            Assert.That(await device.SetControlValueAsync(brightness.Type, 140 + run, false, timeout.Token), Is.EqualTo(writable));
                            Assert.That((await device.GetControlValueAsync(brightness.Type, timeout.Token))!.Value,
                                Is.EqualTo(writable ? 140 + run : original!.Value));
                            Assert.That(await device.SetControlValueAsync(brightness.Type, 256, false, timeout.Token), Is.False);
                            Assert.That(await device.SetControlValueAsync(brightness.Type, 128, true, timeout.Token), Is.False);
                        }
                        Assert.That(await device.SetControlValueAsync(CaptureDeviceControlTypes.Roll, 1, false, timeout.Token), Is.False);
                        Assert.That(await device.GetControlValueAsync(CaptureDeviceControlTypes.Roll, timeout.Token), Is.Null);
                        // Count frames arriving after the rejected operations, not frames already buffered.
                        lock (frames)
                        {
                            frames.Clear();
                            received = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                        }
                        await received.Task.WaitAsync(timeout.Token);
                        await device.StopAsync(timeout.Token);
                        Assert.That(device.IsRunning, Is.False);
                        lock (frames)
                        {
                            Assert.That(frames.Count, Is.GreaterThanOrEqualTo(4));
                            foreach (var frame in frames)
                            {
                                Assert.That(frame.Length, Is.EqualTo(54 + 12));
                                Assert.That(frame.Skip(54), Is.EqualTo(Enumerable.Range(mode == "partial" ? 101 : 1, 12).Select(value => (byte)value)));
                            }
                            frames.Clear();
                            received = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                        }
                    }
                }
                finally { await device.DisposeAsync(); }
                Assert.That(await device.GetControlsAsync(timeout.Token), Is.Empty);
                Assert.That(await device.SetControlValueAsync(CaptureDeviceControlTypes.Brightness, 128, false, timeout.Token), Is.False);
                foreach (var executable in (Environment.GetEnvironmentVariable("FLASHCAP_PIPEWIRE_AOT_BINARIES") ?? "")
                    .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    using var consumer = Process.Start(new ProcessStartInfo(executable)
                    {
                        ArgumentList = { "--pipewire" },
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                    })!;
                    var output = consumer.StandardOutput.ReadToEndAsync();
                    var diagnostics = consumer.StandardError.ReadToEndAsync();
                    try { await consumer.WaitForExitAsync(timeout.Token); }
                    finally
                    {
                        if (!consumer.HasExited) { consumer.Kill(); await consumer.WaitForExitAsync(); }
                    }
                    Assert.That(consumer.ExitCode, Is.Zero, await diagnostics);
                    Assert.That(await output, Does.Contain("PipeWire AOT capture passed."));
                }
            }
        }
        finally
        {
            Trace.Listeners.Remove(trace);
            if (!fixture.HasExited)
            {
                await fixture.StandardInput.WriteLineAsync("quit");
                try { await fixture.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
                catch (TimeoutException) { fixture.Kill(); await fixture.WaitForExitAsync(); }
            }
            TestContext.Out.Write(await errors);
            Assert.That(fixture.ExitCode, Is.Zero);
        }
    }
}
