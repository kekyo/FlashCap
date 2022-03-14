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
using System.Runtime.InteropServices;

namespace FlashCap.Devices
{
    public sealed class VideoForWindowsDevice : ICaptureDevice
    {
        private IntPtr handle;
        private GCHandle pin;
        private readonly int identity;
        private readonly NativeMethods_VideoForWindows.CAPVIDEOCALLBACK callback;
        private readonly IntPtr pBih;  // RAW_BITMAPINFOHEADER*
        private readonly bool transcodeIfYUV;

        internal unsafe VideoForWindowsDevice(
            IntPtr handle, int identity,
            VideoCharacteristics characteristics,
            bool transcodeIfYUV)
        {
            this.handle = handle;
            this.identity = identity;
            this.transcodeIfYUV = transcodeIfYUV;

            NativeMethods_VideoForWindows.capDriverConnect(this.handle, this.identity);

            // https://stackoverflow.com/questions/4097235/is-it-necessary-to-gchandle-alloc-each-callback-in-a-class
            this.pin = GCHandle.Alloc(this, GCHandleType.Normal);
            this.callback = this.CallbackEntry;

            NativeMethods_VideoForWindows.capSetCallbackFrame(this.handle, this.callback);
            NativeMethods_VideoForWindows.capGetVideoFormat(this.handle, out var bih);

            this.pBih = (IntPtr)bih.ToNative();

            this.Characteristics = characteristics;
        }

        ~VideoForWindowsDevice()
        {
            if (this.pin.IsAllocated)
            {
                this.Dispose();
            }
        }

        public void Dispose()
        {
            if (this.handle != IntPtr.Zero)
            {
                this.Stop();
                NativeMethods_VideoForWindows.capSetCallbackFrame(this.handle, null);
                NativeMethods_VideoForWindows.capDriverDisconnect(this.handle, this.identity);
                NativeMethods_VideoForWindows.DestroyWindow(this.handle);
                this.handle = IntPtr.Zero;
                this.pin.Free();
                Marshal.FreeCoTaskMem(this.pBih);
                this.FrameArrived = null;
            }
        }

        public VideoCharacteristics Characteristics { get; }

        public event EventHandler<FrameArrivedEventArgs>? FrameArrived;

        private void CallbackEntry(IntPtr hWnd, in NativeMethods_VideoForWindows.VIDEOHDR hdr) =>
            this.FrameArrived?.Invoke(
                this, new FrameArrivedEventArgs(
                    hdr.lpData,
                    (int)hdr.dwBytesUsed,
                    TimeSpan.FromMilliseconds(hdr.dwTimeCaptured)));

        public void Start()
        {
            NativeMethods_VideoForWindows.capSetPreviewScale(this.handle, false);
            NativeMethods_VideoForWindows.capSetPreviewFPS(this.handle, 60);
            NativeMethods_VideoForWindows.capShowPreview(this.handle, true);
        }

        public void Stop() =>
            NativeMethods_VideoForWindows.capShowPreview(this.handle, false);

        public void Capture(FrameArrivedEventArgs e, PixelBuffer buffer) =>
            buffer.CopyIn(this.pBih, e.Data, e.Size, this.transcodeIfYUV);
    }
}
