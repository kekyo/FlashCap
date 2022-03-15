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

namespace FlashCap.Devices
{
    public sealed class VideoForWindowsDevice : ICaptureDevice
    {
        private readonly int deviceIndex;
        private readonly bool transcodeIfYUV;

        private IntPtr handle;
        private GCHandle thisPin;
        private NativeMethods_VideoForWindows.CAPVIDEOCALLBACK? callback;
        private IntPtr pBih;

        internal unsafe VideoForWindowsDevice(
            int deviceIndex,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV)
        {
            this.deviceIndex = deviceIndex;
            this.transcodeIfYUV = transcodeIfYUV;

            this.handle = NativeMethods_VideoForWindows.CreateVideoSourceWindow(deviceIndex);

            NativeMethods_VideoForWindows.capDriverConnect(this.handle, this.deviceIndex);

            // Try to set fps, but VFW API may cause ignoring it silently...
            NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out var cp);
            cp.dwRequestMicroSecPerFrame = (int)(1_000_000_000.0 / characteristics.FramesPer1000Second);
            NativeMethods_VideoForWindows.capCaptureSetSetup(handle, cp);
            NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out cp);

            var pih = Marshal.AllocCoTaskMem(sizeof(NativeMethods.RAW_BITMAPINFOHEADER));
            try
            {
                var pBih = (NativeMethods.RAW_BITMAPINFOHEADER*)pih.ToPointer();

                pBih->biSize = sizeof(NativeMethods.RAW_BITMAPINFOHEADER);
                pBih->biCompression = characteristics.PixelFormat;
                pBih->biPlanes = 1;
                pBih->biBitCount = (short)characteristics.BitsPerPixel;
                pBih->biWidth = characteristics.Width;
                pBih->biHeight = characteristics.Height;
                pBih->biSizeImage = pBih->CalculateImageSize();

                NativeMethods_VideoForWindows.capSetVideoFormat(handle, pih);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pih);
            }

            NativeMethods_VideoForWindows.capGetVideoFormat(handle, out this.pBih);

            if (NativeMethods.CreateVideoCharacteristics(
                this.pBih, (int)(1_000_000_000.0 / cp.dwRequestMicroSecPerFrame)) is { } vc)
            {
                this.Characteristics = vc;
            }
            else
            {
                throw new InvalidOperationException("Couldn't set bitmap format to VFW device.");
            }

            // https://stackoverflow.com/questions/4097235/is-it-necessary-to-gchandle-alloc-each-callback-in-a-class
            this.thisPin = GCHandle.Alloc(this, GCHandleType.Normal);
            this.callback = this.CallbackEntry;

            NativeMethods_VideoForWindows.capSetCallbackFrame(this.handle, this.callback);
        }

        ~VideoForWindowsDevice() =>
            this.Dispose();

        public void Dispose()
        {
            if (this.handle != IntPtr.Zero)
            {
                this.Stop();
                NativeMethods_VideoForWindows.capSetCallbackFrame(this.handle, null);
                NativeMethods_VideoForWindows.capDriverDisconnect(this.handle, this.deviceIndex);
                NativeMethods_VideoForWindows.DestroyWindow(this.handle);
                this.handle = IntPtr.Zero;
                this.thisPin.Free();
                this.callback = null;
                Marshal.FreeCoTaskMem(this.pBih);
                this.pBih = IntPtr.Zero;
                this.FrameArrived = null;
            }
        }

        public VideoCharacteristics Characteristics { get; }

        public event EventHandler<FrameArrivedEventArgs>? FrameArrived;

        private void CallbackEntry(IntPtr hWnd, in NativeMethods_VideoForWindows.VIDEOHDR hdr)
        {
            if (this.FrameArrived is { } fa)
            {
                try
                {
                    // TODO: dwTimeCaptured always zero??
                    fa(this, new FrameArrivedEventArgs(
                        hdr.lpData, (int)hdr.dwBytesUsed, hdr.dwTimeCaptured));
                }
                // DANGER: Stop leaking exception around outside of unmanaged area...
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

        public void Start()
        {
            NativeMethods_VideoForWindows.capSetPreviewScale(this.handle, false);
            NativeMethods_VideoForWindows.capSetPreviewFPS(this.handle, 15);
            NativeMethods_VideoForWindows.capShowPreview(this.handle, true);
        }

        public void Stop() =>
            NativeMethods_VideoForWindows.capShowPreview(this.handle, false);

        public void Capture(FrameArrivedEventArgs e, PixelBuffer buffer) =>
            buffer.CopyIn(this.pBih, e.Data, e.Size, this.transcodeIfYUV);
    }
}
