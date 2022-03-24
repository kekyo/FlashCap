////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.FrameProcessors;
using FlashCap.Internal;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FlashCap.Devices
{
    public sealed class VideoForWindowsDevice : CaptureDevice
    {
        private readonly int deviceIndex;
        private readonly bool transcodeIfYUV;
        private readonly FrameProcessor frameProcessor;

        private IndependentSingleApartmentContext? workingContext = new();
        private IntPtr handle;
        private GCHandle thisPin;
        private NativeMethods_VideoForWindows.CAPVIDEOCALLBACK? callback;
        private IntPtr pBih;

        internal unsafe VideoForWindowsDevice(
            int deviceIndex,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            FrameProcessor frameProcessor)
        {
            this.deviceIndex = deviceIndex;
            this.Characteristics = characteristics;
            this.transcodeIfYUV = transcodeIfYUV;
            this.frameProcessor = frameProcessor;

            if (!NativeMethods.GetCompressionAndBitCount(
                characteristics.PixelFormat, out var compression, out var bitCount))
            {
                throw new ArgumentException(
                    $"FlashCap: Couldn't set video format [1]: DeviceIndex={deviceIndex}");
            }

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
                if (!NativeMethods_VideoForWindows.capCaptureSetSetup(handle, cp))
                {
                    throw new ArgumentException(
                        $"FlashCap: Couldn't set video frame rate [1]: DeviceIndex={deviceIndex}");
                }
                NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out cp);

                var pih = NativeMethods.AllocateMemory((IntPtr)sizeof(NativeMethods.BITMAPINFOHEADER));
                try
                {
                    var pBih = (NativeMethods.BITMAPINFOHEADER*)pih.ToPointer();

                    pBih->biSize = sizeof(NativeMethods.BITMAPINFOHEADER);
                    pBih->biCompression = compression;
                    pBih->biPlanes = 1;
                    pBih->biBitCount = bitCount;
                    pBih->biWidth = characteristics.Width;
                    pBih->biHeight = characteristics.Height;
                    pBih->biSizeImage = pBih->CalculateImageSize();

                    // Try to set video format.
                    if (!NativeMethods_VideoForWindows.capSetVideoFormat(handle, pih))
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't set video format [2]: DeviceIndex={deviceIndex}");
                    }

                    // Try to set fps, but VFW API may cause ignoring it silently...
                    cp.dwRequestMicroSecPerFrame =
                        (int)(1_000_000 / characteristics.FramesPerSecond);
                    if (!NativeMethods_VideoForWindows.capCaptureSetSetup(handle, cp))
                    {
                        throw new ArgumentException(
                            $"FlashCap: Couldn't set video frame rate [2]: DeviceIndex={deviceIndex}");
                    }
                    NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out cp);
                }
                finally
                {
                    NativeMethods.FreeMemory(pih);
                }

                // Get final video format.
                NativeMethods_VideoForWindows.capGetVideoFormat(handle, out this.pBih);

                ///////////////////////////////////////

                // https://stackoverflow.com/questions/4097235/is-it-necessary-to-gchandle-alloc-each-callback-in-a-class
                this.thisPin = GCHandle.Alloc(this, GCHandleType.Normal);
                this.callback = this.CallbackEntry;

                NativeMethods_VideoForWindows.capSetCallbackFrame(this.handle, this.callback);
            }, null);
        }

        protected override void Dispose(bool disposing)
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
                }, null);

                this.workingContext.Dispose();
                this.workingContext = null;
            }
        }

        private void CallbackEntry(IntPtr hWnd, in NativeMethods_VideoForWindows.VIDEOHDR hdr)
        {
            // HACK: Dodge stupid camera devices...
            if (hdr.dwBytesUsed >= 64)
            {
                try
                {
                    // TODO: dwTimeCaptured always zero??
                    this.frameProcessor.OnFrameArrived(
                        this,
                        hdr.lpData, hdr.dwBytesUsed, hdr.dwTimeCaptured);
                }
                // DANGER: Stop leaking exception around outside of unmanaged area...
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

        public override void Start() =>
            this.workingContext!.Send(_ =>
            {
                NativeMethods_VideoForWindows.capShowPreview(this.handle, true);
                this.IsRunning = true;
            }, null);

        public override void Stop() =>
            this.workingContext!.Send(_ =>
            {
                this.IsRunning = false;
                NativeMethods_VideoForWindows.capShowPreview(this.handle, false);
            }, null);

        public override void Capture(
            IntPtr pData, int size, long timestampMilliseconds,
            PixelBuffer buffer) =>
            buffer.CopyIn(this.pBih, pData, size, timestampMilliseconds, this.transcodeIfYUV);
    }
}
