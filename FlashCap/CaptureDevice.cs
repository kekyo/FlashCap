////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;

namespace FlashCap
{
    public abstract class CaptureDevice : ICaptureDevice
    {
        ~CaptureDevice() =>
            this.Dispose(false);

        public void Dispose() =>
            this.Dispose(true);

        protected abstract void Dispose(bool disposing);

        public VideoCharacteristics Characteristics { get; protected set; } = null!;
        public bool IsRunning { get; protected set; }

        public abstract void Start();
        public abstract void Stop();

        public abstract void Capture(
            IntPtr pData, int size, long timestampMicroseconds, PixelBuffer buffer);

        void ICaptureDevice.Capture(
            IntPtr pData, int size, long timestampMicroseconds, PixelBuffer buffer) =>
            this.Capture(pData, size, timestampMicroseconds, buffer);
    }
}
