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
using System.IO;
using System.Runtime.InteropServices;

namespace FlashCap
{
    public sealed class CapturedEventArgs : EventArgs
    {
        private readonly IntPtr pData;
        private byte[]? allocatedData;

        public readonly int Size;
        public readonly int Timestamp;

        public CapturedEventArgs(IntPtr pData, int size, int timestamp)
        {
            this.pData = pData;
            this.Size = size;
            this.Timestamp = timestamp;
        }

        public void ExtractWithAllocatedBuffer(byte[] rawBitmapData) =>
            Marshal.Copy(pData, rawBitmapData, 0, rawBitmapData.Length);

        public byte[] ExtractBuffer()
        {
            if (this.allocatedData == null)
            {
                var allocatedData = new byte[this.Size];
                Marshal.Copy(pData, allocatedData, 0, allocatedData.Length);
                this.allocatedData = allocatedData;
            }
            return this.allocatedData;
        }

        public Stream ExtractStream() =>
            new MemoryStream(this.ExtractBuffer());
    }

    public delegate void CapturedEventHandler(object sender, CapturedEventArgs e);

    public interface ICaptureDevice : IDisposable
    {
        event CapturedEventHandler? Captured;

        void Start();

        void Stop();
    }

    public sealed class CaptureDevice : IDisposable
    {
        private IntPtr handle;
        private readonly int deviceId;
        private GCHandle pin;

        internal CaptureDevice(IntPtr handle, int deviceId)
        {
            this.handle = handle;
            this.deviceId = deviceId;

            NativeMethods.capDriverConnect(this.handle, this.deviceId);

            // https://stackoverflow.com/questions/4097235/is-it-necessary-to-gchandle-alloc-each-callback-in-a-class
            this.pin = GCHandle.Alloc(this, GCHandleType.Normal);

            NativeMethods.capSetCallbackFrame(this.handle, this.CallbackEntry);
        }

        ~CaptureDevice()
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
                NativeMethods.DestroyWindow(this.handle);
                this.handle = IntPtr.Zero;
                this.pin.Free();
            }
        }

        public event CapturedEventHandler? Captured;

        private void CallbackEntry(IntPtr hWnd, ref NativeMethods.VIDEOHDR hdr) =>
            this.Captured?.Invoke(
                this, new CapturedEventArgs(
                    hdr.lpData, (int)hdr.dwBytesUsed, (int)hdr.dwTimeCaptured));

        public void Start()
        {
            NativeMethods.capSetPreviewScale(this.handle, true);    // TODO:
            NativeMethods.capSetPreviewFPS(this.handle, 60);
            NativeMethods.capShowPreview(this.handle, true);   // TODO:
        }

        public void Stop()
        {
            NativeMethods.capShowPreview(this.handle, false);   // TODO:
            NativeMethods.capDriverDisconnect(this.handle, this.deviceId);
        }
    }
}
