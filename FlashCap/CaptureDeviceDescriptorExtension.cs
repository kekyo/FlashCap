////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.FrameProcessors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap;

public static class CaptureDeviceDescriptorExtension
{
    [Obsolete("This overload is obsoleted, please use TranscodeFormats parameter instead.")]
    public static Task<CaptureDevice> OpenWithFrameProcessorAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        FrameProcessor frameProcessor,
        CancellationToken ct = default) =>
        descriptor.InternalOpenWithFrameProcessorAsync(
            characteristics,
            transcodeIfYUV ? TranscodeFormats.Auto : TranscodeFormats.DoNotTranscode,
            frameProcessor,
            ct);

    public static Task<CaptureDevice> OpenWithFrameProcessorAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct = default) =>
        descriptor.InternalOpenWithFrameProcessorAsync(
            characteristics, transcodeFormat, frameProcessor, ct);

    //////////////////////////////////////////////////////////////////////////////////

    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        PixelBufferArrivedDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, TranscodeFormats.Auto,
            new DelegatedQueuingProcessor(pixelBufferArrived, 1),
            ct);

    [Obsolete("This overload is obsoleted, please use TranscodeFormats parameter instead.")]
    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        PixelBufferArrivedDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeIfYUV,
            new DelegatedQueuingProcessor(pixelBufferArrived, 1),
            ct);

    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        PixelBufferArrivedDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeFormat,
            new DelegatedQueuingProcessor(pixelBufferArrived, 1),
            ct);

    [Obsolete("This overload is obsoleted, please use TranscodeFormats parameter instead.")]
    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        bool isScattering,
        int maxQueuingFrames,
        PixelBufferArrivedDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeIfYUV,
            isScattering ?
                new DelegatedScatteringProcessor(pixelBufferArrived, maxQueuingFrames) :
                new DelegatedQueuingProcessor(pixelBufferArrived, maxQueuingFrames),
            ct);

    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        bool isScattering,
        int maxQueuingFrames,
        PixelBufferArrivedDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeFormat,
            isScattering ?
                new DelegatedScatteringProcessor(pixelBufferArrived, maxQueuingFrames) :
                new DelegatedQueuingProcessor(pixelBufferArrived, maxQueuingFrames),
            ct);

    //////////////////////////////////////////////////////////////////////////////////

    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        PixelBufferArrivedTaskDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, TranscodeFormats.Auto,
            new DelegatedQueuingTaskProcessor(pixelBufferArrived, 1),
            ct);

    [Obsolete("This overload is obsoleted, please use TranscodeFormats parameter instead.")]
    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        PixelBufferArrivedTaskDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeIfYUV,
            new DelegatedQueuingTaskProcessor(pixelBufferArrived, 1),
            ct);

    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        PixelBufferArrivedTaskDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeFormat,
            new DelegatedQueuingTaskProcessor(pixelBufferArrived, 1),
            ct);

    [Obsolete("This overload is obsoleted, please use TranscodeFormats parameter instead.")]
    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        bool isScattering,
        int maxQueuingFrames,
        PixelBufferArrivedTaskDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeIfYUV,
            isScattering ?
                new DelegatedScatteringTaskProcessor(pixelBufferArrived, maxQueuingFrames) :
                new DelegatedQueuingTaskProcessor(pixelBufferArrived, maxQueuingFrames),
            ct);

    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        bool isScattering,
        int maxQueuingFrames,
        PixelBufferArrivedTaskDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeFormat,
            isScattering ?
                new DelegatedScatteringTaskProcessor(pixelBufferArrived, maxQueuingFrames) :
                new DelegatedQueuingTaskProcessor(pixelBufferArrived, maxQueuingFrames),
            ct);

    //////////////////////////////////////////////////////////////////////////////////

    public static async Task<ObservableCaptureDevice> AsObservableAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        CancellationToken ct = default)
    {
        var observerProxy = new ObservableCaptureDevice.ObserverProxy();
        var captureDevice = await descriptor.OpenWithFrameProcessorAsync(
            characteristics, TranscodeFormats.Auto,
            new DelegatedQueuingProcessor(observerProxy.OnPixelBufferArrived, 1),
            ct).
            ConfigureAwait(false);

        return new ObservableCaptureDevice(captureDevice, observerProxy);
    }

    [Obsolete("This overload is obsoleted, please use TranscodeFormats parameter instead.")]
    public static async Task<ObservableCaptureDevice> AsObservableAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        CancellationToken ct = default)
    {
        var observerProxy = new ObservableCaptureDevice.ObserverProxy();
        var captureDevice = await descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeIfYUV,
            new DelegatedQueuingProcessor(observerProxy.OnPixelBufferArrived, 1),
            ct).
            ConfigureAwait(false);

        return new ObservableCaptureDevice(captureDevice, observerProxy);
    }

    public static async Task<ObservableCaptureDevice> AsObservableAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        CancellationToken ct = default)
    {
        var observerProxy = new ObservableCaptureDevice.ObserverProxy();
        var captureDevice = await descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeFormat,
            new DelegatedQueuingProcessor(observerProxy.OnPixelBufferArrived, 1),
            ct).
            ConfigureAwait(false);

        return new ObservableCaptureDevice(captureDevice, observerProxy);
    }

    [Obsolete("This overload is obsoleted, please use TranscodeFormats parameter instead.")]
    public static async Task<ObservableCaptureDevice> AsObservableAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        bool isScattering,
        int maxQueuingFrames,
        CancellationToken ct = default)
    {
        var observerProxy = new ObservableCaptureDevice.ObserverProxy();
        var captureDevice = await descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeIfYUV,
            isScattering ?
                new DelegatedScatteringProcessor(observerProxy.OnPixelBufferArrived, maxQueuingFrames) :
                new DelegatedQueuingProcessor(observerProxy.OnPixelBufferArrived, maxQueuingFrames),
            ct).
            ConfigureAwait(false);

        return new ObservableCaptureDevice(captureDevice, observerProxy);
    }

    public static async Task<ObservableCaptureDevice> AsObservableAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        bool isScattering,
        int maxQueuingFrames,
        CancellationToken ct = default)
    {
        var observerProxy = new ObservableCaptureDevice.ObserverProxy();
        var captureDevice = await descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeFormat,
            isScattering ?
                new DelegatedScatteringProcessor(observerProxy.OnPixelBufferArrived, maxQueuingFrames) :
                new DelegatedQueuingProcessor(observerProxy.OnPixelBufferArrived, maxQueuingFrames),
            ct).
            ConfigureAwait(false);

        return new ObservableCaptureDevice(captureDevice, observerProxy);
    }

    //////////////////////////////////////////////////////////////////////////////////

    public static Task<byte[]> TakeOneShotAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        CancellationToken ct = default) =>
        descriptor.InternalTakeOneShotAsync(characteristics, TranscodeFormats.Auto, ct);

    [Obsolete("This overload is obsoleted, please use TranscodeFormats parameter instead.")]
    public static Task<byte[]> TakeOneShotAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        CancellationToken ct = default) =>
        descriptor.InternalTakeOneShotAsync(
            characteristics,
            transcodeIfYUV ? TranscodeFormats.Auto : TranscodeFormats.DoNotTranscode,
            ct);

    public static Task<byte[]> TakeOneShotAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        CancellationToken ct = default) =>
        descriptor.InternalTakeOneShotAsync(characteristics, transcodeFormat, ct);
}
