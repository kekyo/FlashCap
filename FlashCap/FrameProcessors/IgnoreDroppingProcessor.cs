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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FlashCap.FrameProcessors
{
    internal abstract class IgnoreDroppingProcessor : FrameProcessor
    {
        private readonly PixelBuffer buffer = new();
        private readonly WaitCallback pixelBufferArrivedEntry;
        private volatile int isin;

        protected IgnoreDroppingProcessor() =>
            this.pixelBufferArrivedEntry = this.PixelBufferArrivedEntry;

        public override void Dispose()
        {
        }

        public override sealed void OnFrameArrived(
            CaptureDevice captureDevice,
            IntPtr pData, int size, long timestampMicroseconds)
        {
            if (Interlocked.Increment(ref isin) == 1)
            {
                this.Capture(
                    captureDevice,
                    pData, size, timestampMicroseconds,
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
        protected void Finished() =>
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

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    internal sealed class DelegatedIgnoreDroppingValueTaskProcessor :
        IgnoreDroppingProcessor
    {
        private readonly PixelBufferArrivedValueTaskDelegate pixelBufferArrived;

        public DelegatedIgnoreDroppingValueTaskProcessor(
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
