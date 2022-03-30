////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FlashCap.FrameProcessors
{
    internal abstract class IgnoreDroppingProcessor : InternalFrameProcessor
    {
        private readonly WaitCallback pixelBufferArrivedEntry;
        private volatile PixelBuffer? buffer;
        private volatile int isin;

        protected IgnoreDroppingProcessor() =>
            this.pixelBufferArrivedEntry = this.PixelBufferArrivedEntry;

        public override sealed void OnFrameArrived(
            CaptureDevice captureDevice,
            IntPtr pData, int size,
            long timestampMicroseconds, long frameIndex)
        {
            if (Interlocked.Increment(ref isin) == 1)
            {
                if (this.buffer == null)
                {
                    var buffer = base.GetPixelBuffer(captureDevice);
                    Interlocked.CompareExchange(
                        ref this.buffer, buffer, null);
                }

                this.Capture(
                    captureDevice,
                    pData, size,
                    timestampMicroseconds, frameIndex,
                    this.buffer);

                ThreadPool.QueueUserWorkItem(
                    this.pixelBufferArrivedEntry, this.buffer);
            }
            else
            {
                Interlocked.Decrement(ref isin);
            }
        }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void ReleaseNow(PixelBuffer buffer) =>
            Interlocked.Decrement(ref isin);

        protected abstract void PixelBufferArrivedEntry(object? parameter);
    }

    internal sealed class DelegatedIgnoreDroppingProcessor :
        IgnoreDroppingProcessor
    {
        private readonly PixelBufferArrivedDelegate pixelBufferArrived;

        public DelegatedIgnoreDroppingProcessor(
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override void PixelBufferArrivedEntry(object? parameter)
        {
            var buffer = (PixelBuffer)parameter!;
            using var scope = new InternalPixelBufferScope(this, buffer);
            this.pixelBufferArrived(scope);
        }
    }

#if NET35_OR_GREATER || NETSTANDARD || NETCOREAPP
    internal sealed class DelegatedIgnoreDroppingTaskProcessor :
        IgnoreDroppingProcessor
    {
        private readonly PixelBufferArrivedTaskDelegate pixelBufferArrived;

        public DelegatedIgnoreDroppingTaskProcessor(
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override async void PixelBufferArrivedEntry(object? parameter)
        {
            var buffer = (PixelBuffer)parameter!;
            try
            {
                using var scope = new InternalPixelBufferScope(this, buffer);
                await this.pixelBufferArrived(scope).
                    ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
#endif
}
