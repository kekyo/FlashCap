////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Devices;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Properties;

public static class Program
{
    private static async Task<int> SetCameraPropertyAsync(CancellationToken ct)
    {
        ///////////////////////////////////////////////////////////////
        // Initialize and detection capture devices.

        // Step 1: Enumerate capture devices:
        var devices = new CaptureDevices();
        var descriptor0 = devices.EnumerateDescriptors()
            .Where(d => d.Characteristics.Length >= 1)
            // Only DirectShow Properties are available
            .Where(d => d.DeviceType == DeviceTypes.DirectShow)
            .FirstOrDefault();
        if (descriptor0 == null)
        {
            Console.WriteLine($"Could not detect any capture interfaces.");
            return 0;
        }

        var characteristics0 = descriptor0.Characteristics.
            FirstOrDefault();
        if (characteristics0 == null)
        {
            Console.WriteLine($"Could not select primary characteristics.");
            return 0;
        }

        Console.WriteLine($"Selected capture device: {descriptor0}, {characteristics0}");

        ///////////////////////////////////////////////////////////////

        var captureDevice = await descriptor0.OpenAsync(
            characteristics0,
            bufferScope =>
            {
                var image = bufferScope.Buffer.CopyImage();

                Console.WriteLine($"Captured {image.Length} bytes.");

                bufferScope.ReleaseNow();
            },
            ct);

        await captureDevice.ShowPropertyPageAsync(IntPtr.Zero);

        foreach (var property in captureDevice.Properties)
        {
            Console.WriteLine($"Supported proprety {property.Key} - min value {property.Value.Min} - max value {property.Value.Max} - step {property.Value.Step}");
        }

        var brightnessProperty = captureDevice.Properties.Where(x => x.Key == VideoProcessingAmplifierProperty.Brightness).Select(x => x.Value).FirstOrDefault() ?? throw new Exception("Brightness is not supported with current device");

        var originalBrightnessValue = await captureDevice.GetPropertyValueAsync(
            VideoProcessingAmplifierProperty.Brightness);
        Console.WriteLine($"Current brightness property value is {originalBrightnessValue}");

        await captureDevice.SetPropertyValueAsync(
            VideoProcessingAmplifierProperty.Brightness,
            brightnessProperty.Min);
        Console.WriteLine($"Brightness property value updated to {brightnessProperty.Min}");

        var brightness = await captureDevice.GetPropertyValueAsync(
            VideoProcessingAmplifierProperty.Brightness);
        Console.WriteLine($"Brightness property value is {brightness}");

        await captureDevice.SetPropertyValueAsync(
            VideoProcessingAmplifierProperty.Brightness,
            originalBrightnessValue);
        Console.WriteLine($"Brightness property value restored to {originalBrightnessValue}");

        return 0;
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            return await SetCameraPropertyAsync(default);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return Marshal.GetHRForException(ex);
        }
    }
}
