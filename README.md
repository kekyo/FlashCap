# FlashCap

![FlashCap](Images/FlashCap.100.png)

FlashCap - Independent camera capture library.

[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip)

## NuGet

|Package|NuGet|
|:--|:--|
|FlashCap|[![NuGet FlashCap](https://img.shields.io/nuget/v/FlashCap.svg?style=flat)](https://www.nuget.org/packages/FlashCap)|

## CI

|main|develop|
|:--|:--|
|[![FlashCap CI build (main)](https://github.com/kekyo/FlashCap/workflows/.NET/badge.svg?branch=main)](https://github.com/kekyo/FlashCap/actions?query=branch%3Amain)|[![FlashCap CI build (develop)](https://github.com/kekyo/FlashCap/workflows/.NET/badge.svg?branch=develop)](https://github.com/kekyo/FlashCap/actions?query=branch%3Adevelop)|

---

## What is this?

This is a simple camera image capture library.
By specializing only image capture, it is simple and has no any other library dependencies, [see NuGet summary page.](https://www.nuget.org/packages/FlashCap)

.NET platforms supported are as follows:

* .NET 6, 5 (net6.0, net5.0)
* .NET Core 3.1, 3.0, 2.1, 2.0 (netcoreapp3.1 and etc)
* .NET Standard 2.1, 2.0, 1.1 (netstandard2.1 and etc)
* .NET Framework 4.5, 4.0, 3.5, 2.0 (net45 and etc)

Platforms on which camera devices can be used:

* Windows (around Video For Windows API)
* TODO: Windows (DirectShow API)
* TODO: Linux (V2L2 API)

---

## How to use

For using Video For Windows API:

```csharp
// Capture device enumeration:
var devices = new VideoForWindowsDevices();
var descriptions = devices.Descriptions.ToArray();

Console.WriteLine($"{descriptions[0].Name}, {descriptions[0].Description}");
```

```csharp
// Open a device:
using var device = devices.Open(descriptions[0]);

// Reserved pixel buffer:
var buffer = new PixelBuffer();

// Hook frame arrived event:
device.FrameArrived += (s, e) =>
{
    // Capture a frame into pixel buffer:
    device.Capture(e, buffer);

    // Get image container:
    byte[] image = buffer.ExtractImage();

    // Anything use it (Maybe contains RGB DIB format data)
    var ms = new MemoryStream(image);
    var bitmap = Bitmap.FromStream(ms);

    // ...
};

// Start processing:
device.Start();

// ...
Console.ReadLine();

// Stop processing:
device.Stop();
```

---

## About pixel buffer

Pixel buffer (`PixelBuffer` class) is controlled about
image data allocation and buffering.
You can efficiently handle one frame of image data
by using different instances of `PixelBuffer`.

For example, frames that come in one after another can be captured
(`ICaptureDevice.Capture` method) and queued in separate pixel buffers,
and the retrieval operation (`PixelBuffer.ExtractImage` method)
can be performed in a separate thread.

This method minimizes the cost of frame arrival events and avoids frame dropping.

There is one more important feature that is relevant.
When calling `ExtractImage`, it automatically converts from unique image format
used by the imaging device to `RGB DIB` format.

For example, many image capture devices return frame data in
formats such as `YUY2` or `UYVY`, but these formats are not common.

The conversion code is multi-threaded for fast conversion.
However, in order to offload as much as possible,
the conversion is performed when the `PixelBuffer.ExtractImage` method is called.

Therefore, the following method is recommended:

1. `device.Capture(e, buffer)` is handled in the `FrameArrived` event.
2. When the image is actually needed, use `buffer.ExtractImage` to extract the image data.
This operation can be performed in a separate thread.

---

## License

Apache-v2.

