////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.FrameProcessors;
using FlashCap.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap;

public enum DeviceTypes
{
    VideoForWindows,
    DirectShow,
    V4L2,
    AVFoundation,
}

public enum TranscodeFormats
{
    Auto,
    DoNotTranscode,
    BT601,
    BT601FullRange,
    BT709,
    BT709FullRange,
    BT2020,
    BT2020FullRange,
}

public delegate void PixelBufferArrivedDelegate(
    PixelBufferScope bufferScope);

public delegate Task PixelBufferArrivedTaskDelegate(
    PixelBufferScope bufferScope);

public abstract class CaptureDeviceDescriptor
{
    private readonly AsyncLock locker = new();

    internal readonly BufferPool defaultBufferPool;

    protected CaptureDeviceDescriptor(
        string name, string description,
        VideoCharacteristics[] characteristics,
        BufferPool defaultBufferPool)
    {
        this.Name = name;
        this.Description = description;
        this.Characteristics = characteristics;
        this.defaultBufferPool = defaultBufferPool;
    }

    public abstract object Identity { get; }
    public abstract DeviceTypes DeviceType { get; }
    public string Name { get; }
    public string Description { get; }
    public VideoCharacteristics[] Characteristics { get; }

    protected abstract Task<CaptureDevice> OnOpenWithFrameProcessorAsync(
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct);

    public override string ToString() =>
        $"{this.Name}: {this.Description}, Characteristics={this.Characteristics.Length}";


    //////////////////////////////////////////////////////////////////////////

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Task<CaptureDevice> InternalOpenWithFrameProcessorAsync(
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct) =>
        this.OnOpenWithFrameProcessorAsync(characteristics, transcodeFormat, frameProcessor, ct);

    internal async Task<CaptureDevice> InternalOnOpenWithFrameProcessorAsync(
        CaptureDevice preConstructedDevice,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct)
    {
        if (characteristics.PixelFormat == PixelFormats.Unknown)
        {
            throw new ArgumentException(
                $"FlashCap: Couldn't use unknown pixel format: {characteristics} ({characteristics.RawPixelFormat})");
        }

        using var _ = await this.locker.LockAsync(ct);

        try
        {
            await preConstructedDevice.InternalInitializeAsync(
                characteristics, transcodeFormat, frameProcessor, ct);
        }
        catch
        {
            preConstructedDevice.Dispose();
            throw;
        }
        return preConstructedDevice;
    }

    internal async Task<byte[]> InternalTakeOneShotAsync(
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<byte[]>();

        using var device = await this.OnOpenWithFrameProcessorAsync(
            characteristics, transcodeFormat,
            new DelegatedQueuingProcessor(pixelBuffer =>
            {
                var image = pixelBuffer.Buffer.InternalExtractImage(
                    PixelBuffer.BufferStrategies.ForceCopy);
                Debug.Assert(image.Array!.Length == image.Count);

                pixelBuffer.InternalReleaseNow();

                tcs.TrySetResult(image.Array);
            }, 1, new DefaultBufferPool()),
            ct);

        await device.InternalStartAsync(ct);
        var image = await tcs.Task;
        await device.InternalStopAsync(ct);

        return image;
    }
}
