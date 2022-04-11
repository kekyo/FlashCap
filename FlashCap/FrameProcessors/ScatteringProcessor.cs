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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FlashCap.FrameProcessors
{
    internal abstract class ScatteringProcessor :
        InternalFrameProcessor
    {
        private readonly Stack<PixelBuffer> reserver = new();
        private readonly WaitCallback pixelBufferArrivedEntry;

        protected volatile int processing = 1;
        protected volatile bool aborting;
        protected ManualResetEventSlim final = new(false);

        protected ScatteringProcessor() =>
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
                buffer = base.GetPixelBuffer(captureDevice);
            }

            this.Capture(
                captureDevice,
                pData, size,
                timestampMicroseconds, frameIndex,
                buffer);

            Interlocked.Increment(ref this.processing);
            ThreadPool.QueueUserWorkItem(
                this.pixelBufferArrivedEntry, buffer);
        }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void ReleaseNow(PixelBuffer buffer)
        {
            lock (this.reserver)
            {
                this.reserver.Push(buffer);
            }
        }

        protected abstract void PixelBufferArrivedEntry(object? parameter);
    }

    internal sealed class DelegatedScatteringProcessor :
        ScatteringProcessor
    {
        private readonly PixelBufferArrivedDelegate pixelBufferArrived;

        public DelegatedScatteringProcessor(
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override void PixelBufferArrivedEntry(object? parameter)
        {
            try
            {
                if (this.aborting)
                {
                    return;
                }

                var buffer = (PixelBuffer)parameter!;
                using var scope = new InternalPixelBufferScope(this, buffer);
                this.pixelBufferArrived(scope);
            }
            finally
            {
                if (Interlocked.Decrement(ref base.processing) <= 0)
                {
                    base.final?.Set();
                }
            }
        }
    }

#if NET35_OR_GREATER || NETSTANDARD || NETCOREAPP
    internal sealed class DelegatedScatteringTaskProcessor :
        ScatteringProcessor
    {
        private readonly PixelBufferArrivedTaskDelegate pixelBufferArrived;

        public DelegatedScatteringTaskProcessor(
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override async void PixelBufferArrivedEntry(object? parameter)
        {
            try
            {
                if (this.aborting)
                {
                    return;
                }

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
                    base.final?.Set();
                }
            }
        }
    }
#endif
}
