# FlashCap

![FlashCap](Images/FlashCap.100.png)

FlashCap - Independent camera capture library.

[![Project Status: Active – The project has reached a stable, usable state and is being actively developed.](https://www.repostatus.org/badges/latest/active.svg)](https://www.repostatus.org/#active)

## NuGet

| Package  | NuGet                                                                                                                |
|:---------|:---------------------------------------------------------------------------------------------------------------------|
| FlashCap | [![NuGet FlashCap](https://img.shields.io/nuget/v/FlashCap.svg?style=flat)](https://www.nuget.org/packages/FlashCap) |
| FSharp.FlashCap | [![NuGet FSharp.FlashCap](https://img.shields.io/nuget/v/FSharp.FlashCap.svg?style=flat)](https://www.nuget.org/packages/FSharp.FlashCap) |


---

[![Japanese language](Images/Japanese.256.png)](https://github.com/kekyo/FlashCap/blob/main/README_ja.md)

## What is this?

Do you need to get camera capturing ability on .NET?
Is you tired for camera capturing library solutions on .NET?

This is a camera image capture library by specializing only capturing image data (a.k.a frame grabber).
It has simple API, easy to use, simple architecture and without native libraries.
It also does not depend on any non-official libraries.
[See NuGet dependencies page.](https://www.nuget.org/packages/FlashCap)


----

## Short sample code

Install the `FlashCap` NuGet package.

* The `FSharp.FlashCap` package allows you to use an API set optimized for F#.

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

        // Get image data (Maybe DIB/JPEG/PNG):
        byte[] image = bufferScope.Buffer.ExtractImage();

        // Anything use of it...
        var ms = new MemoryStream(image);
        var bitmap = Bitmap.FromStream(ms);

        // ...
    });

// Start processing:
await device.StartAsync();

// ...

// Stop processing:
await device.StopAsync();
```

You can also use the Reactive Extension:

```csharp
// Get a observable with a video characteristics:
var descriptor0 = devices.EnumerateDescriptors().ElementAt(0);

using var deviceObservable = await descriptor0.AsObservableAsync(
    descriptor0.Characteristics[0]);

// Subscribe the device.
deviceObservable.Subscribe(bufferScope =>
{
    // Captured into a pixel buffer from an argument.

    // Get image data (Maybe DIB/JPEG/PNG):
    byte[] image = bufferScope.Buffer.ExtractImage();

    // Anything use of it...
    var ms = new MemoryStream(image);
    var bitmap = Bitmap.FromStream(ms);

    // ...
});

// Start processing:
await deviceObservable.StartAsync();
```

As you can see, FlashCap does not depend on any GUI elements.
For example, FlashCap can be applied to a console application.

Published introduction article: ["Easy to implement video image capture with FlashCap" (dev.to)](https://dev.to/kozy_kekyo/easy-to-implement-video-image-capture-with-flashcap-o5a)


----

## Target environments

.NET platforms supported are as follows (almost all!):

* .NET 7, 6, 5 (`net7.0` and etc)
* .NET Core 3.1, 3.0, 2.2, 2.1, 2.0 (`netcoreapp3.1` and etc)
* .NET Standard 2.1, 2.0, 1.3 (`netstandard2.1` and etc)
* .NET Framework 4.8, 4.6.1, 4.5, 4.0, 3.5 (`net48` and etc)

Platforms on which camera devices can be used:

* Windows (DirectShow devices, tested on x64/x86)
* Windows (Video for Windows devices, tested on x64/x86)
* Linux (V4L2 devices, supported on x86_64/i686/aarch64/armv7l/mips)

## Tested devices

Run the sample code to verify in 0.11.0.

Verified capture devices / cameras:

* Elgato CamLink 4K (Windows/Linux)
* BlackMagic Design ATEM Mini Pro (Windows/Linux)
* Logitech WebCam C930e (Windows/Linux)
* eMeet HD Webcam C970L (Windows/Linux)
* Microsoft LifeCam Cinema HD720 (Windows/Linux)
* Unnamed cheap USB capture module (Windows/Linux)

Verified computers:

* Generic PC Core i9-9960X (x64, Windows)
* Generic PC Core i9-11900K (x64, Linux)
* Microsoft Surface Go Gen1 inside camera (x64, Windows)
* Sony VAIO Z VJZ131A11N inside camera (x64, Windows)
* clockworks DevTerm A06 (aarch64, Linux)
* Raspberry Pi 400 (armv7l/aarch64, Linux)
* Seeed reTerminal (armv7l, Linux, mono is unstable)
* Teclast X89 E7ED Tablet PC inside camera (x86, Windows)
* NVIDIA Jetson TX2 evaluation board (aarch64, Linux)
* Acer Aspire One ZA3 inside camera (i686, Linux)
* Imagination Creator Ci20 (mipsel, Linux)
* Radxa ROCK5B (aarch64, Linux)

Couldn't detect any devices on FlashCap:

* Surface2 (arm32, Windows RT 8.1 JB'd)
  * Any devices are not found, may not be compatible with both VFW and DirectShow.


----

## Fully sample code

Fully sample code is here:

* [Avalonia11 application](samples/FlashCap.Avalonia/)
* [WPF application](samples/FlashCap.Wpf/)
* [Windows Forms application](samples/FlashCap.WindowsForms/)
* [Console application](samples/FlashCap.OneShot/)

This is an Avalonia sample application on both Windows and Linux.
It is performed realtime usermode capturing, decoding bitmap (from MJPEG) and render to window.
Avalonia is using renderer with Skia [(SkiaImageView)](https://github.com/kekyo/SkiaImageView).
It is pretty fast.

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Windows.png)

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Linux.png)

### Want to take just one image

If you want to take only one image, there is a very simple method:

```csharp
// Take only one image, given the image characteristics:
var descriptor0 = devices.EnumerateDescriptors().ElementAt(0);

byte[] imageData = await descriptor0.TakeOneShotAsync(
    descriptor0.Characteristics[0]);

// Save to file
await File.WriteAllBytesAsync("oneshot", imageData);
```

See [sample code](samples/FlashCap.OneShot/) for a complete implementation.

### Exclusion of unsupported formats

The video characteristics contain a list of formats supported by the camera.
FlashCap does not support all formats, so you must select the correct format before opening the device.
Unsupported formats are indicated by `PixelFormats.Unknown`.

```csharp
// Select a device:
var descriptor0 = devices.EnumerateDescriptors().ElementAt(0);

// Exclude unsupported formats:
var characteristics = descriptor0.Characteristics.
    Where(c => c.PixelFormat ! = PixelFormats.Unknown).
    ToArray();
```

FlashCap enumerates all formats returned by the device.
Therefore, by checking the information in `VideoCharacteristics` with `PixelFormats.Unknown`, you can analyze what formats the device supports.

### Displaying camera device property page

It is possible to display camera device property page.

![PropertyPage](Images/PropertyPage.png)

```csharp
using var device = await descriptor.OpenAsync(
    characteristics,
    async bufferScope =>
    {
        // ...
    });

// if the camera device supports property pages
if (device.HasPropertyPage)
{
    // Get parent window handle from Avalonia window
    if (this.window.TryGetPlatformHandle()?.Handle is { } handle)
    {
        // show the camera device's property page
        await device.ShowPropertyPageAsync(handle);
    }
}
```

Currently, property pages can only be displayed when the target is a DirectShow device.

See [Avalonia sample code](samples/FlashCap.Avalonia/) and [WPF sample code](samples/FlashCap.Wpf/) for a complete implementation.


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

| Method | Speed | Out of scope | Image type |
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
        PixelFormats.JPEG => "output.jpg",
        PixelFormats.PNG => "output.png",
        _ => "output.bmp",
      });
    await fs.WriteAsync(image.Array, image.Offset, image.Count);
    await fs.FlushAsync();
  });
```

## About transcoder

The "raw image data" obtained from a device may not be a JPEG or RGB DIB bitmap, which we can easily handle.
Typically, video format is called "MJPEG" (Motion JPEG) or "YUV" if it is not a continuous stream such as MPEG.

"MJPEG" is completely the same as JPEG, so FlashCap returns the image data as is.
In contrast, the "YUV" format has the same data header format as a DIB bitmap, but the contents are completely different.
Therefore, many image decoders will not be able to process it if it is saved as is in a file such as "output.bmp".

Therefore, FlashCap automatically converts "YUV" format image data into RGB DIB format.
This process is called "transcoding."
Earlier, I explained that `ReferImage()` "basically no copying occurs here," but in the case of "YUV" format, transcoding occurs, so a kind of copying is performed.
(FlashCap handles transcoding in multi-threaded, but even so, large image data can affect performance.)

If the image data is "YUV" and you do not have any problem, you can disable transcoding so that the copying process is completely one time only:

```csharp
// Open device with transcoding disabled:
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  TranscodeFormats.DoNotTranscode,   // Do not transcode.
  async buferScope =>
  {
      // ...
  });

// ...
```

The `TranscodeFormats` enumeration value has the following choices:

| `TranscodeFormats` | Details |
|:----|:----|
| `Auto` | Transcode if necessary and automatically select a transformation matrix. Depending on the resolution, `BT601`, `BT709`, or `BT2020` will be selected. |
| `DoNotTranscode` | No transcoding at all; formats other than JPEG or PNG will be stored in the DIB bitmap as raw data. |
| `BT601` | If necessary, transcode using the BT.601 conversion matrix. This is standard for resolutions up to HD. |
| `BT709` | If necessary, transcode using the BT.709 conversion matrix. This is standard for resolutions up to FullHD. |
| `BT2020` | If necessary, transcode using the BT.2020 conversion matrix. This is standard for resolutions beyond FullHD, such as 4K. |

In addition to the above, there are `BT601FullRange`, `BT709FullRange`, and `BT2020FullRange`.
These extend the assumed range of the luminance signal to the entire 8-bit range, but are less common.
If `Auto` is selected, these `FullRange` matrices are not used.

## Callback handler and invoke trigger

The callback handlers described so far assume that the trigger to be called is "when a frame is obtained,"
but this trigger can be selected from several patterns.
This choice can be made with the `isScattering` and `maxQueuingFrames` arguments,
or with the overloaded argument of `OpenAsync`:

```csharp
// Specifies the trigger for invoking the handler:
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  true,
  true,   // Specifying the invoking trigger (true: Scattering)
  10,     // Maximum number of queuing frames
  async buferScope =>
  {
      // ...
  });

// ...
```

The following is a list of pattern types:

| `isScattering` | `maxQueuingFrames` | Summary |
|:----|:----|:----|
| false | 1 | When argument is omitted (default). Discard all subsequent frames unless the handler returns control. Suitable for general usage. |
| false | n | Subsequent frames are stored in the queue even if the handler does not return control. If computer performance is sufficient, up to the maximum number of frames will not be lost. |
| true | n | Handlers are processed in parallel by a multithreaded worker. Although the order of corresponding frames is not guaranteed, processing can be accelerated if the CPU supports multiple cores. |

The default call trigger is appropriate for many cases.
For example, if an image is previewed in the UI and an excessive value is specified for the number of frames to stay,
if the handler is slow, the queue will hold old image data and the current pose and preview will diverge rapidly.
Also, at some point the process will be forced to terminate due to lack of memory.

Similarly, `isScattering == true` is more difficult to master.
Your handler will be called and processed in a multi-threaded environment at the same time.
Therefore, at the very least, your handler should be implemented to be thread-safe.
Also, being called in a multi-threaded fashion means that the buffers to be processed may not necessarily maintain their order.
For example, when displaying a preview in the UI, the video should momentarily go back in time or feel choppy.

To deal with the fact that `isScattering == true` can cause the order of frames to be lost, the `PixelBuffer` class defines the `Timestamp` and `FrameIndex` properties. By referring to these properties, you can determine the frame order.

## Reactive extension issue

By the way, have you noticed that there are overloads for
both `PixelBufferArrivedDelegate` and `PixelBufferArrivedTaskDelegate` in the handler argument of `OpenAsync()`?
This is because they correspond to the synchronous and asynchronous versions of the handler implementation,
respectively, and both correctly recognize the completion of handler processing.

However, in the case of `AsObservableAsync()`,
the handler implementation corresponds to the Reactive Extension's `OnNext()` method,
which only exists in the synchronous version.
In other words, if you use the Reactive Extension,
you cannot use asynchronous processing for the observer implementation.
You can mark with `async void` for `async void OnNext(...)`,
but be very careful that the pixel buffer expires just before the first `await`.
The compiler cannot detect this problem.

The safest course of action would be to extract (copy) the image data from the pixel buffer as quickly as possible.
This is easily accomplished using the projection operator:

```csharp
deviceObservable.
    // Immediately projection
    Select(bufferScope =>
        Bitmap.FromStream(bufferScope.Buffer.ReferImage().AsStream())).
    // Do whatever you want after that...
    // ...
```

## Master for frame processor (Advanced topic)

Welcome to the underground dungeon, where FlashCap's frame processor is a polished gem.
But you don't need to understand frame processors unless you have a lot of experience with them.
This explanation should be used as a reference when dealing with unavoidable frame processors.
Also helpful would be [the default implementation of the frame processor](https://github.com/kekyo/FlashCap/tree/main/FlashCap/FrameProcessors) that FlashCap includes.

The callback handler invocation triggers described in the previous section are internally realized by switching frame processors.
In other words, it is an abstraction of how frames are handled and their behavior.

The frame processor is implemented by inheriting a very simple base class:

```csharp
// (Will spare you the detailed definitions.)
public abstract class FrameProcessor : IDisposable
{
  // Implement if necessary.
  public virtual void Dispose()
  {
  }

  // Get a pixel buffer.
  protected PixelBuffer GetPixelBuffer()
  { /* ... */ }

  // Return the pixel buffer.
  public void ReleasePixelBuffer(PixelBuffer buffer)
  { /* ... */ }

  // Perform capture using the device.
  protected void Capture(
    CaptureDevice captureDevice,
    IntPtr pData, int size,
    long timestampMicroseconds, long frameIndex,
    PixelBuffer buffer)
  { /* ... */ }

  // Called when a frame is arrived.
  public abstract void OnFrameArrived(
    CaptureDevice captureDevice,
    IntPtr pData, int size, long timestampMicroseconds, long frameIndex);
}
```

At the very least, you need to implement the `OnFrameArrived()` method.
This is literally called when a frame is arrived.
As you can see from the signature, it is passed a raw pointer, the size of the image data, a timestamp, and a frame number.

Note also that the return value is `void`.
This method cannot be asynchronous.
Even if you qualify it with `async void`, the information passed as arguments cannot be maintained.

Here is a typical implementation of this method:

```csharp
public sealed class CoolFrameProcessor : FrameProcessor
{
  private readonly Action<PixelBuffer> action;

  // Hold a delegate to run once captured.
  public CoolFrameProcessor(Action<PixelBuffer> action) =>
    this.action = action;

  // Called when a frame is arrived.
  public override void OnFrameArrived(
    CaptureDevice captureDevice,
    IntPtr pData, int size, long timestampMicroseconds, long frameIndex)
  {
    // Get a pixel buffer.
    var buffer = base.GetPixelBuffer();

    // Perform capture.
    // Image data is stored in pixel buffer. (First copy occurs.)
    base.Capture(
      captureDevice,
      pData, size,
      timestampMicroseconds, frameIndex,
      buffer);

    // Invoke a delegate.
    this.action(buffer);

    // Return the pixel buffer (optional, will reuse allocated buffer)
    base.ReleasePixelBuffer(buffer);
  }
}
```

Recall that this method is called each time a frame is arrived.
In other words, this example implementation creates a pixel buffer, captures it, and invoke the delegate every time a frame is arrived.

Let's try to use it:

```csharp
var devices = new CaptureDevices();
var descriptor0 = devices.EnumerateDevices().ElementAt(0);

// Open by specifying our frame processor.
using var device = await descriptor0.OpenWitFrameProcessorAsync(
  descriptor0.Characteristics[0],
  TranscodeFormats.Auto,
  new CoolFrameProcessor(buffer =>   // Using our frame processor.
  {
    // Captured pixel buffer is passed.
    var image = buffer.ReferImage();

    // Perform decode.
    var bitmap = Bitmap.FromStream(image.AsStream());

    // ...
  });

await device.StartAsync();

// ...
```

Your first frame processor is ready to go.
And even if you don't actually run it, you're probably aware of its features and problems:

* The delegate is invoked in the shortest possible time when the frame arrives. (It is the fastest to the point where it is invoked.)
* `OnFrameArrived()` blocks until the delegate completes processing.
* The delegate assumes synchronous processing. Therefore, the decoding process takes time, and blocking this thread can easily cause frame dropping.
* If you use `async void` here to avoid blocking, access to the pixel buffer is at risk because it cannot wait for the delegate to complete.

For this reason, FlashCap uses a standard set of frame processors that can be operated with some degree of safety.
So where is the advantage of implementing custom frame processors?

It is possible to implement highly optimized frame and image data processing.
For example, pixel buffers are created efficiently, but we do not have to be used.
(Calling the `Capture()` method is optional.)
Since a pointer to the raw image data and its size are given by the arguments, it is possible to access the image data directly.
So, you can implement your own image data processing to achieve the fastest possible processing.


----

## Limitation

* In Video for Windows, "Source device" is not selectable by programmable. VFW logical structure is:
  1. VFW device driver (always only 1 driver, defaulted WDM device on latest Windows): `ICaptureDevices.EnumerateDevices()` iterate it.
  2. Source devices (truly real camera devices) each drivers. But we could not select programmable.
     Will show up selection dialog automatically when multiple camera devices are found.


----

## Build FlashCap

FlashCap keeps a clean build environment.
Basically, if you have Visual Studio 2022 .NET development environment installed, you can build it as is.
(Please add the WPF and Windows Forms options. These are required to build the sample code)

1. Clone this repository.
2. Build `FlashCap.sln`.
   * Build it with `dotnet build`.
   * Or open `FlashCap.sln` with Visual Studio 2022 and build it.

NOTE: FlashCap itself should build in a Linux environment,
but since the sample code has a Windows-dependent implementation,
we assume Windows as the development environment.

Pull requests are welcome! Development is on the `develop` branch and merged into the `main` branch at release time.
Therefore, if you make a pull request, please make new your topic branch from the `develop` branch.


----

## License

Apache-v2.


----

## History

* 1.8.0:
  * Fixed some incorrect conversion matrix coefficients for transcoding [#107](https://github.com/kekyo/FlashCap/issues/107)
* 1.7.0:
  * Supported display property page on DirectShow device. [#112](https://github.com/kekyo/FlashCap/issues/112)
  * Added transcoding formats by `TranscodeFormats` enum type, declared BT.601, BT.709 and BT.2020. [#107](https://github.com/kekyo/FlashCap/issues/107)
  * Supported BlackMagic specific YUYV format. [#105](https://github.com/kekyo/FlashCap/issues/105)
  * Some methods/functions are marked as `Obsolete` . Change them according to the warnings.
  * Supported .NET 8.0 RC2.
* 1.6.0:
  * Fixed problem with some formats not being enumerated in V4L2.
  * Unsupported formats are now visible as `PixelFormats.Unknown` instead of being implicitly excluded.
  * Downgraded dependent F# packages to 5.0.0.
* 1.5.0:
  * Added `TakeOneShotAsync()` method to easily take a single image, and added corresponding sample project.
  * Avalonia sample code now displays FPS and taken image information in real time.
* 1.4.0:
  * Allow `CancellationToken` to be specified in asynchronous methods.
  * `Start` and `Stop` now support asynchronous processing.
  * Fixed where an implicit asynchronous operation is required (Async-Over-Sync).
  * Fixed a bug in V4L2 that caused an `ArgumentException` when reopening a capture device [#9](https://github.com/kekyo/FlashCap/issues/9).
  * Avalonia sample code allows switching between devices and characteristics on UI elements.
* 1.3.0:
  * Added `FSharp.FlashCap` package that exposes API set for F#.
* 1.2.0:
  * Supported Reactive Extension on `AsObservableAsync()`.
* 1.1.0:
  * Moved implementation of pixel buffer pooling into base FrameProcessor class.
  * Fixed IDisposable is not implemented on CaptureDevice.
* 1.0.0:
  * Reached 1.0.0 🎉
  * Supported miplel on V4L2.
  * All planned environments were tested.
  * Added WPF sample project.
  * Implemented graceful shutdown in sample projects.
* 0.15.0:
  * Completed rewriting V4L2 interop code, and fixed V4L2 on i686.
  * Remove supporting net20, because made completely asynchronous operation.
  * `OpenAsync` has been changed to allow `maxQueuingFrames` to be specified.
* 0.14.0:
  * Avoid deadlocking when arrived event handlers stuck in disposing process on internal frame processors.
* 0.12.0:
  * Added overloading to Open method to specify transcoding flags.
  * Implemented graceful shutdown for internal frame processors.
  * Applied processor count on transcoding.
  * Reduced timestamp calculation cost, achieving for lesser resource environments.
  * Added frame index property.
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

