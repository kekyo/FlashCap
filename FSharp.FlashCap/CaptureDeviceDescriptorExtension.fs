////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace FlashCap

open FlashCap.FrameProcessors
open System.Threading.Tasks

[<AutoOpen>]
module public CaptureDeviceDescriptorExtension =
            
    let inline private asTask (pixelBufferArrived: PixelBufferScope -> Async<unit>) =
        fun pixelBufferScope -> pixelBufferArrived(pixelBufferScope) |> Async.StartImmediateAsTask :> Task

    type public CaptureDeviceDescriptor with

        member self.openAsync(
            characteristics: VideoCharacteristics,
            pixelBufferArrived: PixelBufferScope -> unit) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics, true,
                new DelegatedQueuingProcessor(pixelBufferArrived, 1)) |> Async.AwaitTask

        member self.openAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            pixelBufferArrived: PixelBufferScope -> unit) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                new DelegatedQueuingProcessor(pixelBufferArrived, 1)) |> Async.AwaitTask

        member self.openAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            isScattering: bool,
            maxQueuingFrames: int,
            pixelBufferArrived: PixelBufferScope -> unit) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                match isScattering with
                | true -> (new DelegatedScatteringProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)
                | false -> (new DelegatedQueuingProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)) |> Async.AwaitTask

        //////////////////////////////////////////////////////////////////////////////////

        member self.openAsync(
            characteristics: VideoCharacteristics,
            pixelBufferArrived: PixelBufferScope -> Async<unit>) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics, true,
                new DelegatedQueuingTaskProcessor(asTask pixelBufferArrived, 1)) |> Async.AwaitTask
        
        member self.openAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            pixelBufferArrived: PixelBufferScope -> Async<unit>) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                new DelegatedQueuingTaskProcessor(asTask pixelBufferArrived, 1)) |> Async.AwaitTask

        member self.openAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            isScattering: bool,
            maxQueuingFrames: int,
            pixelBufferArrived: PixelBufferScope -> Async<unit>) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics, transcodeIfYUV,
                match isScattering with
                | true -> (new DelegatedScatteringTaskProcessor(asTask pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)
                | false -> (new DelegatedQueuingTaskProcessor(asTask pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)) |> Async.AwaitTask

        //////////////////////////////////////////////////////////////////////////////////
        
        member self.asObservableAsync(
            characteristics: VideoCharacteristics) : Async<ObservableCaptureDevice> = async {
                let observerProxy = new ObservableCaptureDevice.ObserverProxy()
                let! captureDevice = self.InternalOpenWithFrameProcessorAsync(
                    characteristics, true,
                    new DelegatedQueuingProcessor(
                        new PixelBufferArrivedDelegate(observerProxy.OnPixelBufferArrived), 1)) |> Async.AwaitTask
                return new ObservableCaptureDevice(captureDevice, observerProxy)
            }

        member self.asObservableAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool) : Async<ObservableCaptureDevice> = async {
                let observerProxy = new ObservableCaptureDevice.ObserverProxy()
                let! captureDevice = self.InternalOpenWithFrameProcessorAsync(
                    characteristics, transcodeIfYUV,
                    new DelegatedQueuingProcessor(
                        new PixelBufferArrivedDelegate(observerProxy.OnPixelBufferArrived), 1)) |> Async.AwaitTask
                return new ObservableCaptureDevice(captureDevice, observerProxy)
            }
 
        member self.asObservableAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            isScattering: bool,
            maxQueuingFrames: int) : Async<ObservableCaptureDevice> = async {
                let observerProxy = new ObservableCaptureDevice.ObserverProxy()
                let pixelBufferArrived = new PixelBufferArrivedDelegate(observerProxy.OnPixelBufferArrived)
                let! captureDevice = self.InternalOpenWithFrameProcessorAsync(
                    characteristics, transcodeIfYUV,
                    match isScattering with
                    | true -> (new DelegatedScatteringProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)
                    | false -> (new DelegatedQueuingProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)) |> Async.AwaitTask
                return new ObservableCaptureDevice(captureDevice, observerProxy)
            }
