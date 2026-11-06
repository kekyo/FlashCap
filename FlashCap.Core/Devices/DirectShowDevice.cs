////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Devices;
#if NET6_0_OR_GREATER // Remove this when dropping net5.0. Its required because RequiresUnreferencedCode was not allowed on class level in net5.0
[RequiresUnreferencedCode("Direct show depends on COM runtime generation and might require unreferenced code.")]
#endif
[SupportedOSPlatform("windows")]
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
    private TranscodeFormats transcodeFormat;
    private FrameProcessor frameProcessor;
    private NativeMethods_DirectShow.IGraphBuilder? graphBuilder;
    // COM object of the capture source, its control interfaces are queried at runtime.
    private object? captureSource;
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
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct)
    {
        var devicePath = (string)this.Identity;

        this.transcodeFormat = transcodeFormat;
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
                            pin.EnumerateFormats(captureSource).
                            SelectMany(format => format.EnumerateVideoCharacteristics().
                                Collect(vfc =>
                                    characteristics.Equals(vfc.Value) ?
                                        new { pin, format, interval = vfc.Key, vfc = vfc.Value } : null))).
                        FirstOrDefault() is { } entry)
                    {
                        this.Characteristics = entry.vfc;
                        entry.pin.SetFormat(entry.format, entry.interval);
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

                    this.sampleGrabberSink =
                        new SampleGrabberSink(this, frameProcessor);
                    if (sampleGrabber.SetCallback(this.sampleGrabberSink, 1) < 0)
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't get grabbing media type: DevicePath={devicePath}");
                    }

                    // Camera controls are queried from the capture source.
                    this.captureSource = captureSource;
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
                this.captureSource = null;
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

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    protected override void OnCapture(
        IntPtr pData, int size,
        long timestampMicroseconds, long frameIndex,
        PixelBuffer buffer) =>
        buffer.CopyIn(this.pBih, pData, size, timestampMicroseconds, frameIndex, this.transcodeFormat);

    /////////////////////////////////////////////////////////////////////////////////////////////////

    // Property page implementation.
    // https://learn.microsoft.com/en-us/windows/win32/directshow/displaying-a-filters-property-pages

    public override bool HasPropertyPage => true;

    protected override Task<bool> OnShowPropertyPageAsync(
        IntPtr parentWindow, CancellationToken ct) =>
        this.workingContext!.InvokeAsync(() =>
        {
            var devicePath = (string)this.Identity;

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
                if (captureSource is NativeMethods_DirectShow.ISpecifyPropertyPages specifyPropertyPages &&
                    captureSource is object sourceAsObject &&
                    specifyPropertyPages.GetPages(out var pPages) == 0)
                {
                    try
                    {
                        NativeMethods_DirectShow.OleCreatePropertyFrame(
                            parentWindow, 0, 0, this.Name, 1, ref sourceAsObject,
                            pPages.cElems, pPages.pElems, 0, 0, IntPtr.Zero);

                        return true;
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(pPages.pElems);
                    }
                }
            }

            return false;
        }, ct);

    /////////////////////////////////////////////////////////////////////////////////////////////////

    // Camera controls implementation.
    // https://learn.microsoft.com/en-us/windows/win32/directshow/configuring-the-video-quality

    private static readonly Dictionary<CaptureDeviceControlTypes, NativeMethods_DirectShow.VideoProcAmpProperty> videoProcAmpProperties = new()
    {
        { CaptureDeviceControlTypes.Brightness, NativeMethods_DirectShow.VideoProcAmpProperty.Brightness },
        { CaptureDeviceControlTypes.Contrast, NativeMethods_DirectShow.VideoProcAmpProperty.Contrast },
        { CaptureDeviceControlTypes.Hue, NativeMethods_DirectShow.VideoProcAmpProperty.Hue },
        { CaptureDeviceControlTypes.Saturation, NativeMethods_DirectShow.VideoProcAmpProperty.Saturation },
        { CaptureDeviceControlTypes.Sharpness, NativeMethods_DirectShow.VideoProcAmpProperty.Sharpness },
        { CaptureDeviceControlTypes.Gamma, NativeMethods_DirectShow.VideoProcAmpProperty.Gamma },
        { CaptureDeviceControlTypes.WhiteBalance, NativeMethods_DirectShow.VideoProcAmpProperty.WhiteBalance },
        { CaptureDeviceControlTypes.BacklightCompensation, NativeMethods_DirectShow.VideoProcAmpProperty.BacklightCompensation },
        { CaptureDeviceControlTypes.Gain, NativeMethods_DirectShow.VideoProcAmpProperty.Gain },
    };

    private static readonly Dictionary<CaptureDeviceControlTypes, NativeMethods_DirectShow.CameraControlProperty> cameraControlProperties = new()
    {
        { CaptureDeviceControlTypes.Pan, NativeMethods_DirectShow.CameraControlProperty.Pan },
        { CaptureDeviceControlTypes.Tilt, NativeMethods_DirectShow.CameraControlProperty.Tilt },
        { CaptureDeviceControlTypes.Zoom, NativeMethods_DirectShow.CameraControlProperty.Zoom },
        { CaptureDeviceControlTypes.Exposure, NativeMethods_DirectShow.CameraControlProperty.Exposure },
        { CaptureDeviceControlTypes.Focus, NativeMethods_DirectShow.CameraControlProperty.Focus },
    };

    protected override Task<CaptureDeviceControl[]> OnGetControlsAsync(
        CancellationToken ct) =>
        this.workingContext!.InvokeAsync(() =>
        {
            var controls = new List<CaptureDeviceControl>();
            foreach (var type in videoProcAmpProperties.Keys.Concat(cameraControlProperties.Keys))
            {
                if (this.GetControlRange(type, out var min, out var max, out var step, out var defaultValue, out var caps) == 0 &&
                    max >= min)
                {
                    var supportsAuto =
                        (caps & NativeMethods_DirectShow.ControlFlags.Auto) != 0 &&
                        (caps & NativeMethods_DirectShow.ControlFlags.Manual) != 0;
                    controls.Add(new CaptureDeviceControl(type, min, max, step, defaultValue, supportsAuto));
                }
            }
            return controls.ToArray();
        }, ct);

    protected override Task<CaptureDeviceControlValue?> OnGetControlValueAsync(
        CaptureDeviceControlTypes type, CancellationToken ct) =>
        this.workingContext!.InvokeAsync(() =>
            this.GetControl(type, out var value, out var flags) == 0 ?
                new CaptureDeviceControlValue(value, (flags & NativeMethods_DirectShow.ControlFlags.Auto) != 0) :
                null, ct);

    protected override Task<bool> OnSetControlValueAsync(
        CaptureDeviceControlTypes type, int value, bool isAuto, CancellationToken ct) =>
        this.workingContext!.InvokeAsync(() =>
        {
            if (isAuto)
            {
                // The value is kept, the device changes it itself.
                if (this.GetControl(type, out value, out _) != 0)
                {
                    return false;
                }
            }
            return this.SetControl(
                type, value,
                isAuto ? NativeMethods_DirectShow.ControlFlags.Auto : NativeMethods_DirectShow.ControlFlags.Manual) == 0;
        }, ct);

    private int GetControlRange(
        CaptureDeviceControlTypes type,
        out int min, out int max, out int step, out int defaultValue,
        out NativeMethods_DirectShow.ControlFlags caps)
    {
        if (videoProcAmpProperties.TryGetValue(type, out var videoProcAmpProperty) &&
            this.captureSource is NativeMethods_DirectShow.IAMVideoProcAmp videoProcAmp)
        {
            return videoProcAmp.GetRange(videoProcAmpProperty, out min, out max, out step, out defaultValue, out caps);
        }
        if (cameraControlProperties.TryGetValue(type, out var cameraControlProperty) &&
            this.captureSource is NativeMethods_DirectShow.IAMCameraControl cameraControl)
        {
            return cameraControl.GetRange(cameraControlProperty, out min, out max, out step, out defaultValue, out caps);
        }

        min = max = step = defaultValue = 0;
        caps = 0;
        return E_NOTIMPL;
    }

    private int GetControl(
        CaptureDeviceControlTypes type, out int value, out NativeMethods_DirectShow.ControlFlags flags)
    {
        if (videoProcAmpProperties.TryGetValue(type, out var videoProcAmpProperty) &&
            this.captureSource is NativeMethods_DirectShow.IAMVideoProcAmp videoProcAmp)
        {
            return videoProcAmp.Get(videoProcAmpProperty, out value, out flags);
        }
        if (cameraControlProperties.TryGetValue(type, out var cameraControlProperty) &&
            this.captureSource is NativeMethods_DirectShow.IAMCameraControl cameraControl)
        {
            return cameraControl.Get(cameraControlProperty, out value, out flags);
        }

        value = 0;
        flags = 0;
        return E_NOTIMPL;
    }

    private int SetControl(
        CaptureDeviceControlTypes type, int value, NativeMethods_DirectShow.ControlFlags flags)
    {
        if (videoProcAmpProperties.TryGetValue(type, out var videoProcAmpProperty) &&
            this.captureSource is NativeMethods_DirectShow.IAMVideoProcAmp videoProcAmp)
        {
            return videoProcAmp.Set(videoProcAmpProperty, value, flags);
        }
        if (cameraControlProperties.TryGetValue(type, out var cameraControlProperty) &&
            this.captureSource is NativeMethods_DirectShow.IAMCameraControl cameraControl)
        {
            return cameraControl.Set(cameraControlProperty, value, flags);
        }

        return E_NOTIMPL;
    }

    private const int E_NOTIMPL = unchecked((int)0x80004001);
}
