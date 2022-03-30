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
    public abstract class FrameProcessor : IDisposable
    {
        protected FrameProcessor()
        {
        }

        public virtual void Dispose()
        {
        }

        protected PixelBuffer GetPixelBuffer(CaptureDevice captureDevice) =>
            new PixelBuffer();    // TODO: zero-copy solution

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected void Capture(CaptureDevice captureDevice,
            IntPtr pData, int size,
            long timestampMicroseconds, long frameIndex,
            PixelBuffer buffer) =>
            captureDevice.InternalOnCapture(pData, size, timestampMicroseconds, frameIndex, buffer);

        public abstract void OnFrameArrived(
            CaptureDevice captureDevice,
            IntPtr pData, int size, long timestampMicroseconds, long frameIndex);
    }
}
