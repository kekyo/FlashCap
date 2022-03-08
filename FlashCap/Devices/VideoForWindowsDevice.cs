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
        private readonly NativeMethods.CAPVIDEOCALLBACK callback;
        private readonly NativeMethods.BITMAPINFOHEADER bih;
        private readonly bool holdRawData;

        internal VideoForWindowsDevice(IntPtr handle, int identity, bool holdRawData)
        {
            this.handle = handle;
            this.identity = identity;
            this.holdRawData = holdRawData;

            NativeMethods.capDriverConnect(this.handle, this.identity);

            // https://stackoverflow.com/questions/4097235/is-it-necessary-to-gchandle-alloc-each-callback-in-a-class
            this.pin = GCHandle.Alloc(this, GCHandleType.Normal);
            this.callback = this.CallbackEntry;

            NativeMethods.capSetCallbackFrame(this.handle, this.callback);
            NativeMethods.capGetVideoFormat(this.handle, ref this.bih);
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
                NativeMethods.capSetCallbackFrame(this.handle, null);
                NativeMethods.capDriverDisconnect(this.handle, this.identity);
                NativeMethods.DestroyWindow(this.handle);
                this.handle = IntPtr.Zero;
                this.pin.Free();
            }
        }

        public int Width =>
            this.bih.biWidth;
        public int Height =>
            this.bih.biHeight;
        public int BitsPerPixel =>
            this.bih.biBitCount;
        public PixelFormats PixelFormat =>
            (PixelFormats)this.bih.biCompression;

        public event EventHandler<FrameArrivedEventArgs>? FrameArrived;

        private void CallbackEntry(IntPtr hWnd, in NativeMethods.VIDEOHDR hdr) =>
            this.FrameArrived?.Invoke(
                this, new FrameArrivedEventArgs(
                    hdr.lpData,
                    (int)hdr.dwBytesUsed,
                    (int)hdr.dwTimeCaptured));

        public void Start()
        {
            NativeMethods.capSetPreviewScale(this.handle, false);    // TODO:
            NativeMethods.capSetPreviewFPS(this.handle, 60);
            NativeMethods.capShowPreview(this.handle, true);   // TODO:
            //NativeMethods.capGrabFrameNonStop(this.handle);
        }

        public void Stop() =>
            NativeMethods.capShowPreview(this.handle, false);   // TODO:

        public void Capture(FrameArrivedEventArgs e, PixelBuffer buffer) =>
            buffer.CopyIn(in this.bih, e.Data, e.Size, this.holdRawData);
    }
}
