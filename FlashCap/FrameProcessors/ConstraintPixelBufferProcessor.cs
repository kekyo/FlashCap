﻿////////////////////////////////////////////////////////////////////////////
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
    internal abstract class ConstraintPixelBufferProcessor : FrameProcessor
    {
        private readonly PixelBuffer buffer = new();
        private readonly WaitCallback pixelBufferArrivedEntry;
        private volatile int isin;

        protected ConstraintPixelBufferProcessor() =>
            this.pixelBufferArrivedEntry = this.PixelBufferArrivedEntry;

        public override sealed void OnFrameArrived(
            CaptureDevice captureDevice,
            IntPtr pData, int size, long timestampMicroseconds)
        {
            if (Interlocked.Increment(ref isin) == 1)
            {
                captureDevice.Capture(
                    pData, size, timestampMicroseconds, this.buffer);

                ThreadPool.QueueUserWorkItem(
                    this.pixelBufferArrivedEntry, this.buffer);
            }
            else
            {
                Interlocked.Decrement(ref isin);
            }
        }

        protected void Finished() =>
            Interlocked.Decrement(ref isin);

        protected abstract void PixelBufferArrivedEntry(object? parameter);
    }

    internal sealed class DelegatedConstraintPixelBufferProcessor :
        ConstraintPixelBufferProcessor
    {
        private readonly PixelBufferArrivedDelegate pixelBufferArrived;

        public DelegatedConstraintPixelBufferProcessor(
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
                this.Finished();
            }
        }
    }

#if NET40_OR_GREATER || NETSTANDARD || NETCOREAPP
    internal sealed class DelegatedConstraintPixelBufferTaskProcessor :
        ConstraintPixelBufferProcessor
    {
        private readonly PixelBufferArrivedTaskDelegate pixelBufferArrived;

        public DelegatedConstraintPixelBufferTaskProcessor(
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override void PixelBufferArrivedEntry(object? parameter)
        {
            var buffer = (PixelBuffer)parameter!;
            this.pixelBufferArrived(buffer).
                ContinueWith(task =>
                {
                    this.Finished();
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Trace.WriteLine(task.Exception);
                    }
                });
        }
    }

#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
    internal sealed class DelegatedConstraintPixelBufferValueTaskProcessor :
        ConstraintPixelBufferProcessor
    {
        private readonly PixelBufferArrivedValueTaskDelegate pixelBufferArrived;

        public DelegatedConstraintPixelBufferValueTaskProcessor(
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
                    this.Finished();
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
