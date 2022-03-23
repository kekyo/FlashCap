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
using System.Threading;
using System.Threading.Tasks;

#if NET40 || NET45 || NET461 || NET48 || NETSTANDARD1_3 || NETSTANDARD2_0 || NETCOREAPP2_0
using ValueTask = System.Threading.Tasks.Task;
#endif

namespace FlashCap.FrameProcessors
{
    public delegate void PixelBufferArrivedDelegate(
        PixelBuffer buffer);
    public delegate Task PixelBufferArrivedAsyncDelegate(
        PixelBuffer buffer);

    internal abstract class ConstraintPixelBufferProcessor : FrameProcessor
    {
        private readonly PixelBuffer buffer = new();
        private readonly WaitCallback pixelBufferArrivedEntry;
        private volatile int isin;

        protected ConstraintPixelBufferProcessor() =>
            this.pixelBufferArrivedEntry = this.PixelBufferArrivedEntry;

        public override sealed void OnFrameArrived(
            ICaptureDevice captureDevice,
            IntPtr pData, int size, TimeSpan timestamp)
        {
            if (Interlocked.Increment(ref isin) == 1)
            {
                captureDevice.Capture(pData, size, timestamp, this.buffer);

                ThreadPool.QueueUserWorkItem(
                    this.pixelBufferArrivedEntry, buffer);
            }
            else
            {
                Interlocked.Decrement(ref isin);
            }
        }

        protected void Finished() =>
            Interlocked.Decrement(ref isin);

        protected abstract void PixelBufferArrivedEntry(object parameter);
    }

    internal sealed class ConstraintPixelBufferSyncProcessor : ConstraintPixelBufferProcessor
    {
        private readonly PixelBufferArrivedDelegate pixelBufferArrived;

        public ConstraintPixelBufferSyncProcessor(
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override void PixelBufferArrivedEntry(object parameter)
        {
            var buffer = (PixelBuffer)parameter;
            try
            {
                this.pixelBufferArrived(buffer);
            }
            finally
            {
                this.Finished();
            }
        }
    }

    internal sealed class ConstraintPixelBufferAsyncProcessor : ConstraintPixelBufferProcessor
    {
        private readonly PixelBufferArrivedAsyncDelegate pixelBufferArrived;

        public ConstraintPixelBufferAsyncProcessor(
            PixelBufferArrivedAsyncDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override async void PixelBufferArrivedEntry(object parameter)
        {
            var buffer = (PixelBuffer)parameter;
            try
            {
                await this.pixelBufferArrived(buffer).ConfigureAwait(false);
            }
            finally
            {
                this.Finished();
            }
        }
    }
}
