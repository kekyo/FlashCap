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
    internal abstract class QueuingProcessor :
        FrameProcessor
    {
        private readonly Stack<PixelBuffer> reserver = new();
        private readonly Queue<PixelBuffer> queue = new();
        private ManualResetEventSlim arrived = new(false);
        private ManualResetEventSlim abort = new(false);
        private Thread thread;

        protected QueuingProcessor()
        {
            this.thread = new Thread(this.ThreadEntry);
            this.thread.Start();
        }

        public override void Dispose()
        {
            if (this.thread != null)
            {
                this.abort.Set();
                this.thread.Join();

                this.reserver.Clear();
                this.queue.Clear();

                this.abort.Dispose();
                this.abort = null!;
                this.arrived.Dispose();
                this.arrived = null!;
                this.thread = null!;
            }
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

            this.Capture(
                captureDevice,
                pData, size, timestampMicroseconds,
                buffer);

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

            var handles = new[] { this.abort.WaitHandle, this.arrived.WaitHandle };

            while (true)
            {
                var index = WaitHandle.WaitAny(handles);
                if (index == 0)
                {
                    return null;
                }

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

        protected abstract void ThreadEntry();
    }

    internal sealed class DelegatedQueuingProcessor :
        QueuingProcessor
    {
        private readonly PixelBufferArrivedDelegate pixelBufferArrived;

        public DelegatedQueuingProcessor(
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
    internal sealed class DelegatedQueuingTaskProcessor :
        QueuingProcessor
    {
        private readonly PixelBufferArrivedTaskDelegate pixelBufferArrived;

        public DelegatedQueuingTaskProcessor(
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
    internal sealed class DelegatedQueuingValueTaskProcessor :
        QueuingProcessor
    {
        private readonly PixelBufferArrivedValueTaskDelegate pixelBufferArrived;

        public DelegatedQueuingValueTaskProcessor(
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
