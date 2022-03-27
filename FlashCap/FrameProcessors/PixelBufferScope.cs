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

namespace FlashCap.FrameProcessors
{
    internal abstract class InternalFrameProcessor : FrameProcessor
    {
        protected InternalFrameProcessor()
        {
        }

        public abstract void ReleaseNow(PixelBuffer buffer);
    }

    internal sealed class InternalPixelBufferScope :
        PixelBufferScope, IDisposable
    {
        private InternalFrameProcessor? parent;

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public InternalPixelBufferScope(
            InternalFrameProcessor parent,
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
                    parent.ReleaseNow(this.Buffer);
                    base.ReleaseNow();
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
