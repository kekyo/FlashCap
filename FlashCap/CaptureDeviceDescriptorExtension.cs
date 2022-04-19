////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.FrameProcessors;
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
                new DelegatedQueuingProcessor(pixelBufferArrived));

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                new DelegatedQueuingProcessor(pixelBufferArrived));

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            bool isScattering,
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                isScattering ?
                    new DelegatedScatteringProcessor(pixelBufferArrived) :
                    new DelegatedQueuingProcessor(pixelBufferArrived));

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, true,
                new DelegatedQueuingTaskProcessor(pixelBufferArrived));

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                new DelegatedQueuingTaskProcessor(pixelBufferArrived));

        public static Task<CaptureDevice> OpenAsync(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            bool isScattering,
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                isScattering ?
                    new DelegatedScatteringTaskProcessor(pixelBufferArrived) :
                    new DelegatedQueuingTaskProcessor(pixelBufferArrived));
    }
}
