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
using System.Threading;

namespace FlashCap.FrameProcessors
{
    internal abstract class ScatteringPixelBufferProcessor :
        FrameProcessor
    {
        private readonly Stack<PixelBuffer> reserver = new();
        private readonly WaitCallback pixelBufferArrivedEntry;

        protected ScatteringPixelBufferProcessor() =>
            this.pixelBufferArrivedEntry = this.PixelBufferArrivedEntry;

        public override sealed void OnFrameArrived(
            CaptureDevice captureDevice,
            IntPtr pData, int size, long timestampMicroseconds)
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

            captureDevice.Capture(
                pData, size, timestampMicroseconds, buffer);

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

        protected abstract void PixelBufferArrivedEntry(object? parameter);
    }

    internal sealed class DelegatedScatteringPixelBufferProcessor :
        ScatteringPixelBufferProcessor
    {
        private readonly PixelBufferArrivedDelegate pixelBufferArrived;

        public DelegatedScatteringPixelBufferProcessor(
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override void PixelBufferArrivedEntry(object? parameter)
        {
            var buffer = (PixelBuffer)parameter!;
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

#if NET40_OR_GREATER || NETSTANDARD || NETCOREAPP
    internal sealed class DelegatedScatteringPixelBufferTaskProcessor :
        ScatteringPixelBufferProcessor
    {
        private readonly PixelBufferArrivedTaskDelegate pixelBufferArrived;

        public DelegatedScatteringPixelBufferTaskProcessor(
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override void PixelBufferArrivedEntry(object? parameter)
        {
            var buffer = (PixelBuffer)parameter!;
            this.pixelBufferArrived(buffer).
                ContinueWith(task =>
                {
                    this.Reserve(buffer);
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Trace.WriteLine(task.Exception);
                    }
                });
        }
    }

#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
    internal sealed class DelegatedScatteringPixelBufferValueTaskProcessor :
        ScatteringPixelBufferProcessor
    {
        private readonly PixelBufferArrivedValueTaskDelegate pixelBufferArrived;

        public DelegatedScatteringPixelBufferValueTaskProcessor(
            PixelBufferArrivedValueTaskDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override async void PixelBufferArrivedEntry(object? parameter)
        {
            var buffer = (PixelBuffer)parameter!;
            try
            {
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
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
#endif
#endif
}
