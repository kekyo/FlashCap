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
using System.Threading;

namespace FlashCap.FrameProcessors
{
    internal abstract class ScatteringProcessor :
        FrameProcessor
    {
        private readonly int maxQueuingFrames;
        private readonly WaitCallback pixelBufferArrivedEntry;

        protected volatile int processing = 1;
        protected volatile bool aborting;
        protected ManualResetEventSlim final = new(false);

        protected ScatteringProcessor(int maxQueuingFrames)
        {
            this.maxQueuingFrames = maxQueuingFrames;
            this.pixelBufferArrivedEntry = this.PixelBufferArrivedEntry;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.final != null)
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

            if (Interlocked.Increment(ref this.processing) <= this.maxQueuingFrames)
            {
                var buffer = base.GetPixelBuffer();

                this.Capture(
                    captureDevice,
                    pData, size,
                    timestampMicroseconds, frameIndex,
                    buffer);

                ThreadPool.QueueUserWorkItem(
                    this.pixelBufferArrivedEntry, buffer);
            }
            else
            {
                Interlocked.Decrement(ref this.processing);
            }
        }

        protected abstract void PixelBufferArrivedEntry(object? parameter);
    }

    internal sealed class DelegatedScatteringProcessor :
        ScatteringProcessor
    {
        private PixelBufferArrivedDelegate pixelBufferArrived;

        public DelegatedScatteringProcessor(
            PixelBufferArrivedDelegate pixelBufferArrived, int maxQueuingFrames) :
            base(maxQueuingFrames) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.pixelBufferArrived = null!;
        }

        protected override void PixelBufferArrivedEntry(object? parameter)
        {
            var buffer = (PixelBuffer)parameter!;

            if (this.aborting)
            {
                if (Interlocked.Decrement(ref base.processing) <= 0)
                {
                    base.final?.Set();
                }
                return;
            }

            try
            {
                using var scope = new AutoPixelBufferScope(this, buffer);
                this.pixelBufferArrived(scope);
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

    internal sealed class DelegatedScatteringTaskProcessor :
        ScatteringProcessor
    {
        private PixelBufferArrivedTaskDelegate pixelBufferArrived;

        public DelegatedScatteringTaskProcessor(
            PixelBufferArrivedTaskDelegate pixelBufferArrived, int maxQueuingFrames) :
            base(maxQueuingFrames) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.pixelBufferArrived = null!;
        }

        protected override async void PixelBufferArrivedEntry(object? parameter)
        {
            var buffer = (PixelBuffer)parameter!;

            if (this.aborting)
            {
                if (Interlocked.Decrement(ref base.processing) <= 0)
                {
                    base.final?.Set();
                }
                return;
            }

            try
            {
                using var scope = new AutoPixelBufferScope(this, buffer);
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

    internal sealed class DelegatedScatteringObservableProcessor :
        ScatteringProcessor
    {
        private IObserver<PixelBufferScope> observer;

        public DelegatedScatteringObservableProcessor(
            IObserver<PixelBufferScope> observer, int maxQueuingFrames) :
            base(maxQueuingFrames) =>
            this.observer = observer;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.observer != null)
            {
                this.observer.OnCompleted();
                this.observer = null!;
            }
        }

        protected override void PixelBufferArrivedEntry(object? parameter)
        {
            var buffer = (PixelBuffer)parameter!;

            if (this.aborting)
            {
                if (Interlocked.Decrement(ref base.processing) <= 0)
                {
                    base.final?.Set();
                }
                return;
            }

            try
            {
                using var scope = new AutoPixelBufferScope(this, buffer);
                this.observer.OnNext(scope);
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
}
