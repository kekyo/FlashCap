////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FlashCap
{
    public abstract class FrameProcessor : IDisposable
    {
        private readonly Stack<PixelBuffer> reserver = new();

        protected FrameProcessor()
        {
        }

        ~FrameProcessor() =>
            this.Dispose(false);

        public void Dispose()
        {
            try
            {
                this.Dispose(true);
            }
            finally
            {
                lock (this.reserver)
                {
                    this.reserver.Clear();
                }
            }
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected PixelBuffer GetPixelBuffer()
        {
            PixelBuffer? buffer = null;
            lock (this.reserver)
            {
                if (this.reserver.Count >= 1)
                {
                    buffer = this.reserver.Pop();
                }
            }
            if (buffer == null)
            {
                buffer = new PixelBuffer();
            }
            return buffer;
        }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void ReleasePixelBuffer(PixelBuffer buffer)
        {
            lock (this.reserver)
            {
                this.reserver.Push(buffer);
            }
        }

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

        protected sealed class AutoPixelBufferScope :
            PixelBufferScope, IDisposable
        {
            private FrameProcessor? parent;

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public AutoPixelBufferScope(
                FrameProcessor parent,
                PixelBuffer buffer) :
                base(buffer) =>
                this.parent = parent;

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Dispose()
            {
                lock (this)
                {
                    if (this.parent is { } parent)
                    {
                        base.ReleaseNow();
                        this.parent.ReleasePixelBuffer(this.Buffer);
                        this.parent = null;
                    }
                }
            }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public override void ReleaseNow() =>
                this.Dispose();
        }
    }
}
