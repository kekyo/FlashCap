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
using System.Threading.Tasks;

namespace FlashCap
{
    public delegate void PixelBufferArrivedDelegate(
        PixelBufferScope bufferScope);

    public delegate Task PixelBufferArrivedTaskDelegate(
        PixelBufferScope bufferScope);

    public static class CaptureDeviceDescriptorExtension
    {
        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, true,
                new DelegatedQueuingProcessor(pixelBufferArrived, 1));

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                new DelegatedQueuingProcessor(pixelBufferArrived, 1));

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            bool isScattering,
            int maxQueuingFrames,
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                isScattering ?
                    new DelegatedScatteringProcessor(pixelBufferArrived, maxQueuingFrames) :
                    new DelegatedQueuingProcessor(pixelBufferArrived, maxQueuingFrames));

        //////////////////////////////////////////////////////////////////////////////////

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, true,
                new DelegatedQueuingTaskProcessor(pixelBufferArrived, 1));

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                new DelegatedQueuingTaskProcessor(pixelBufferArrived, 1));

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            bool isScattering,
            int maxQueuingFrames,
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                isScattering ?
                    new DelegatedScatteringTaskProcessor(pixelBufferArrived, maxQueuingFrames) :
                    new DelegatedQueuingTaskProcessor(pixelBufferArrived, maxQueuingFrames));

        //////////////////////////////////////////////////////////////////////////////////

        public static async Task<ObservableCaptureDevice> AsObservableAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics)
        {
            var observerProxy = new ObservableCaptureDevice.ObserverProxy();
            var captureDevice = await descriptor.OpenWithFrameProcessorAsync(
                characteristics, true,
                new DelegatedQueuingProcessor(observerProxy.OnPixelBufferArrived, 1)).
                ConfigureAwait(false);

            return new ObservableCaptureDevice(captureDevice, observerProxy);
        }

        public static async Task<ObservableCaptureDevice> AsObservableAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV)
        {
            var observerProxy = new ObservableCaptureDevice.ObserverProxy();
            var captureDevice = await descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                new DelegatedQueuingProcessor(observerProxy.OnPixelBufferArrived, 1)).
                ConfigureAwait(false);

            return new ObservableCaptureDevice(captureDevice, observerProxy);
        }

        public static async Task<ObservableCaptureDevice> AsObservableAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            bool isScattering,
            int maxQueuingFrames)
        {
            var observerProxy = new ObservableCaptureDevice.ObserverProxy();
            var captureDevice = await descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                isScattering ?
                    new DelegatedScatteringProcessor(observerProxy.OnPixelBufferArrived, maxQueuingFrames) :
                    new DelegatedQueuingProcessor(observerProxy.OnPixelBufferArrived, maxQueuingFrames)).
                ConfigureAwait(false);

            return new ObservableCaptureDevice(captureDevice, observerProxy);
        }
    }
}
