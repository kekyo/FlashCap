////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

namespace FlashCap
{
    public abstract class CaptureDevice
    {
        protected CaptureDevice()
        {
        }

        ~CaptureDevice() =>
            this.Dispose(false);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        public VideoCharacteristics Characteristics { get; protected set; } = null!;
        public bool IsRunning { get; protected set; }

        public abstract void Start();
        public abstract void Stop();

        protected abstract void OnCapture(
            IntPtr pData, int size, double timestampMicroseconds, PixelBuffer buffer);

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal void InternalOnCapture(
            IntPtr pData, int size, double timestampMicroseconds, PixelBuffer buffer) =>
            this.OnCapture(pData, size, timestampMicroseconds, buffer);
    }
}
