////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Synchronized;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FlashCap.FrameProcessors
{
    internal abstract class ScatteringProcessor :
        FrameProcessor
    {
        private readonly Stack<PixelBuffer> reserver = new();
        private readonly WaitCallback pixelBufferArrivedEntry;

        protected ScatteringProcessor() =>
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

            this.Capture(
                captureDevice,
                pData, size, timestampMicroseconds,
                buffer);

            ThreadPool.QueueUserWorkItem(
                this.pixelBufferArrivedEntry, buffer);
        }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected void Reserve(PixelBuffer buffer)
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
            var buffer = (PixelBuffer)parameter!;
            try
            {
                try
                {
                    await this.pixelBufferArrived(buffer).ConfigureAwait(false);
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

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    internal sealed class DelegatedScatteringValueTaskProcessor :
        ScatteringProcessor
    {
        private readonly PixelBufferArrivedValueTaskDelegate pixelBufferArrived;

        public DelegatedScatteringValueTaskProcessor(
            PixelBufferArrivedValueTaskDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override async void PixelBufferArrivedEntry(object? parameter)
        {
            var buffer = (PixelBuffer)parameter!;
            try
            {
                try
                {
                    await this.pixelBufferArrived(buffer).ConfigureAwait(false);
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
