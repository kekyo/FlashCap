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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Devices;

public sealed class DirectShowDevice :
    CaptureDevice
{
    private sealed class SampleGrabberSink :
        NativeMethods_DirectShow.ISampleGrabberCB
    {
        private DirectShowDevice parent;
        private FrameProcessor frameProcessor;
        private long frameIndex;

        public SampleGrabberSink(
            DirectShowDevice parent,
            FrameProcessor frameProcessor)
        {
            this.parent = parent;
            this.frameProcessor = frameProcessor;
        }

        public void ResetFrameIndex() =>
            this.frameIndex = 0;

        // whichMethodToCallback: 0
        [PreserveSig] public int SampleCB(
            double sampleTime, NativeMethods_DirectShow.IMediaSample sample) =>
            unchecked((int)0x80004001);  // E_NOTIMPL

        // whichMethodToCallback: 1
        [PreserveSig] public int BufferCB(
            double sampleTime, IntPtr pBuffer, int bufferLen)
        {
            // HACK: Avoid stupid camera devices...
            if (bufferLen >= 64)
            {
                try
                {
                    this.frameProcessor.OnFrameArrived(
                        this.parent, pBuffer, bufferLen,
                        (long)(sampleTime * 1_000_000),
                        this.frameIndex++);
                }
                // DANGER: Stop leaking exception around outside of unmanaged area...
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
            return 0;
        }
    }

    // DirectShow objects are sandboxed in the working context.
    private IndependentSingleApartmentContext? workingContext = new();
    private bool transcodeIfYUV;
    private FrameProcessor frameProcessor;
    private NativeMethods_DirectShow.IGraphBuilder? graphBuilder;
    private NativeMethods_DirectShow.ICaptureGraphBuilder2? captureGraphBuilder;
    private SampleGrabberSink? sampleGrabberSink;
    private IntPtr pBih;

#pragma warning disable CS8618
    internal DirectShowDevice(object identity, string name) :
        base(identity, name)
#pragma warning restore CS8618
    {
    }

    protected override Task OnInitializeAsync(
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        FrameProcessor frameProcessor,
        CancellationToken ct)
    {
        var devicePath = (string)this.Identity;

        this.transcodeIfYUV = transcodeIfYUV;
        this.frameProcessor = frameProcessor;

        return this.workingContext!.InvokeAsync(() =>
        {
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

                    captureGraphBuilder = NativeMethods_DirectShow.CreateCaptureGraphBuilder();
                    if (captureGraphBuilder.SetFiltergraph(this.graphBuilder) < 0)
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't set graph builder: DevicePath={devicePath}");
                    }

                    ///////////////////////////////
                    
                    captureGraphBuilder.FindInterface(Guid.Empty, Guid.Empty, captureSource, NativeMethods_DirectShow.IAMVideoProcAmpHelper.GUID, out object? videoProcAmpObject);
                    if (videoProcAmpObject != null)
                    {
                        var videoProcAmp = (NativeMethods_DirectShow.IAMVideoProcAmp)videoProcAmpObject;
                        videoProcAmp.GetRange(NativeMethods_DirectShow.VideoProcAmpProperty.Brightness, out int brightnessMin, out int brightnessMax, out int steppingDelta, out int def, out NativeMethods_DirectShow.VideoProcAmpFlags videoProcAmpFlags);
                        Properties.Add(VideoProcessingAmplifierProperty.Brightness, new DirectShowProperty(VideoProcessingAmplifierProperty.Brightness, brightnessMin, brightnessMax, steppingDelta));
                        Marshal.ReleaseComObject(videoProcAmpObject);
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

                    this.sampleGrabberSink =
                        new SampleGrabberSink(this, frameProcessor);
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
        }, ct);
    }

    ~DirectShowDevice()
    {
        if (this.pBih != IntPtr.Zero)
        {
            NativeMethods.FreeMemory(this.pBih);
            this.pBih = IntPtr.Zero;
        }
    }

    protected override async Task OnDisposeAsync()
    {
        if (this.graphBuilder != null)
        {
            await this.frameProcessor.DisposeAsync().
                ConfigureAwait(false);

            await this.OnStopAsync(default).
                ConfigureAwait(false);

            await this.workingContext!.InvokeAsync(() =>
            {
                Marshal.ReleaseComObject(this.graphBuilder);
                this.graphBuilder = null!;
                this.sampleGrabberSink = null!;
                NativeMethods.FreeMemory(this.pBih);
                this.pBih = IntPtr.Zero;
            }, default);

            this.workingContext.Dispose();
            this.workingContext = null;
        }
    }

    protected override Task OnStartAsync(CancellationToken ct)
    {
        if (!this.IsRunning)
        {
            return this.workingContext!.InvokeAsync(() =>
            {
                if (this.graphBuilder is NativeMethods_DirectShow.IMediaControl mediaControl)
                {
                    this.sampleGrabberSink!.ResetFrameIndex();

                    mediaControl.Run();
                    this.IsRunning = true;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }, ct);
        }
        else
        {
            return TaskCompat.CompletedTask;
        }
    }

    protected override Task OnStopAsync(CancellationToken ct)
    {
        if (this.IsRunning)
        {
            return this.workingContext!.InvokeAsync(() =>
            {
                if (this.graphBuilder is NativeMethods_DirectShow.IMediaControl mediaControl)
                {
                    this.IsRunning = false;
                    mediaControl.Stop();
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }, ct);
        }
        else
        {
            return TaskCompat.CompletedTask;
        }
    }

    public override int GetPropertyValue(VideoProcessingAmplifierProperty property)
    {
        if (Properties.TryGetValue(property, out CaptureDeviceProperty _)
            && graphBuilder != null
            && captureGraphBuilder != null)
        {
            graphBuilder.FindFilterByName("Capture source", out NativeMethods_DirectShow.IBaseFilter? captureSourceFilter);
            if (captureSourceFilter != null)
            {
                captureGraphBuilder.FindInterface(Guid.Empty, Guid.Empty, captureSourceFilter, NativeMethods_DirectShow.IAMVideoProcAmpHelper.GUID, out object? videoProcAmpObject);
                if (videoProcAmpObject != null)
                {
                    var videoProcAmp = (NativeMethods_DirectShow.IAMVideoProcAmp)videoProcAmpObject;
                    videoProcAmp.Get(DirectShowProperty.FromVideoProcessingAmplifierProperty(property), out int value, out NativeMethods_DirectShow.VideoProcAmpFlags _);
                    Marshal.ReleaseComObject(videoProcAmpObject);
                    return value;
                }
            }
        }

        throw new ArgumentException(
            $"FlashCap: Property is not supported by device: Property={property}");
    }

    public override void SetPropertyValue(VideoProcessingAmplifierProperty property, object? obj)
    {
        if (Properties.TryGetValue(property, out CaptureDeviceProperty? captureDeviceProperty)
            && obj != null
            && captureDeviceProperty != null
            && captureDeviceProperty.IsPropertyValueValid(obj)
            && graphBuilder != null
            && captureGraphBuilder != null)
        {
            graphBuilder.FindFilterByName("Capture source", out NativeMethods_DirectShow.IBaseFilter? captureSourceFilter);
            if (captureSourceFilter != null)
            {
                captureGraphBuilder.FindInterface(Guid.Empty, Guid.Empty, captureSourceFilter, NativeMethods_DirectShow.IAMVideoProcAmpHelper.GUID, out object? videoProcAmpObject);
                if (videoProcAmpObject != null)
                {
                    var videoProcAmp = (NativeMethods_DirectShow.IAMVideoProcAmp)videoProcAmpObject;
                    videoProcAmp.Get(DirectShowProperty.FromVideoProcessingAmplifierProperty(property), out int _, out NativeMethods_DirectShow.VideoProcAmpFlags videoProcAmpFlags);
                    videoProcAmp.Set(DirectShowProperty.FromVideoProcessingAmplifierProperty(property), (int)obj, videoProcAmpFlags);
                    Marshal.ReleaseComObject(videoProcAmpObject);
                }
            }
        }

        throw new ArgumentException(
            $"FlashCap: Property is not supported by device: Property={property}");
    }

    public override void DisplayPropertyPage_CaptureFilter(IntPtr hwndOwner)
    {
        if (graphBuilder != null
            && graphBuilder.FindFilterByName("Capture source", out NativeMethods_DirectShow.IBaseFilter? captureSourceFilter) >= 0)
        {
            DisplayPropertyPage_Filter(captureSourceFilter, hwndOwner);
        }
    }

    private void DisplayPropertyPage_Filter(object? obj, IntPtr hwndOwner)
    {
        if (obj == null)
            return;

        if (obj is not NativeMethods_DirectShow.ISpecifyPropertyPages pProp)
            return;

        if (obj is not NativeMethods_DirectShow.IBaseFilter filter)
            return;

        if (filter.QueryFilterInfo(out NativeMethods_DirectShow.FILTER_INFO filterInfo) < 0)
        {
            throw new Exception(
                $"FlashCap: Couldn't query filter info");
        }

        if (pProp.GetPages(out NativeMethods_DirectShow.DsCAUUID caGUID) < 0)
        {
            throw new Exception(
                $"FlashCap: Couldn't get pages");
        }

        object oDevice = obj;
        if (NativeMethods_DirectShow.OleCreatePropertyFrame(hwndOwner, 0, 0, filterInfo.chName, 1, ref oDevice, caGUID.cElems, caGUID.pElems, 0, 0, IntPtr.Zero) < 0)
        {
            throw new Exception(
                $"FlashCap: Couldn't create property frame");
        }

        Marshal.FreeCoTaskMem(caGUID.pElems);
        Marshal.ReleaseComObject(pProp);
        if (filterInfo.graph != null)
        {
            Marshal.ReleaseComObject(filterInfo.graph);
        }
    }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    protected override void OnCapture(
        IntPtr pData, int size,
        long timestampMicroseconds, long frameIndex,
        PixelBuffer buffer) =>
        buffer.CopyIn(this.pBih, pData, size, timestampMicroseconds, frameIndex, this.transcodeIfYUV);
}
