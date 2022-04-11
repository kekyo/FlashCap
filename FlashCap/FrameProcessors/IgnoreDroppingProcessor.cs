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

        protected volatile int processing = 1;
        protected volatile bool aborting;
        protected ManualResetEventSlim final = new(false);

        protected IgnoreDroppingProcessor() =>
            this.pixelBufferArrivedEntry = this.PixelBufferArrivedEntry;

        public override void Dispose()
        {
            if (this.final != null)
            {
                Debug.Assert(!this.aborting);

                this.aborting = true;
                if (Interlocked.Decrement(ref this.processing) >= 1)
                {
                    // HACK: Avoid deadlocking when arrived event handlers stuck in disposing process.
                    this.final.Wait(TimeSpan.FromSeconds(2));
                }
                this.final.Dispose();
                this.final = null!;
            }
        }

        public override sealed void OnFrameArrived(
            CaptureDevice captureDevice,
            IntPtr pData, int size,
            long timestampMicroseconds, long frameIndex)
        {
            if (this.aborting)
            {
                return;
            }

            if (Interlocked.Increment(ref this.isin) == 1)
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

                Interlocked.Increment(ref this.processing);
                ThreadPool.QueueUserWorkItem(
                    this.pixelBufferArrivedEntry, this.buffer);
            }
            else
            {
                Interlocked.Decrement(ref this.isin);
            }
        }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void ReleaseNow(PixelBuffer buffer) =>
            Interlocked.Decrement(ref this.isin);

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
            try
            {
                var buffer = (PixelBuffer)parameter!;
                using var scope = new InternalPixelBufferScope(this, buffer);
                this.pixelBufferArrived(scope);
            }
            finally
            {
                if (Interlocked.Decrement(ref base.processing) <= 0)
                {
                    Debug.Assert(base.final != null);
                    base.final!.Set();
                }
            }
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
            try
            {
                var buffer = (PixelBuffer)parameter!;
                using var scope = new InternalPixelBufferScope(this, buffer);
                await this.pixelBufferArrived(scope).
                    ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                if (Interlocked.Decrement(ref base.processing) <= 0)
                {
                    Debug.Assert(base.final != null);
                    base.final!.Set();
                }
            }
        }
    }
#endif
}
