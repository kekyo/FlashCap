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

    public static class CaptureDeviceDescriptorExtension
    {
        public static ICaptureDevice OpenWithFrameArrived(
            this ICaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            FrameArrivedDelegate frameArrived,
            bool transcodeIfYUV = true) =>
            descriptor.OpenWithFrameProcessor(
                characteristics, transcodeIfYUV,
                new DelegatedFrameProcessor(frameArrived));

        public static ICaptureDevice Open(
            this ICaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            PixelBufferArrivedDelegate pixelBufferArrived,
            bool scattering = false,
            bool transcodeIfYUV = true) =>
            descriptor.OpenWithFrameProcessor(
                characteristics, transcodeIfYUV,
                scattering ?
                    new DelegatedConstraintPixelBufferProcessor(pixelBufferArrived) :
                    new DelegatedScatteringPixelBufferProcessor(pixelBufferArrived));

#if NET40_OR_GREATER || NETSTANDARD || NETCOREAPP
        public static ICaptureDevice Open(
            this ICaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            PixelBufferArrivedTaskDelegate pixelBufferArrived,
            bool scattering = false,
            bool transcodeIfYUV = true) =>
            descriptor.OpenWithFrameProcessor(
                characteristics, transcodeIfYUV,
                scattering ?
                new DelegatedConstraintPixelBufferTaskProcessor(pixelBufferArrived) :
                new DelegatedScatteringPixelBufferTaskProcessor(pixelBufferArrived));

#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
        public static ICaptureDevice Open(
            this ICaptureDeviceDescriptor descriptor,
            VideoCharacteristics characteristics,
            PixelBufferArrivedValueTaskDelegate pixelBufferArrived,
            bool scattering = false,
            bool transcodeIfYUV = true) =>
            descriptor.OpenWithFrameProcessor(
                characteristics, transcodeIfYUV,
                scattering ?
                new DelegatedConstraintPixelBufferValueTaskProcessor(pixelBufferArrived) :
                new DelegatedScatteringPixelBufferValueTaskProcessor(pixelBufferArrived));
#endif
#endif
    }
}
