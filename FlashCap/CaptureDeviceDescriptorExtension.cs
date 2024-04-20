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
            new DelegatedQueuingProcessor(pixelBufferArrived, 1, descriptor.defaultBufferPool),
            ct);

    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        PixelBufferArrivedDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeFormat,
            new DelegatedQueuingProcessor(pixelBufferArrived, 1, descriptor.defaultBufferPool),
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
                new DelegatedScatteringProcessor(pixelBufferArrived, maxQueuingFrames, descriptor.defaultBufferPool) :
                new DelegatedQueuingProcessor(pixelBufferArrived, maxQueuingFrames, descriptor.defaultBufferPool),
            ct);

    //////////////////////////////////////////////////////////////////////////////////

    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        PixelBufferArrivedTaskDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, TranscodeFormats.Auto,
            new DelegatedQueuingTaskProcessor(pixelBufferArrived, 1, descriptor.defaultBufferPool),
            ct);

    public static Task<CaptureDevice> OpenAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        PixelBufferArrivedTaskDelegate pixelBufferArrived,
        CancellationToken ct = default) =>
        descriptor.OpenWithFrameProcessorAsync(
            characteristics, transcodeFormat,
            new DelegatedQueuingTaskProcessor(pixelBufferArrived, 1, descriptor.defaultBufferPool),
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
                new DelegatedScatteringTaskProcessor(pixelBufferArrived, maxQueuingFrames, descriptor.defaultBufferPool) :
                new DelegatedQueuingTaskProcessor(pixelBufferArrived, maxQueuingFrames, descriptor.defaultBufferPool),
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
            new DelegatedQueuingProcessor(observerProxy.OnPixelBufferArrived, 1, descriptor.defaultBufferPool),
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
            new DelegatedQueuingProcessor(observerProxy.OnPixelBufferArrived, 1, descriptor.defaultBufferPool),
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
                new DelegatedScatteringProcessor(observerProxy.OnPixelBufferArrived, maxQueuingFrames, descriptor.defaultBufferPool) :
                new DelegatedQueuingProcessor(observerProxy.OnPixelBufferArrived, maxQueuingFrames, descriptor.defaultBufferPool),
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

    public static Task<byte[]> TakeOneShotAsync(
        this CaptureDeviceDescriptor descriptor,
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        CancellationToken ct = default) =>
        descriptor.InternalTakeOneShotAsync(characteristics, transcodeFormat, ct);
}
