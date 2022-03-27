////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.FrameProcessors;

namespace FlashCap.Synchronized
{
    public static class CaptureDeviceDescriptorExtension
    {
        public static CaptureDevice OpenWithFrameProcessor(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            FrameProcessor frameProcessor) =>
            descriptor.InternalOpenWithFrameProcessor(
                characteristics, transcodeIfYUV, frameProcessor);

        public static CaptureDevice Open(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            descriptor.InternalOpenWithFrameProcessor(
                characteristics, true,
                new DelegatedIgnoreDroppingProcessor(pixelBufferArrived));

        public static CaptureDevice Open(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            HandlerStrategies handlerStrategy,
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            descriptor.InternalOpenWithFrameProcessor(
                characteristics, transcodeIfYUV,
                handlerStrategy switch
                {
                    HandlerStrategies.Queuing =>
                        new DelegatedQueuingProcessor(pixelBufferArrived),
                    HandlerStrategies.Scattering =>
                        new DelegatedScatteringProcessor(pixelBufferArrived),
                    _ =>
                        new DelegatedIgnoreDroppingProcessor(pixelBufferArrived),
                });

#if NET35_OR_GREATER || NETSTANDARD || NETCOREAPP
        public static CaptureDevice Open(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            descriptor.InternalOpenWithFrameProcessor(
                characteristics, true,
                new DelegatedIgnoreDroppingTaskProcessor(pixelBufferArrived));

        public static CaptureDevice Open(
            this CaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            HandlerStrategies handlerStrategy,
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            descriptor.InternalOpenWithFrameProcessor(
                characteristics, transcodeIfYUV,
                handlerStrategy switch
                {
                    HandlerStrategies.Queuing =>
                        new DelegatedQueuingTaskProcessor(pixelBufferArrived),
                    HandlerStrategies.Scattering =>
                        new DelegatedScatteringTaskProcessor(pixelBufferArrived),
                    _ =>
                        new DelegatedIgnoreDroppingTaskProcessor(pixelBufferArrived),
                });
#endif
    }
}
