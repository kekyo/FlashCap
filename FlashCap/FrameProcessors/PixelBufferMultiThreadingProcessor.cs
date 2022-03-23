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

#if NET45 || NET461 || NET48 || NETSTANDARD1_3 || NETSTANDARD2_0 || NETCOREAPP2_0
using ValueTask = System.Threading.Tasks.Task;
#endif

namespace FlashCap.FrameProcessors
{
    internal abstract class PixelBufferMultiThreadingProcessor : FrameProcessor
    {
        private readonly Stack<PixelBuffer> reserver = new();
        private readonly WaitCallback pixelBufferArrivedEntry;

        protected PixelBufferMultiThreadingProcessor() =>
            this.pixelBufferArrivedEntry = this.PixelBufferArrivedEntry;

        public override sealed void OnFrameArrived(
            ICaptureDevice captureDevice,
            IntPtr pData, int size, TimeSpan timestamp)
        {
            PixelBuffer? buffer = null;
            lock (reserver)
            {
                if (reserver.Count >= 1)
                {
                    buffer = reserver.Pop();
                }
            }
            if (buffer == null)
            {
                buffer = new PixelBuffer();
            }

            captureDevice.Capture(pData, size, timestamp, buffer);

            ThreadPool.QueueUserWorkItem(
                this.pixelBufferArrivedEntry, buffer);
        }

        protected void Reserve(PixelBuffer buffer)
        {
            lock (this.reserver)
            {
                this.reserver.Push(buffer);
            }
        }

        protected abstract void PixelBufferArrivedEntry(object parameter);
    }

    internal sealed class DelegatedPixelBufferSyncProcessor : PixelBufferMultiThreadingProcessor
    {
        private readonly PixelBufferArrivedDelegate pixelBufferArrived;

        public DelegatedPixelBufferSyncProcessor(
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
                this.Reserve(buffer);
            }
        }
    }

    internal sealed class DelegatedPixelBufferAsyncProcessor : PixelBufferMultiThreadingProcessor
    {
        private readonly PixelBufferArrivedAsyncDelegate pixelBufferArrived;

        public DelegatedPixelBufferAsyncProcessor(
            PixelBufferArrivedAsyncDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override async void PixelBufferArrivedEntry(object parameter)
        {
            var buffer = (PixelBuffer)parameter;
            try
            {
                await this.pixelBufferArrived(buffer).
                    ConfigureAwait(false);
            }
            finally
            {
                this.Reserve(buffer);
            }
        }
    }
}
