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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Internal.MediaFoundation;

/// <summary>
/// Keeps COM initialization, source ownership and synchronous camera commands on one thread.
/// The asynchronous Source Reader delivers frames on Media Foundation's callback threads.
/// </summary>
internal sealed class MediaFoundationWorker
{
    private readonly object sync = new();
    private readonly Queue<Action<Exception?>> commands = new();
    private readonly Task completion;
    private bool closing;
    private Exception? failure;

    internal MediaFoundationWorker(Action initialize, Action uninitialize) =>
        this.completion = MediaFoundationHelpers.StartCaptureWorker(() => this.Run(initialize, uninitialize));

    internal Task<T> InvokeAsync<T>(Func<T> operation, CancellationToken ct)
    {
        var result = new AsyncTaskCompletionSource<T>();
        lock (this.sync)
        {
            if (this.failure is { } failure)
            {
                result.TrySetException(failure);
            }
            else if (this.closing)
            {
                result.TrySetException(new ObjectDisposedException(nameof(MediaFoundationWorker)));
            }
            else
            {
                this.commands.Enqueue(failure =>
                {
                    try
                    {
                        if (failure is not null)
                        {
                            result.TrySetException(failure);
                            return;
                        }
                        // A queued command can be canceled. Once a native call starts, wait for
                        // it to return before completing the task and releasing the device lock.
                        ct.ThrowIfCancellationRequested();
                        result.TrySetResult(operation());
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        result.TrySetCanceled(ct);
                    }
                    catch (Exception exception)
                    {
                        result.TrySetException(exception);
                    }
                });
                Monitor.Pulse(this.sync);
            }
        }
        return result.Task;
    }

    internal Task InvokeAsync(Action operation, CancellationToken ct) =>
        this.InvokeAsync(() => { operation(); return 0; }, ct);

    internal Task DisposeAsync()
    {
        lock (this.sync)
        {
            this.closing = true;
            Monitor.Pulse(this.sync);
        }
        return this.completion;
    }

    private void Run(Action initialize, Action uninitialize)
    {
        var initialized = false;
        try
        {
            initialize();
            initialized = true;
            while (true)
            {
                Action<Exception?> command;
                lock (this.sync)
                {
                    while (this.commands.Count == 0 && !this.closing)
                    {
                        Monitor.Wait(this.sync);
                    }
                    if (this.commands.Count == 0)
                    {
                        return;
                    }
                    command = this.commands.Dequeue();
                }
                command(null);
            }
        }
        catch (Exception exception)
        {
            lock (this.sync)
            {
                this.failure = exception;
                while (this.commands.Count != 0)
                {
                    this.commands.Dequeue()(exception);
                }
            }
            throw;
        }
        finally
        {
            if (initialized)
            {
                uninitialize();
            }
        }
    }
}
#endif
