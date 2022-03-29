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


----

## Short sample code

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
    async bufferScope =>
    {
        // Captured into a pixel buffer from an argument.

        // Get image data (Maybe DIB/Jpeg/PNG):
        byte[] image = bufferScope.Buffer.ExtractImage();

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

----

## Target environments

.NET platforms supported are as follows (almost all!):

* .NET 6, 5 (`net6.0`, `net5.0`)
* .NET Core 3.1, 3.0, 2.2, 2.1, 2.0 (`netcoreapp3.1` and etc)
* .NET Standard 2.1, 2.0, 1.3 (`netstandard2.1` and etc)
* .NET Framework 4.8, 4.6.1, 4.5, 4.0, 3.5, 2.0 (`net48` and etc)

Platforms on which camera devices can be used:

* Windows (DirectShow devices)
* Windows (Video for Windows devices)
* Linux (V4L2 devices)

## Tested devices

Run the sample code to verify in 0.11.0.

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
  * Any devices are not found, may not be compatible with both VFW and DirectShow.

----

## Fully sample code

Fully sample code is here:

* [Windows Forms application](samples/FlashCap.WindowsForms/)
* [Avalonia](samples/FlashCap.Avalonia/)

TODO:

* [WPF application](samples/FlashCap.WPF/)

This is an Avalonia sample application on both Windows and Linux.
It is performed realtime usermode capturing, decoding bitmap (from MJPEG) and render to window.
Avalonia is using renderer with Skia.
It is pretty fast.

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Windows.png)

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Linux.png)

----

## Implementation Guidelines

In the following sections, we will explain various techniques for processing large amounts of image data using FlashCap.
This is an application example, so it is not necessary to read it, but it will give you some hints for implementation.

## Reduce data copy

Processing video requires handling large amounts of data; in FlashCap, each piece of video is called "a frame."
The frames come and go at a rate of 60 or 30 times per second.

The key here is how to process the data in each frame without copying it.
Currently, FlashCap requires at least one copy.
However, depending on how it is used, two, three, or even more copies may occur.

The callback when calling the `OpenAsync` method will pass a `PixelBufferScope` argument.
This argument contains the data of the frame that was copied once.
Now let's call the `CopyImage()` method, which is the "safest" method:

```csharp
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  async bufferScope =>  // <-- `PixelBufferScope` (already copied once at this point)
  {
    // This is where the second copy occurs.
    byte[] image = bufferScope.Buffer.CopyImage();

    // Convert to Stream.
    var ms = new MemoryStream(image);
    // Consequently, a third copy occurs here.
    var bitmap = Bitmap.FromStream(ms);

    // ...
  });
```

This would result in at least two copies in total.
Furthermore, by decoding the resulting image data (`image`) with `Bitmap.FromStream()`, three copies will have occurred as a result.

Now, what about the first code example, using `ExtractImage()`?

```csharp
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  async bufferScope =>  // <-- `PixelBufferScope` (already copied once at this point)
  {
    // This is where the second copy (may) occur.
    byte[] image = bufferScope.Buffer.ExtractImage();

    // Convert to Stream.
    var ms = new MemoryStream(image);
    // Decode. Consequently, a second or third copy occurs here.
    var bitmap = Bitmap.FromStream(ms);

    // ...
  });
```

When I say "copying (may) occur," I mean that under some circumstances, copying may not occur.
If so, you may think that you should use only `ExtractImage()` instead of `CopyImage()`. 
However, `ExtractImage()` has a problem that the validity period of obtained data is short.

Consider the following code:

```csharp
// Stores image data outside the scope of the callback.
byte[]? image = null;

using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  bufferScope =>  // <-- `PixelBufferScope` (already copied once at this point)
  {
    // Save out of scope. (second copy)
    image = bufferScope.Buffer.CopyImage();
    //image = bufferScope.ExtractImage();  // DANGER!!!
  });

// Use outside of scope.
var ms = new MemoryStream(image);
// Decode (Third copy)
var bitmap = Bitmap.FromStream(ms);
```

Thus, if the image is copied with `CopyImage()`, it can be safely referenced outside the scope of the callback.
However, if you use `ExtractImage()`, you must be careful because the image data may be corrupted if you reference it outside the scope.

Similarly, using the `ReferImage()` method, basically no copying occurs. (Except when transcoding occurs. See below.)
Again, out-of-scope references cannot be made. Also, the image data is not stored in a byte array, but in a `ArraySegment<byte>` is used to refer to the image data.

This type cannot be used as is because it represents a partial reference to an array.
For example, if you want to use it as a `Stream`, use the following:

```csharp
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  async bufferScope =>  // <-- `PixelBufferScope` (already copied once at this point)
  {
    // Basically no copying occurs here.
    ArraySegment<byte> image =
      bufferScope.Buffer.ReferImage();

    // Convert to Stream.
    var ms = new MemoryStream(
      image.Array, image.Offset, image.Count);
    // Decode (Second copy)
    var bitmap = Bitmap.LoadFrom(ms);

    // ...
  });
```

If you use `MemoryStream`, you may use the extension method `AsStream()`, which is defined similar to this code example.
Also, if you use SkiaSharp, you can pass `ArraySegment<byte>` directly using `SKBitmap.Decode()`.

The following is a list of methods for acquiring image data described up to this point:

| method | speed | out of scope | image type |
|:-----------------|:----------|:--------|:---------------------|
| `CopyImage()` | Slow | Safe | `byte[]` |
| `ExtractImage()` | Slow in some cases | Danger | `byte[]` |
| `ReferImage()` | Fast | Danger | `ArraySegment<byte>` |

I found that using `ReferImage()`, I can achieve this with at least two copies.
So how can we shorten it to once?

To achieve this with only one copy, the decoding of the image data must be given up.
Perhaps, if the environment allows hardware to process the image data, the second copy could be offloaded by passing the image data directly to the hardware.

As an easy-to-understand example, consider the following operation, which saves image data directly to a file. In this case, since no decoding is performed, it means that the copying is done once. (Instead, the I/O operation is tremendously slow...)

```csharp
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  async bufferScope =>  // <-- `PixelBufferScope` (already copied once at this point)
  {
    // Basically no copying occurs here.
    ArraySegment<byte> image = bufferScope.Buffer.ReferImage();

    // Output image data directly to a file.
    using var fs = File.Create(
      descriptor0.Characteristics[0].PixelFormat switch
      {
        PixelFormats.Jpeg => "output.jpg",
        _ => "output.bmp",
      });
    await fs.WriteAsync(image.Array, image.Offset, image.Count);
    await fs.FlushAsync();
  });
```

### About transcoder

The "raw image data" obtained from a device may not be a JPEG or DIB bitmap, which we can easily handle.
Typically, video format is called "MJPEG" (Motion JPEG) or "YUV" if it is not a continuous stream such as MPEG.

"MJPEG" is completely the same as JPEG, so FlashCap returns the image data as is.
In contrast, the "YUV" format has the same data header format as a DIB bitmap, but the contents are completely different.
Therefore, many image decoders will not be able to process it if it is saved as is in a file such as "output.bmp".

Therefore, FlashCap automatically converts "YUV" format image data into "RGB" DIB format.
This process is called "transcoding."
Earlier, I explained that `ReferImage()` "basically no copying occurs here," but in the case of "YUV" format, transcoding occurs, so a kind of copying is performed.
(FlashCap handles transcoding in multi-threaded, but even so, large image data can affect performance.)

If the image data is "YUV" and you do not have any problem, you can disable transcoding so that the copying process is completely one time only:

```csharp
// Open device with transcoding disabled:
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  false,   // transcodeIfYUV == false
  async buferScope =>
  {
      // ...
  });

// ...
```

----

## About callback handler and strategies

TODO: rewrite to what is handler strategies.

----

## Master for frame processor (Advanced topic)

TODO: rewrite to what is frame processor.

----

## Limitation

* In Video for Windows, "Source device" is not selectable by programmable. VFW logical structure is:
  1. VFW device driver (always only 1 driver, defaulted WDM device on latest Windows): `ICaptureDevices.EnumerateDevices()` iterate it.
  2. Source devices (truly real camera devices) each drivers. But we could not select programmable.
     Will show up selection dialog automatically when multiple camera devices are found.

----

## License

Apache-v2.

----

## History

* 0.11.0:
  * Added `PixelBufferScope` to allow early release of pixel buffers.
  * Add `IsDiscrete` so that it can determine whether the video characteristics are defined by the device or not.
  * Fixed some bugs by testing on various devices and computers.
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

