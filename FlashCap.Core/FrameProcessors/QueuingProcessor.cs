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

namespace FlashCap.FrameProcessors;

internal abstract class QueuingProcessor :
    FrameProcessor
{
    private readonly int maxQueuingFrames;
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

    protected override void Dispose(bool disposing)
    {
        if (disposing && this.thread != null)
        {
            this.aborting = true;
            this.abort.Set();

            // Force exhaust.
            lock (this.queue)
            {
                this.queue.Clear();
                this.arrived.Reset();
            }

            // HACK: Avoid deadlocking when arrived event handlers stuck in disposing process.
            this.thread.Join(TimeSpan.FromSeconds(2));
            this.thread = null!;
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

        var buffer = base.GetPixelBuffer();

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

    protected abstract void ThreadEntry();
}

internal sealed class DelegatedQueuingProcessor :
    QueuingProcessor
{
    private PixelBufferArrivedDelegate pixelBufferArrived;

    public DelegatedQueuingProcessor(
        PixelBufferArrivedDelegate pixelBufferArrived, int maxQueuingFrames) :
        base(maxQueuingFrames) =>
        this.pixelBufferArrived = pixelBufferArrived;

    protected override void ThreadEntry()
    {
        try
        {
            while (true)
            {
                if (this.Dequeue() is { } buffer)
                {
                    try
                    {
                        using var scope = new AutoPixelBufferScope(this, buffer);
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
        finally
        {
            this.pixelBufferArrived = null!;
        }
    }
}

internal sealed class DelegatedQueuingTaskProcessor :
    QueuingProcessor
{
    private PixelBufferArrivedTaskDelegate pixelBufferArrived;

    public DelegatedQueuingTaskProcessor(
        PixelBufferArrivedTaskDelegate pixelBufferArrived, int maxQueuingFrames) :
        base(maxQueuingFrames) =>
        this.pixelBufferArrived = pixelBufferArrived;

    protected override async void ThreadEntry()
    {
        try
        {
            while (true)
            {
                if (this.Dequeue() is { } buffer)
                {
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
                }
                else
                {
                    break;
                }
            }
        }
        finally
        {
            this.pixelBufferArrived = null!;
        }
    }
}
