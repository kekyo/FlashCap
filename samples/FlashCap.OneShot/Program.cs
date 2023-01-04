////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.OneShot;

public static class Program
{
    private static async Task<int> TakeOneShotToFileAsync(
        string fileName, CancellationToken ct)
    {
        ///////////////////////////////////////////////////////////////
        // Initialize and detection capture devices.

        // Step 1: Enumerate capture devices:
        var devices = new CaptureDevices();
        var descriptor0 = devices.EnumerateDescriptors().
            // You could filter by device type and characteristics.
            //Where(d => d.DeviceType == DeviceTypes.DirectShow).  // Only DirectShow device.
            FirstOrDefault();
        if (descriptor0 == null)
        {
            Console.WriteLine($"Could not detect any capture interfaces.");
            return 0;
        }

#if false
        // Step 2-1: Request video characteristics strictly:
        // Will raise exception when parameters are not accepted.
        var characteristics = new VideoCharacteristics(
            PixelFormats.JPEG, 1920, 1080, 60);
#else
        // Step 2-2: Or, you could choice from device descriptor:
        var characteristics0 = descriptor0.Characteristics.
            //Where(c => c.PixelFormat == PixelFormats.JPEG).  // Only MJPEG characteristics.
            FirstOrDefault();
        if (characteristics0 == null)
        {
            Console.WriteLine($"Could not select primary characteristics.");
            return 0;
        }
#endif

        Console.WriteLine($"Selected capture device: {descriptor0}, {characteristics0}");

        ///////////////////////////////////////////////////////////////
        // Start capture and get one image.

        // Step 3-1: Open the capture device with specific characteristics:
        var tcs = new TaskCompletionSource<byte[]>();
        using var captureDevice = await descriptor0.OpenAsync(
            characteristics0,
            bufferScope =>
            {
                ////////////////////////////////////////////////
                // Pixel buffer has arrived.

                // Step 3-2: Copy image data binary:
                var image = bufferScope.Buffer.CopyImage();

                Console.WriteLine($"Captured {image.Length} bytes.");

                // Step 3-3: Relay to outside continuation.
                tcs.SetResult(image);

                // If you output to each files from continuous image data,
                // it would be easier to output directly to file here.
                // In that case, use:
                // * `isScattering` argument to true.
                // * `maxQueuingFrames` argument.
                // * `bufferScope.ReleaseNow()` method.
                // and be careful not to cause frame dropping.
            },
            ct);

        // Step 4: Start capturing:
        await captureDevice.StartAsync(ct);

        Console.WriteLine($"Device {descriptor0} opened.");

        // Step 5: Waiting to continue:
        var image = await tcs.Task;

        // Step 6: Stop capturing:
        await captureDevice.StopAsync(ct);

        Console.WriteLine($"Device {descriptor0} stopped.");

        ///////////////////////////////////////////////////////////////
        // Save image data to file.

        // Step 7: Construct storing file name:
        var extension = characteristics0.PixelFormat switch
        {
            PixelFormats.JPEG => ".jpg",
            PixelFormats.PNG => ".png",   // (Very rare device, I dont know)
            _ => ".bmp",
        };
        var path = $"{fileName}{extension}";

        // Step 8: Write to the file:
        using var fs = new FileStream(
            path,
            FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite,
            65536, true);
        await fs.WriteAsync(image, 0, image.Length, ct);
        await fs.FlushAsync(ct);

        Console.WriteLine($"The image wrote to file {path}.");

        return 0;
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            return await TakeOneShotToFileAsync("oneshot", default);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return Marshal.GetHRForException(ex);
        }
    }
}
