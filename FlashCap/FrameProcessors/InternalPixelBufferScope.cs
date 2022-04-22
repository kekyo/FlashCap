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
    internal sealed class InternalPixelBufferScope :
        PixelBufferScope, IDisposable
    {
        private FrameProcessor? parent;

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public InternalPixelBufferScope(
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
