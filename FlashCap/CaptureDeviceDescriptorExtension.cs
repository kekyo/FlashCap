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
    public delegate void FrameArrivedDelegate(
        CaptureDevice captureDevice,
        IntPtr pData, int size, long timestampMilliseconds);

    public delegate void PixelBufferArrivedDelegate(
        PixelBuffer buffer);

#if NET40_OR_GREATER || NETSTANDARD || NETCOREAPP
    public delegate Task PixelBufferArrivedTaskDelegate(
        PixelBuffer buffer);
#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
    public delegate ValueTask PixelBufferArrivedValueTaskDelegate(
        PixelBuffer buffer);
#endif
#endif

    public enum HandlerStrategies
    {
        IgnoreDropping,
        Queuing,
        Scattering,
    }

    public static class CaptureDeviceDescriptorExtension
    {
        public static ICaptureDevice Open(
            this ICaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessor(
                characteristics, true,
                new DelegatedIgnoreDroppingProcessor(pixelBufferArrived));

        public static ICaptureDevice Open(
            this ICaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            HandlerStrategies handlerStrategy,
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessor(
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

#if NET40_OR_GREATER || NETSTANDARD || NETCOREAPP
        public static ICaptureDevice Open(
            this ICaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessor(
                characteristics, true,
                new DelegatedIgnoreDroppingTaskProcessor(pixelBufferArrived));

        public static ICaptureDevice Open(
            this ICaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            HandlerStrategies handlerStrategy,
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessor(
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

#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
        public static ICaptureDevice Open(
            this ICaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            PixelBufferArrivedValueTaskDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessor(
                characteristics, true,
                new DelegatedIgnoreDroppingValueTaskProcessor(pixelBufferArrived));

        public static ICaptureDevice Open(
            this ICaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            HandlerStrategies handlerStrategy,
            PixelBufferArrivedValueTaskDelegate pixelBufferArrived) =>
            descriptor.OpenWithFrameProcessor(
                characteristics, transcodeIfYUV,
                handlerStrategy switch
                {
                    HandlerStrategies.Queuing =>
                        new DelegatedQueuingValueTaskProcessor(pixelBufferArrived),
                    HandlerStrategies.Scattering =>
                        new DelegatedScatteringValueTaskProcessor(pixelBufferArrived),
                    _ =>
                        new DelegatedIgnoreDroppingValueTaskProcessor(pixelBufferArrived),
                });
#endif
#endif
    }
}
