#if FLASHCAP_MEDIAFOUNDATION
////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
#if !NET9_0_OR_GREATER
using Lock = System.Object;
#endif

namespace FlashCap.Internal.MediaFoundation;

internal sealed class AsyncSourceReaderState
{
    private enum Lifecycle
    {
        Created,
        Running,
        StopRequested,
        Flushing,
        Draining,
        Completed,
    }

    private readonly Lock sync = new();
    private readonly Action requestSample;
    private readonly Action flush;
    private readonly TaskCompletionSource<bool> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> stopRequested = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private Exception? failure;
    private Exception? flushFailure;
    private int activeCallbacks;
    private Lifecycle lifecycle;

    internal AsyncSourceReaderState(Action requestSample, Action flush)
    {
        this.requestSample = requestSample;
        this.flush = flush;
    }

    internal Task Completion => this.completion.Task;
    internal Task StopRequested => this.stopRequested.Task;

    internal Exception? FlushFailure
    {
        get
        {
            lock (this.sync)
            {
                return this.flushFailure;
            }
        }
    }

    internal bool Start()
    {
        Exception? failure = null;
        lock (this.sync)
        {
            if (this.lifecycle is not Lifecycle.Created and not Lifecycle.Running)
            {
                return false;
            }
            if (this.lifecycle == Lifecycle.Running)
            {
                return true;
            }

            this.lifecycle = Lifecycle.Running;
            try
            {
                this.requestSample();
            }
            catch (Exception exception)
            {
                this.lifecycle = Lifecycle.Draining;
                this.failure = exception;
                failure = exception;
            }
        }

        if (failure is null)
        {
            return true;
        }

        this.TryComplete();
        ExceptionDispatchInfo.Capture(failure).Throw();
        return true;
    }

    internal void Stop()
    {
        bool shouldFlush = false;
        lock (this.sync)
        {
            if (this.lifecycle is Lifecycle.Flushing or Lifecycle.Draining or Lifecycle.Completed)
            {
                return;
            }
            if (this.lifecycle == Lifecycle.Created)
            {
                this.lifecycle = Lifecycle.Draining;
            }
            else
            {
                this.lifecycle = Lifecycle.Flushing;
                shouldFlush = true;
            }
        }

        if (shouldFlush)
        {
            try
            {
                this.flush();
            }
            catch (Exception exception)
            {
                lock (this.sync)
                {
                    this.flushFailure = exception;
                    this.failure ??= exception;
                    if (this.lifecycle != Lifecycle.Completed)
                    {
                        this.lifecycle = Lifecycle.Draining;
                    }
                }
            }
        }
        this.TryComplete();
    }

    internal bool EnterCallback(out bool processSample)
    {
        lock (this.sync)
        {
            if (this.lifecycle == Lifecycle.Completed)
            {
                processSample = false;
                return false;
            }
            this.activeCallbacks++;
            processSample = this.lifecycle == Lifecycle.Running;
            return true;
        }
    }

    internal void ExitCallback(Exception? callbackFailure, bool requestNextSample)
    {
        bool requestStop = false;
        lock (this.sync)
        {
            if (this.activeCallbacks <= 0)
            {
                throw new InvalidOperationException("No asynchronous Source Reader callback is active.");
            }

            if (callbackFailure is not null)
            {
                this.failure ??= callbackFailure;
                if (this.lifecycle is Lifecycle.Running or Lifecycle.StopRequested)
                {
                    this.lifecycle = Lifecycle.StopRequested;
                    requestStop = true;
                }
            }
            else if (requestNextSample && this.lifecycle == Lifecycle.Running)
            {
                try
                {
                    this.requestSample();
                }
                catch (Exception exception)
                {
                    this.failure ??= exception;
                    this.lifecycle = Lifecycle.StopRequested;
                    requestStop = true;
                }
            }
            this.activeCallbacks--;
        }

        if (requestStop)
        {
            this.stopRequested.TrySetResult(true);
        }
        this.TryComplete();
    }

    internal void OnFlushed()
    {
        lock (this.sync)
        {
            if (this.lifecycle == Lifecycle.Flushing)
            {
                this.lifecycle = Lifecycle.Draining;
            }
        }
        this.TryComplete();
    }

    private void TryComplete()
    {
        Exception? completionFailure;
        lock (this.sync)
        {
            if (this.lifecycle != Lifecycle.Draining || this.activeCallbacks != 0)
            {
                return;
            }
            this.lifecycle = Lifecycle.Completed;
            completionFailure = this.failure;
        }

        if (completionFailure is null)
        {
            this.completion.TrySetResult(true);
        }
        else
        {
            this.completion.TrySetException(completionFailure);
        }
    }
}
#endif
