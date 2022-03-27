# FlashCap

![FlashCap](Images/FlashCap.100.png)

FlashCap - Independent camera capture library.

[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip)

## NuGet

| Package  | NuGet                                                                                                                |
|:---------|:---------------------------------------------------------------------------------------------------------------------|
| FlashCap | [![NuGet FlashCap](https://img.shields.io/nuget/v/FlashCap.svg?style=flat)](https://www.nuget.org/packages/FlashCap) |

## CI

| main                                                                                                                                                                 | develop                                                                                                                                                                       |
|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------|:------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [![FlashCap CI build (main)](https://github.com/kekyo/FlashCap/workflows/.NET/badge.svg?branch=main)](https://github.com/kekyo/FlashCap/actions?query=branch%3Amain) | [![FlashCap CI build (develop)](https://github.com/kekyo/FlashCap/workflows/.NET/badge.svg?branch=develop)](https://github.com/kekyo/FlashCap/actions?query=branch%3Adevelop) |

---

[![Japanese language](Images/Japanese.256.png)](https://github.com/kekyo/FlashCap/blob/main/README_ja.md)

## What is this?

Do you need to get camera capturing ability on .NET?
Is you tired for camera capturing library solutions on .NET?

This is a camera image capture library by specializing only capturing image data.
It has simple API, easy to use, simple architecture and without native libraries.
It also does not depend on any non-official libraries.
[See NuGet dependencies page.](https://www.nuget.org/packages/FlashCap)

.NET platforms supported are as follows (almost all!):

* .NET 6, 5 (`net6.0`, `net5.0`)
* .NET Core 3.1, 3.0, 2.2, 2.1, 2.0 (`netcoreapp3.1` and etc)
* .NET Standard 2.1, 2.0, 1.3 (`netstandard2.1` and etc)
* .NET Framework 4.8, 4.6.1, 4.5, 4.0, 3.5, 2.0 (`net48` and etc)

Platforms on which camera devices can be used:

* Windows (DirectShow devices)
* Windows (Video for Windows devices)
* Linux (V4L2 devices)

### Tested devices

Run the sample code to verify.

Verified capture devices:

* Elgato CamLink 4K (Windows/Linux)
* Logitech WebCam C930e (Windows/Linux)
* Unnamed cheap USB capture module (Windows/Linux)

Verified computers:

* Generic PC Core i9-9960X (x64, Windows)
* Generic PC Core i9-11900K (x64, Linux)
* Microsoft Surface Go Gen1 inside camera (x64, Windows)
* Sony VAIO Z VJZ131A11N inside camera (x64, Windows)
* clockworks DevTerm A06 (arm64, Linux)
* Raspberry Pi 400 (armhf/arm64, Linux)
* Seeed reTerminal (armhf, Linux)
* Teclast X89 E7ED Tablet PC inside camera (x86, Windows)
* NVIDIA Jetson TX2 evaluation board (arm64, Linux)

Couldn't detect any devices on FlashCap:

* Surface2 (Windows RT 8.1 JB'd)

---

## How to use

Enumerate target devices and video characteristics:

```csharp
using FlashCap;

// Capture device enumeration:
var devices = new CaptureDevices();

foreach (var descriptor in devices.EnumerateDescriptors())
{
    // "Logicool Webcam C930e: DirectShow device, Characteristics=34"
    // "Default: VideoForWindows default, Characteristics=1"
    Console.WriteLine(descriptor);

    foreach (var characteristics in descriptor.Characteristics)
    {
        // "1920x1080 [JPEG, 30fps]"
        // "640x480 [YUYV, 60fps]"
        Console.WriteLine(characteristics);
    }
}
```

Then, capture it:

```csharp
// Open a device with a video characteristics:
var descriptor0 = devices.EnumerateDescriptors().ElementAt(0);

using var device = await descriptor0.OpenAsync(
    descriptor0.Characteristics[0],
    async buffer =>
    {
        // Captured into a pixel buffer from an argument.

        // Get image data (Maybe DIB/Jpeg/PNG):
        byte[] image = buffer.ExtractImage();

        // Anything use of it...
        var ms = new MemoryStream(image);
        var bitmap = Bitmap.FromStream(ms);

        // ...
    });

// Start processing:
device.Start();

// ...

// Stop processing:
device.Stop();
```

Fully sample code is here:

* [Windows Forms application](samples/FlashCap.WindowsForms/)
* [Avalonia](samples/FlashCap.Avalonia/)

TODO:

* [WPF application](samples/FlashCap.WPF/)

This is an Avalonia sample application on both Windows and Linux.
It is performed realtime usermode capturing,
decoding bitmap (from MJPEG) and render to window.
Avalonia is using renderer with Skia. It is pretty fast.

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Windows.png)

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Linux.png)

---

## About handler and strategies

TODO: rewrite to what is handler strategies.

---

## Reduce data copy

TODO: rewrite

Another topic, `PixelBuffer.ReferImage()` method will return `ArraySegment<byte>`.
We can use it avoid copying image data (when transcode is not applicable).

**Caution**: The resulting array segment is valid until the next `Capture()` is executed.

```csharp
// Perform decode:
ArraySegment<byte> image = buffer.ReferImage();
var bitmap = SkiaSharp.SKBitmap.Decode(image);

// AFTER decoded, the pixel buffer isn't needed.
reserver.Push(buffer);

// (Anything use of it...)
```

There is a table for overall image extractions:

| Method           | Speed            | Thread safe       | Image data type      |
|:-----------------|:-----------------|:------------------|:---------------------|
| `CopyImage()`    | Slow             | Safe              | `byte[]`             |
| `ExtractImage()` | Sometimes slower | Protection needed | `byte[]`             |
| `ReferImage()`   | Fastest          | Protection needed | `ArraySegment<byte>` |

And disable transcoding when become "YUV" format, it performs referring image data absolutely raw.
(Of course, it is your responsibility to decode the raw data...)

```csharp
// Open device with disable transcoder:
using var device = await descriptor0.OpenAsync(
    descriptor0.Characteristics[0],
    false,    // transcodeIfYUV == false
    async bufer =>
    {
        // ...
    });

// ...
```

---

## Master for frame processor (Advanced topic)

TODO: rewrite to what is frame processor.

---

## Limitation

* In Video for Windows, "Source device" is not selectable by programmable. VFW logical structure is:
  1. VFW device driver (always only 1 driver, defaulted WDM device on latest Windows): `ICaptureDevices.EnumerateDevices()` iterate it.
  2. Source devices (truly real camera devices) each drivers. But we could not select programmable.
     Will show up selection dialog automatically when multiple camera devices are found.

---

## License

Apache-v2.

---

## History

* 0.10.0:
  * Implemented frame processor and exposed easier to use and be expandable frame/pixel grabbing strategies.
  * Removed event based interface and added callback interface.
  * Added supporting async methods on net35/net40 platform. (Need depending official async packages.)
  * Added supporting ValueTask async methods on net461 or upper platform. (Need depending official async packages.)
  * Completely separated between sync and async methods.
  * Removed any interface types.
  * Fixed causing randomly exception on mono-linux environment.
  * (We are almost to 1.0.0)
* 0.9.0:
  * Supported Linux V4L2 devices 🎉
* 0.8.0:
  * Improved frame rate calculation with fraction type.
  * Added easier taking an image method `CaptureOneShot`.
  * Video characteristics interface has changed. (Frame rate and pixel format related, from V4L2 impls.)
* 0.7.0:
  * Improved dodging video device with sending invalid video frame on DirectShow.
  * Fixed causing entry point is not found for RtlCopyMemory on 32bit environment.
  * Added LimitedExecutor class.
* 0.6.0:
  * Supported DirectShow devices 🎉
  * Improved and made stability for VFW, using separated STA thread.
* 0.5.0:
  * Reenabled netstandard1.3.
  * Added more useful methods and interfaces.
  * Improved dodging video device with sending invalid video frame on VFW.
  * Fixed broken for media type structure accessing.
* 0.4.0:
  * Implemented using DirectShow API (But still working progress, this version is disabled. Wait next release.)
  * Improved buffer handling.
  * Eliminated allocation at frame arrived event.
  * Fixed invalid transcoded data when arrived image is jpeg/mjpeg.
  * Fixed didn't invoke frame arrived event when VFW hosted window is truly outside of desktop area.
* 0.3.0:
  * Implemented central device enumeration.
  * Add supporting device characteristics enumeration.
  * Brush up some interface members for DirectShow and V4L2.
* 0.2.0:
  * Applied YUV conversion formula with MS's technical article.
  * Made PixelBuffer thread safe.
* 0.1.10:
  * Increased scatter height on parallel conversion.
  * Fixed conversion overrun when height mod isn't just.
* 0.1.0:
  * Initial release.

