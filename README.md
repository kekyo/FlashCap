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

Do you need to get camera capturing ability on .NET?
Is you tired for camera capturing library solutions on .NET?

This is a camera image capture library by specializing only capturing image data.
It has simple API, easy to use, simple architecture and has no any other library dependencies, [see NuGet summary page.](https://www.nuget.org/packages/FlashCap)

.NET platforms supported are as follows (almost all!):

* .NET 6, 5 (`net6.0`, `net5.0`)
* .NET Core 3.1, 3.0, 2.2, 2.1, 2.0 (`netcoreapp3.1` and etc)
* .NET Standard 2.1, 2.0, 1.3 (`netstandard2.1` and etc)
* .NET Framework 4.8, 4.6.1, 4.5, 4.0, 3.5, 2.0 (`net48` and etc)

Platforms on which camera devices can be used:

* Windows (Video for Windows API)

WIP:

* Windows (DirectShow API)

TODO:

* Linux (V2L2 API)

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
        // "1920x1080 [MJPG, 24bits, 30fps]"
        // "640x480 [YUY2, 16bits, 60fps]"
        Console.WriteLine(characteristics);
    }
}
```

Then, capture it:

```csharp
// Open a device with a video characteristics:
var descriptor0 = devices.EnumerateDescriptors().ElementAt(0);

using var device = descriptor0.Open(
    descriptor0.Characteristics[0])

// Reserved pixel buffer:
var buffer = new PixelBuffer();

// Hook frame arrived event:
device.FrameArrived += (s, e) =>
{
    // Capture a frame into pixel buffer:
    device.Capture(e, buffer);

    // Get image data binary:
    byte[] image = buffer.ExtractImage();

    // Anything use of it:
    var ms = new MemoryStream(image);
    var bitmap = Bitmap.FromStream(ms);

    // ...
};

// Start processing:
device.Start();

// ...

// Stop processing:
device.Stop();
```

Full sample code and another library combination usage are here:

* [Windows Forms application](samples/FlashCap.WindowsForms/)

TODO:

* [WPF application](samples/FlashCap.WPF/)
* [Avalonia](samples/FlashCap.Avalonia/)

---

## Limitation

* In Video for Windows, "Source device" is not selectable by programmable. VFW logical structure is:
  1. VFW device driver (always only 1 driver, defaulted WDM device on latest Windows): ICaptureDevices.EnumerateDevices() iterate it.
  2. Source devices (truly real camera devices) each drivers. But we could not select programmable.
     Will show up selection dialog automatically when multiple camera devices are found.

---

## Master for pixel buffer (Advanced topic)

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
When calling `ExtractImage`, it automatically transcodes from unique image format
used by the imaging device to `RGB DIB` format.

For example, many image capture devices return frame data in
"YUV" formats such as `YUY2` or `UYVY`, but these formats are not common.

The transcoder is faster with multi-threaded.
However, in order to offload as much as possible,
the transcoder is performed when the `PixelBuffer.ExtractImage` method is called.

Therefore, the following method is recommended:

1. `device.Capture(e, buffer)` is (only) handled when the `FrameArrived` event.
2. When the image data is actually needed, use `buffer.ExtractImage` to extract the image data.
This operation can be offloaded in a separate thread.

### 1. Enable queuing

This is illustrated for it strategy:

* These sample code contain using [SkiaSharp](https://github.com/mono/SkiaSharp).
  Because it is faster and not needed to assume any thread context difficulty.

```csharp
using System.Collections.Concurrent;
using SkiaSharp;

// Pixel buffer queue:
var queue = new BlockingCollection<PixelBuffer>();

// Hook frame arrived event:
device.FrameArrived += (s, e) =>
{
    // Capture a frame into a pixel buffer.
    // We have to do capturing on only FrameArrived event context.
    var buffer = new PixelBuffer();
    device.Capture(e, buffer);

    // Enqueue pixel buffer.
    queue.Add(buffer);
};

// Decoding with offloaded thread:
Task.Run(() =>
{
    foreach (var buffer in queue.GetConsumingEnumerable())
    {
        // Get image data binary:
        byte[] image = buffer.ExtractImage();

        // Decode by SkiaSharp:
        var bitmap = SkiaSharp.SKBitmap.Decode(image);

        // (Anything use of it...)
    }
});
```

### 2. Reuse pixel buffers

We can reuse `PixelBuffer` instance when it is not needed.
These code completes reusing:

```csharp
// Pixel buffer queue and reserver:
var reserver = new ConcurrentStack<PixelBuffer>();
var queue = new BlockingCollection<PixelBuffer>();

// Hook frame arrived event:
device.FrameArrived += (s, e) =>
{
    // Try dispence a pixel buffer:
    if (!reserver.TryPop(out var buffer))
    {
        // If empty, create now:
        buffer = new PixelBuffer();
    }

    // Capture a frame into a dispensed pixel buffer.
    device.Capture(e, buffer);

    // Enqueue pixel buffer.
    queue.Add(buffer);
};

// Decoding with offloaded thread:
Task.Run(() =>
{
    foreach (var buffer in queue.GetConsumingEnumerable())
    {
        // Get image data binary with copy:
        byte[] image = buffer.CopyImage();  // Need to copy.

        // Now, the pixel buffer isn't needed.
        // So we can push it into reserver.
        reserver.Push(buffer);

        // Decode by SkiaSharp:
        var bitmap = SkiaSharp.SKBitmap.Decode(image);

        // (Anything use of it...)
    }
});
```

### 3. Decode with multiple worker threads

Furthermore, it is possible to consider offloading
with multiple worker threads each scattering pixel buffers:

```csharp
// Scattering each pixel buffers.
Parallel.ForEach(
    queue.GetConsumingEnumerable(),
    buffer =>
    {
        byte[] image = buffer.CopyImage();  // Need to copy.
        reserver.Push(buffer);
        var bitmap = SkiaSharp.SKBitmap.Decode(image);

        // (Anything use of it...)
    });
```

### 4. Reduce data copy

Another topic, `PixelBuffer.ReferImage()` method will return `ArraySegment<byte>`.
We can use it dodge copying image data (when transcode is not applicable).

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

|Method|Speed|Thread safe|Image data type|
|:---|:---|:---|
|`CopyImage()`|Slow|Safe|`byte[]`|
|`ExtractImage()`|Sometimes slower|Protection needed|`byte[]`|
|`ReferImage()`|Fastest|Protection needed|`ArraySegment<byte>`|

And disable transcoding when become "YUV" format, it performs referring image data absolutely raw.
(Of course, it is your responsibility to decode the raw data...)

```csharp
// Open device with disable transcoder:
using var device = descriptor0.Open(
    descriptor0.Characteristics[0],
    false);    // transcodeIfYUV == false

// ...
```

---

## License

Apache-v2.

## History

* 0.4.0:
  * Implemented using DirectShow API (But still working progress, this version is disabled. Wait next release.)
  * Improved buffer handling.
  * Eliminated allocation at frame arrived event.
  * Fixed invalid transcoded data when arrived image is jpeg/mjpeg.
  * Fixed didn't invoke frame arrived event when VFW hosted window is truly outside of desktop area.
* 0.3.0:
  * Implemented central device enumeration.
  * Add supporting device characteristics enumeration.
  * Brush up some interface members for DirectShow and V2L2.
* 0.2.0:
  * Applied YUV conversion formula with MS's technical article.
  * Made PixelBuffer thread safe.
* 0.1.10:
  * Increased scatter height on parallel conversion.
  * Fixed conversion overrun when height mod isn't just.
* 0.1.0:
  * Initial release.

