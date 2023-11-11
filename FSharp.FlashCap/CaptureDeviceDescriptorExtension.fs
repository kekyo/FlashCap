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
open System
open System.ComponentModel
open System.Threading.Tasks
open System.Threading

[<AutoOpen>]
module public CaptureDeviceDescriptorExtension =
    
    let inline private asCT (ct: CancellationToken option) =
        match ct with
        | Some(ct) -> ct
        | _ -> CancellationToken()

    let inline private asTask (pixelBufferArrived: PixelBufferScope -> Async<unit>) =
        fun pixelBufferScope -> pixelBufferArrived(pixelBufferScope) |> Async.StartImmediateAsTask :> Task

    let inline private toFormat (transcodeIfYUV: bool) =
        match transcodeIfYUV with
        | true -> TranscodeFormats.Auto
        | false -> TranscodeFormats.DoNotTranscode

    type public CaptureDeviceDescriptor with

        member self.openDevice(
            characteristics: VideoCharacteristics,
            pixelBufferArrived: PixelBufferScope -> unit,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics, TranscodeFormats.Auto,
                new DelegatedQueuingProcessor(pixelBufferArrived, 1),
                asCT ct) |> Async.AwaitTask

        member self.openDevice(
            characteristics: VideoCharacteristics,
            transcodeFormat: TranscodeFormats,
            pixelBufferArrived: PixelBufferScope -> unit,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics,
                transcodeFormat,
                new DelegatedQueuingProcessor(pixelBufferArrived, 1),
                asCT ct) |> Async.AwaitTask

        member self.openDevice(
            characteristics: VideoCharacteristics,
            transcodeFormat: TranscodeFormats,
            isScattering: bool,
            maxQueuingFrames: int,
            pixelBufferArrived: PixelBufferScope -> unit,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics,
                transcodeFormat,
                (match isScattering with
                 | true -> (new DelegatedScatteringProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)
                 | false -> (new DelegatedQueuingProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)),
                asCT ct) |> Async.AwaitTask

        //////////////////////////////////////////////////////////////////////////////////

        member self.openDevice(
            characteristics: VideoCharacteristics,
            pixelBufferArrived: PixelBufferScope -> Async<unit>,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics,
                TranscodeFormats.Auto,
                new DelegatedQueuingTaskProcessor(asTask pixelBufferArrived, 1),
                asCT ct) |> Async.AwaitTask
        
        member self.openDevice(
            characteristics: VideoCharacteristics,
            transcodeFormat: TranscodeFormats,
            pixelBufferArrived: PixelBufferScope -> Async<unit>,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics,
                transcodeFormat,
                new DelegatedQueuingTaskProcessor(asTask pixelBufferArrived, 1),
                asCT ct) |> Async.AwaitTask

        member self.openDevice(
            characteristics: VideoCharacteristics,
            transcodeFormat: TranscodeFormats,
            isScattering: bool,
            maxQueuingFrames: int,
            pixelBufferArrived: PixelBufferScope -> Async<unit>,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics,
                transcodeFormat,
                (match isScattering with
                 | true -> (new DelegatedScatteringTaskProcessor(asTask pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)
                 | false -> (new DelegatedQueuingTaskProcessor(asTask pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)),
                asCT ct) |> Async.AwaitTask

        //////////////////////////////////////////////////////////////////////////////////
        
        member self.asObservable(
            characteristics: VideoCharacteristics,
            ?ct: CancellationToken) : Async<ObservableCaptureDevice> = async {
                let observerProxy = new ObservableCaptureDevice.ObserverProxy()
                let! captureDevice = self.InternalOpenWithFrameProcessorAsync(
                    characteristics,
                    TranscodeFormats.Auto,
                    (new DelegatedQueuingProcessor(
                        new PixelBufferArrivedDelegate(observerProxy.OnPixelBufferArrived), 1)), asCT ct) |> Async.AwaitTask
                return new ObservableCaptureDevice(captureDevice, observerProxy)
            }

        member self.asObservable(
            characteristics: VideoCharacteristics,
            transcodeFormat: TranscodeFormats,
            ?ct: CancellationToken) : Async<ObservableCaptureDevice> = async {
                let observerProxy = new ObservableCaptureDevice.ObserverProxy()
                let! captureDevice = self.InternalOpenWithFrameProcessorAsync(
                    characteristics,
                    transcodeFormat,
                    (new DelegatedQueuingProcessor(
                        new PixelBufferArrivedDelegate(observerProxy.OnPixelBufferArrived), 1)), asCT ct) |> Async.AwaitTask
                return new ObservableCaptureDevice(captureDevice, observerProxy)
            }
 
        member self.asObservable(
            characteristics: VideoCharacteristics,
            transcodeFormat: TranscodeFormats,
            isScattering: bool,
            maxQueuingFrames: int,
            ?ct: CancellationToken) : Async<ObservableCaptureDevice> = async {
                let observerProxy = new ObservableCaptureDevice.ObserverProxy()
                let pixelBufferArrived = new PixelBufferArrivedDelegate(observerProxy.OnPixelBufferArrived)
                let! captureDevice = self.InternalOpenWithFrameProcessorAsync(
                    characteristics,
                    transcodeFormat,
                    (match isScattering with
                     | true -> (new DelegatedScatteringProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)
                     | false -> (new DelegatedQueuingProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)), asCT ct) |> Async.AwaitTask
                return new ObservableCaptureDevice(captureDevice, observerProxy)
            }

        //////////////////////////////////////////////////////////////////////////////////

        member self.takeOneShot(
            characteristics: VideoCharacteristics,
            ?ct: CancellationToken) : Async<byte[]> =
            self.InternalTakeOneShotAsync(
                characteristics,
                TranscodeFormats.Auto,
                asCT ct) |> Async.AwaitTask

        member self.takeOneShot(
            characteristics: VideoCharacteristics,
            transcodeFormat: TranscodeFormats,
            ?ct: CancellationToken) : Async<byte[]> =
            self.InternalTakeOneShotAsync(
                characteristics,
                transcodeFormat,
                asCT ct) |> Async.AwaitTask

        //////////////////////////////////////////////////////////////////////////////////

        [<Obsolete("This function is obsoleted, please use `openDevice` instead.")>]
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member self.openAsync(
            characteristics: VideoCharacteristics,
            pixelBufferArrived: PixelBufferScope -> unit,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics, TranscodeFormats.Auto,
                new DelegatedQueuingProcessor(pixelBufferArrived, 1),
                asCT ct) |> Async.AwaitTask

        [<Obsolete("This function is obsoleted, please use `openDevice` instead.")>]
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member self.openAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            pixelBufferArrived: PixelBufferScope -> unit,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics,
                toFormat transcodeIfYUV,
                new DelegatedQueuingProcessor(pixelBufferArrived, 1),
                asCT ct) |> Async.AwaitTask

        [<Obsolete("This function is obsoleted, please use `openDevice` instead.")>]
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member self.openAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            isScattering: bool,
            maxQueuingFrames: int,
            pixelBufferArrived: PixelBufferScope -> unit,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics,
                toFormat transcodeIfYUV,
                (match isScattering with
                 | true -> (new DelegatedScatteringProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)
                 | false -> (new DelegatedQueuingProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)),
                asCT ct) |> Async.AwaitTask

        //////////////////////////////////////////////////////////////////////////////////

        [<Obsolete("This function is obsoleted, please use `openDevice` instead.")>]
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member self.openAsync(
            characteristics: VideoCharacteristics,
            pixelBufferArrived: PixelBufferScope -> Async<unit>,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics,
                TranscodeFormats.Auto,
                new DelegatedQueuingTaskProcessor(asTask pixelBufferArrived, 1),
                asCT ct) |> Async.AwaitTask
        
        [<Obsolete("This function is obsoleted, please use `openDevice` instead.")>]
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member self.openAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            pixelBufferArrived: PixelBufferScope -> Async<unit>,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics,
                toFormat transcodeIfYUV,
                new DelegatedQueuingTaskProcessor(asTask pixelBufferArrived, 1),
                asCT ct) |> Async.AwaitTask

        [<Obsolete("This function is obsoleted, please use `openDevice` instead.")>]
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member self.openAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            isScattering: bool,
            maxQueuingFrames: int,
            pixelBufferArrived: PixelBufferScope -> Async<unit>,
            ?ct: CancellationToken) : Async<CaptureDevice> =
            self.InternalOpenWithFrameProcessorAsync(
                characteristics,
                toFormat transcodeIfYUV,
                (match isScattering with
                 | true -> (new DelegatedScatteringTaskProcessor(asTask pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)
                 | false -> (new DelegatedQueuingTaskProcessor(asTask pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)),
                asCT ct) |> Async.AwaitTask

        //////////////////////////////////////////////////////////////////////////////////
        
        [<Obsolete("This function is obsoleted, please use `asObservable` instead.")>]
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member self.asObservableAsync(
            characteristics: VideoCharacteristics,
            ?ct: CancellationToken) : Async<ObservableCaptureDevice> = async {
                let observerProxy = new ObservableCaptureDevice.ObserverProxy()
                let! captureDevice = self.InternalOpenWithFrameProcessorAsync(
                    characteristics,
                    TranscodeFormats.Auto,
                    (new DelegatedQueuingProcessor(
                        new PixelBufferArrivedDelegate(observerProxy.OnPixelBufferArrived), 1)), asCT ct) |> Async.AwaitTask
                return new ObservableCaptureDevice(captureDevice, observerProxy)
            }

        [<Obsolete("This function is obsoleted, please use `asObservable` instead.")>]
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member self.asObservableAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            ?ct: CancellationToken) : Async<ObservableCaptureDevice> = async {
                let observerProxy = new ObservableCaptureDevice.ObserverProxy()
                let! captureDevice = self.InternalOpenWithFrameProcessorAsync(
                    characteristics,
                    toFormat transcodeIfYUV,
                    (new DelegatedQueuingProcessor(
                        new PixelBufferArrivedDelegate(observerProxy.OnPixelBufferArrived), 1)), asCT ct) |> Async.AwaitTask
                return new ObservableCaptureDevice(captureDevice, observerProxy)
            }
 
        [<Obsolete("This function is obsoleted, please use `asObservable` instead.")>]
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member self.asObservableAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            isScattering: bool,
            maxQueuingFrames: int,
            ?ct: CancellationToken) : Async<ObservableCaptureDevice> = async {
                let observerProxy = new ObservableCaptureDevice.ObserverProxy()
                let pixelBufferArrived = new PixelBufferArrivedDelegate(observerProxy.OnPixelBufferArrived)
                let! captureDevice = self.InternalOpenWithFrameProcessorAsync(
                    characteristics,
                    toFormat transcodeIfYUV,
                    (match isScattering with
                     | true -> (new DelegatedScatteringProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)
                     | false -> (new DelegatedQueuingProcessor(pixelBufferArrived, maxQueuingFrames) :> FrameProcessor)), asCT ct) |> Async.AwaitTask
                return new ObservableCaptureDevice(captureDevice, observerProxy)
            }

        //////////////////////////////////////////////////////////////////////////////////

        [<Obsolete("This function is obsoleted, please use `takeOneShot` instead.")>]
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member self.takeOneShotAsync(
            characteristics: VideoCharacteristics,
            ?ct: CancellationToken) : Async<byte[]> =
            self.InternalTakeOneShotAsync(
                characteristics,
                TranscodeFormats.Auto,
                asCT ct) |> Async.AwaitTask

        [<Obsolete("This function is obsoleted, please use `takeOneShot` instead.")>]
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member self.takeOneShotAsync(
            characteristics: VideoCharacteristics,
            transcodeIfYUV: bool,
            ?ct: CancellationToken) : Async<byte[]> =
            self.InternalTakeOneShotAsync(
                characteristics,
                toFormat transcodeIfYUV,
                asCT ct) |> Async.AwaitTask
