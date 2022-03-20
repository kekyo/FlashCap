////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using FlashCap.Utilities;

namespace FlashCap.Devices
{
    public sealed class DirectShowDevice : ICaptureDevice
    {
        private sealed class SampleGrabberSink :
            NativeMethods_DirectShow.ISampleGrabberCB, IDisposable
        {
            private DirectShowDevice? parent;
            private FrameArrivedEventArgs? e = new();

            public SampleGrabberSink(DirectShowDevice parent) =>
                this.parent = parent;

            public void Dispose()
            {
                this.FrameArrived = null;
                this.e = null;
                this.parent = null;
            }

            public event EventHandler<FrameArrivedEventArgs>? FrameArrived;

            // whichMethodToCallback: 0
            [PreserveSig] public int SampleCB(
                double sampleTime, NativeMethods_DirectShow.IMediaSample sample) =>
                unchecked((int)0x80004001);  // E_NOTIMPL

            // whichMethodToCallback: 1
            [PreserveSig] public int BufferCB(
                double sampleTime, IntPtr pBuffer, int bufferLen)
            {
                if (this.FrameArrived is { } fa)
                {
                    // HACK: Dodge stupid camera devices...
                    if (bufferLen >= 64)
                    {
                        try
                        {
                            e!.Update(pBuffer, bufferLen, sampleTime * 1000);
                            fa(this.parent, e);
                        }
                        // DANGER: Stop leaking exception around outside of unmanaged area...
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                        }
                    }
                }
                return 0;
            }
        }

        private readonly bool transcodeIfYUV;
        private NativeMethods_DirectShow.IGraphBuilder? graphBuilder;
        private SampleGrabberSink? sampleGrabberSink;
        private IntPtr pBih;

        internal DirectShowDevice(
            string devicePath, VideoCharacteristics characteristics, bool transcodeIfYUV)
        {
            this.transcodeIfYUV = transcodeIfYUV;

            if (NativeMethods_DirectShow.EnumerateDeviceMoniker(
                NativeMethods_DirectShow.CLSID_VideoInputDeviceCategory).
                Where(moniker =>
                    moniker.GetPropertyBag() is { } pb &&
                    pb.SafeReleaseBlock(pb =>
                        pb.GetValue("DevicePath", default(string))?.Trim() is { } dp &&
                        dp.Equals(devicePath))).
                Collect(moniker =>
                    moniker.BindToObject(null, null, in NativeMethods_DirectShow.IID_IBaseFilter, out var captureSource) == 0 ?
                    captureSource as NativeMethods_DirectShow.IBaseFilter : null).
                FirstOrDefault() is { } captureSource)
            {
                try
                {
                    if (captureSource.EnumeratePins().
                        Collect(pin =>
                            pin.GetPinInfo() is { } pinInfo &&
                            pinInfo.dir == NativeMethods_DirectShow.PIN_DIRECTION.Output ?
                                pin : null).
                        SelectMany(pin =>
                            pin.EnumerateFormats().
                            Collect(format =>
                            {
                                var vfc = format.CreateVideoCharacteristics();
                                return characteristics.Equals(vfc) ?
                                    new { pin, format, vfc } : null;
                            })).
                        FirstOrDefault() is { } entry)
                    {
                        this.Characteristics = entry.vfc;
                        entry.pin.SetFormat(entry.format);
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't set video format: DevicePath={devicePath}");
                    }

                    ///////////////////////////////

                    this.graphBuilder = NativeMethods_DirectShow.CreateGraphBuilder();
                    if (this.graphBuilder.AddFilter(captureSource, "Capture source") < 0)
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't add capture source: DevicePath={devicePath}");
                    }

                    ///////////////////////////////

                    var sampleGrabber = NativeMethods_DirectShow.CreateSampleGrabber();
                    if (this.graphBuilder.AddFilter(sampleGrabber, "Sample grabber") < 0)
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't add sample grabber: DevicePath={devicePath}");
                    }

                    if (sampleGrabber.SetOneShot(false) < 0)
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't set oneshot mode: DevicePath={devicePath}");
                    }
                    if (sampleGrabber.SetBufferSamples(true) < 0)
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't start sampling: DevicePath={devicePath}");
                    }

                    ///////////////////////////////

                    var nullRenderer = NativeMethods_DirectShow.CreateNullRenderer();
                    if (this.graphBuilder.AddFilter(nullRenderer, "Null renderer") < 0)
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't add null renderer: DevicePath={devicePath}");
                    }

                    ///////////////////////////////

                    var captureGraphBuilder = NativeMethods_DirectShow.CreateCaptureGraphBuilder();
                    if (captureGraphBuilder.SetFiltergraph(this.graphBuilder) < 0)
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't set graph builder: DevicePath={devicePath}");
                    }

                    ///////////////////////////////

                    if (captureGraphBuilder.RenderStream(
                        in NativeMethods_DirectShow.PIN_CATEGORY_CAPTURE,
                        in NativeMethods_DirectShow.MEDIATYPE_Video,
                        captureSource,
                        sampleGrabber,
                        nullRenderer) < 0)
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't set render stream: DevicePath={devicePath}");
                    }

                    ///////////////////////////////

                    if (sampleGrabber.GetConnectedMediaType(out var mediaType) < 0)
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't get media type: DevicePath={devicePath}");
                    }

                    this.pBih = mediaType.AllocateAndGetBih();

                    ///////////////////////////////

                    this.sampleGrabberSink = new SampleGrabberSink(this);
                    if (sampleGrabber.SetCallback(this.sampleGrabberSink, 1) < 0)
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't get grabbing media type: DevicePath={devicePath}");
                    }
                }
                catch
                {
                    if (this.graphBuilder != null)
                    {
                        Marshal.ReleaseComObject(this.graphBuilder);
                    }
                    throw;
                }
            }
            else
            {
                throw new ArgumentException(
                    $"FlashCap: Couldn't find a device: DevicePath={devicePath}");
            }
        }

        ~DirectShowDevice() =>
            this.Dispose();

        public void Dispose()
        {
            if (this.graphBuilder != null)
            {
                Marshal.ReleaseComObject(this.graphBuilder);
                this.graphBuilder = null!;
                this.sampleGrabberSink!.Dispose();
                this.sampleGrabberSink = null!;
                NativeMethods.FreeMemory(this.pBih);
                this.pBih = IntPtr.Zero;
            }
        }

        public VideoCharacteristics Characteristics { get; }

        public event EventHandler<FrameArrivedEventArgs>? FrameArrived
        {
            add
            {
                if (this.sampleGrabberSink is { } sgs)
                {
                    sgs.FrameArrived += value;
                }
            }
            remove
            {
                if (this.sampleGrabberSink is { } sgs)
                {
                    sgs.FrameArrived -= value;
                }
            }
        }

        public void Start()
        {
            if (this.graphBuilder is NativeMethods_DirectShow.IMediaControl mediaControl)
            {
                mediaControl.Run();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void Stop()
        {
            if (this.graphBuilder is NativeMethods_DirectShow.IMediaControl mediaControl)
            {
                mediaControl.Stop();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void Capture(FrameArrivedEventArgs e, PixelBuffer buffer) =>
            buffer.CopyIn(
                this.pBih, e.pData, e.size,
                e.timestampMilliseconds, this.transcodeIfYUV);
    }
}
