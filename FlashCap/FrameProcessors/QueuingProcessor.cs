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
        InternalFrameProcessor
    {
        private readonly int maxQueuingFrames;
        private readonly Stack<PixelBuffer> reserver = new();
        private readonly Queue<PixelBuffer> queue = new();
        private ManualResetEventSlim arrived = new(false);
        private ManualResetEventSlim abort = new(false);
        private volatile bool aborting;
        private Thread thread;

        protected QueuingProcessor(int maxQueuingFrames)
        {
            this.maxQueuingFrames = maxQueuingFrames;
            this.thread = new Thread(this.ThreadEntry);
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        public override void Dispose()
        {
            if (this.thread != null)
            {
                this.aborting = true;
                this.abort.Set();

                // Force exhaust.
                lock (this.queue)
                {
                    while (this.queue.Count >= 1)
                    {
                        var buffer = this.queue.Dequeue();
                        this.ReleaseNow(buffer);
                    }
                    this.arrived.Reset();
                }

                // HACK: Avoid deadlocking when arrived event handlers stuck in disposing process.
                this.thread.Join(TimeSpan.FromSeconds(2));
                this.thread = null!;

                lock (this.reserver)
                {
                    this.reserver.Clear();
                }
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

            lock (this.queue)
            {
                if (this.queue.Count >= this.maxQueuingFrames)
                {
                    return;
                }
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
        public override void ReleaseNow(PixelBuffer buffer)
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
            PixelBufferArrivedDelegate pixelBufferArrived, int maxQueuingFrames) :
            base(maxQueuingFrames) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override void ThreadEntry()
        {
            while (true)
            {
                if (this.Dequeue() is { } buffer)
                {
                    try
                    {
                        using var scope = new InternalPixelBufferScope(this, buffer);
                        this.pixelBufferArrived(scope);
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

    internal sealed class DelegatedQueuingTaskProcessor :
        QueuingProcessor
    {
        private readonly PixelBufferArrivedTaskDelegate pixelBufferArrived;

        public DelegatedQueuingTaskProcessor(
            PixelBufferArrivedTaskDelegate pixelBufferArrived, int maxQueuingFrames) :
            base(maxQueuingFrames) =>
            this.pixelBufferArrived = pixelBufferArrived;

        protected override async void ThreadEntry()
        {
            while (true)
            {
                if (this.Dequeue() is { } buffer)
                {
                    try
                    {
                        using var scope = new InternalPixelBufferScope(this, buffer);
                        await this.pixelBufferArrived(scope).
                            ConfigureAwait(false);
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
}
