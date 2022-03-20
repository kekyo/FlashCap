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
using System.Runtime.InteropServices;
using FlashCap.Utilities;

namespace FlashCap.Devices
{
    public sealed class VideoForWindowsDevice : ICaptureDevice
    {
        private readonly int deviceIndex;
        private readonly bool transcodeIfYUV;

        private IndependentSingleApartmentContext? workingContext = new();
        private IntPtr handle;
        private GCHandle thisPin;
        private NativeMethods_VideoForWindows.CAPVIDEOCALLBACK? callback;
        private IntPtr pBih;
        private FrameArrivedEventArgs? e = new();

        internal unsafe VideoForWindowsDevice(
            int deviceIndex,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV)
        {
            this.deviceIndex = deviceIndex;
            this.transcodeIfYUV = transcodeIfYUV;

            this.workingContext!.Send(_ =>
            {
                this.handle = NativeMethods_VideoForWindows.CreateVideoSourceWindow(deviceIndex);

                NativeMethods_VideoForWindows.capDriverConnect(this.handle, this.deviceIndex);

                ///////////////////////////////////////

                NativeMethods_VideoForWindows.capSetPreviewScale(this.handle, false);
                NativeMethods_VideoForWindows.capSetPreviewFPS(this.handle, 15);
                NativeMethods_VideoForWindows.capSetOverlay(this.handle, true);

                ///////////////////////////////////////

                // At first set 5fps, because can't set both fps and video format atomicity.
                NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out var cp);
                cp.dwRequestMicroSecPerFrame = 1_000_000 / 5;   // 5fps
                NativeMethods_VideoForWindows.capCaptureSetSetup(handle, cp);
                NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out cp);

                var setFormatResult = false;
                var pih = NativeMethods.AllocateMemory((IntPtr)sizeof(NativeMethods.BITMAPINFOHEADER));
                try
                {
                    var pBih = (NativeMethods.BITMAPINFOHEADER*)pih.ToPointer();

                    pBih->biSize = sizeof(NativeMethods.BITMAPINFOHEADER);
                    pBih->biCompression = (NativeMethods.Compression)Enum.Parse(
                        typeof(NativeMethods.Compression), characteristics.RawPixelFormat);
                    pBih->biPlanes = 1;
                    pBih->biBitCount = characteristics.RGBBitsPerPixel;
                    pBih->biWidth = characteristics.Width;
                    pBih->biHeight = characteristics.Height;
                    pBih->biSizeImage = pBih->CalculateImageSize();

                    // Try to set video format.
                    setFormatResult = NativeMethods_VideoForWindows.capSetVideoFormat(handle, pih);

                    // Try to set fps, but VFW API may cause ignoring it silently...
                    cp.dwRequestMicroSecPerFrame = (int)(1_000_000 / characteristics.FramesPerSecond);
                    NativeMethods_VideoForWindows.capCaptureSetSetup(handle, cp);
                    NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out cp);
                }
                finally
                {
                    NativeMethods.FreeMemory(pih);
                }

                // Get final video format.
                NativeMethods_VideoForWindows.capGetVideoFormat(handle, out this.pBih);

                ///////////////////////////////////////

                if (NativeMethods.CreateVideoCharacteristics(
                    this.pBih, Fraction.Create(1_000_000, cp.dwRequestMicroSecPerFrame)) is { } vc)
                {
                    if (setFormatResult)
                    {
                        Debug.WriteLine($"FlashCap: Characteristics={vc}");
                    }
                    else
                    {
                        Trace.WriteLine($"FlashCap: Couldn't set video format, Requested={characteristics}, Actual={vc}");
                    }

                    this.Characteristics = vc;
                }
                else
                {
                    throw new InvalidOperationException("Couldn't set bitmap format to VFW device.");
                }

                ///////////////////////////////////////

                // https://stackoverflow.com/questions/4097235/is-it-necessary-to-gchandle-alloc-each-callback-in-a-class
                this.thisPin = GCHandle.Alloc(this, GCHandleType.Normal);
                this.callback = this.CallbackEntry;

                NativeMethods_VideoForWindows.capSetCallbackFrame(this.handle, this.callback);
            }, null);
        }

        ~VideoForWindowsDevice() =>
            this.Dispose();

        public void Dispose()
        {
            if (this.handle != IntPtr.Zero)
            {
                this.workingContext!.Send(_ =>
                {
                    this.Stop();
                    NativeMethods_VideoForWindows.capSetCallbackFrame(this.handle, null);
                    NativeMethods_VideoForWindows.capDriverDisconnect(this.handle, this.deviceIndex);
                    NativeMethods_VideoForWindows.DestroyWindow(this.handle);
                    this.handle = IntPtr.Zero;
                    this.thisPin.Free();
                    this.callback = null;
                    NativeMethods.FreeMemory(this.pBih);
                    this.pBih = IntPtr.Zero;
                    this.FrameArrived = null;
                    this.e = null;
                }, null);

                this.workingContext.Dispose();
                this.workingContext = null;
            }
        }

        public VideoCharacteristics Characteristics { get; private set; } = null!;

        public event EventHandler<FrameArrivedEventArgs>? FrameArrived;

        private void CallbackEntry(IntPtr hWnd, in NativeMethods_VideoForWindows.VIDEOHDR hdr)
        {
            if (this.FrameArrived is { } fa)
            {
                // HACK: Dodge stupid camera devices...
                if (hdr.dwBytesUsed >= 64)
                {
                    try
                    {
                        // TODO: dwTimeCaptured always zero??
                        e!.Update(hdr.lpData, hdr.dwBytesUsed, hdr.dwTimeCaptured);
                        fa(this, e);
                    }
                    // DANGER: Stop leaking exception around outside of unmanaged area...
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                }
            }
        }

        public void Start() =>
            this.workingContext!.Send(_ =>
                NativeMethods_VideoForWindows.capShowPreview(this.handle, true),
                null);

        public void Stop() =>
            this.workingContext!.Send(_ =>
                NativeMethods_VideoForWindows.capShowPreview(this.handle, false),
                null);

        public void Capture(FrameArrivedEventArgs e, PixelBuffer buffer) =>
            buffer.CopyIn(
                this.pBih, e.pData, e.size,
                e.timestampMilliseconds, this.transcodeIfYUV);
    }
}
