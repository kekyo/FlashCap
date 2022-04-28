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

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            IObserver<PixelBufferScope> observer) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, true,
                new DelegatedQueuingObservableProcessor(observer, 1));

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            IObserver<PixelBufferScope> observer) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                new DelegatedQueuingObservableProcessor(observer, 1));

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            bool isScattering,
            int maxQueuingFrames,
            IObserver<PixelBufferScope> observer) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                isScattering ?
                    new DelegatedScatteringObservableProcessor(observer, maxQueuingFrames) :
                    new DelegatedQueuingObservableProcessor(observer, maxQueuingFrames));
    }
}
