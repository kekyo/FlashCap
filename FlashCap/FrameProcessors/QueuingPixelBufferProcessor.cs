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
    internal abstract class QueuingPixelBufferProcessor :
        FrameProcessor
    {
        private readonly Stack<PixelBuffer> reserver = new();
        private readonly Queue<PixelBuffer> queue = new();
        private readonly ManualResetEventSlim arrived = new(false);
        private readonly Thread thread;

        protected QueuingPixelBufferProcessor()
        {
            this.thread = new Thread(this.ThreadEntry);
            this.thread.Start();
        }

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

            lock (this.queue)
            {
                this.queue.Enqueue(buffer);
                if (this.queue.Count == 1)
                {
                    this.arrived.Set();
                }
            }
        }

        protected PixelBuffer? Dequeue()
        {
            lock (this.queue)
            {
                if (this.queue.Count >= 1)
                {
                    return this.queue.Dequeue();
                }
                else
                {
                    this.arrived.Reset();
                }
            }

            while (true)
            {
                // TODO: abort
                this.arrived.Wait();

                lock (this.queue)
                {
                    if (this.queue.Count >= 1)
                    {
                        return this.queue.Dequeue();
                    }
                    else
                    {
                        // Sprious
                        this.arrived.Reset();
                    }
                }
            }
        }

        protected void Reserve(PixelBuffer buffer)
        {
            lock (this.reserver)
            {
                this.reserver.Push(buffer);
            }
        }

        protected abstract void ThreadEntry();
    }

    internal sealed class DelegatedQueuingPixelBufferProcessor :
        QueuingPixelBufferProcessor
    {
        private readonly PixelBufferArrivedDelegate pixelBufferArrived;

        public DelegatedQueuingPixelBufferProcessor(
            PixelBufferArrivedDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override void ThreadEntry()
        {
            while (true)
            {
                if (this.Dequeue() is { } buffer)
                {
                    try
                    {
                        try
                        {
                            this.pixelBufferArrived(buffer);
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
                else
                {
                    break;
                }
            }
        }
    }

#if NET40_OR_GREATER || NETSTANDARD || NETCOREAPP
    internal sealed class DelegatedQueuingPixelBufferTaskProcessor :
        QueuingPixelBufferProcessor
    {
        private readonly PixelBufferArrivedTaskDelegate pixelBufferArrived;

        public DelegatedQueuingPixelBufferTaskProcessor(
            PixelBufferArrivedTaskDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override void ThreadEntry()
        {
            void RecursiveLoop()
            {
                if (this.Dequeue() is { } buffer)
                {
                    this.pixelBufferArrived(buffer).
                        ContinueWith(task =>
                        {
                            this.Reserve(buffer);
                            if (task.IsCanceled || task.IsFaulted)
                            {
                                Trace.WriteLine(task.Exception);
                            }
                            RecursiveLoop();
                        });
                }
            }
            RecursiveLoop();
        }
    }

#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
    internal sealed class DelegatedQueuingPixelBufferValueTaskProcessor :
        QueuingPixelBufferProcessor
    {
        private readonly PixelBufferArrivedValueTaskDelegate pixelBufferArrived;

        public DelegatedQueuingPixelBufferValueTaskProcessor(
            PixelBufferArrivedValueTaskDelegate pixelBufferArrived) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override async void ThreadEntry()
        {
            while (true)
            {
                if (this.Dequeue() is { } buffer)
                {
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
                else
                {
                    break;
                }
            }
        }
    }
#endif
#endif
}
