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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Internal;

internal sealed class AsyncLock
{
    private readonly Disposer disposer;
    private readonly Queue<AsyncTaskCompletionSource<Disposer>> queue = new();
    private bool held;

    public AsyncLock() =>
        this.disposer = new(this);

#if NETCOREAPP || NETSTANDARD2_1
    public ValueTask<Disposer> LockAsync(CancellationToken ct)
#else
    public Task<Disposer> LockAsync(CancellationToken ct)
#endif
    {
        ct.ThrowIfCancellationRequested();
        AsyncTaskCompletionSource<Disposer> waiter;
        CancellationTokenRegistration registration;
        lock (this.queue)
        {
            if (!this.held)
            {
                this.held = true;
#if NETCOREAPP || NETSTANDARD2_1
                return new(this.disposer);
#else
                return TaskCompat.FromResult(this.disposer);
#endif
            }

            // Publish the waiter atomically with observing the current owner.
            // Unlock must never observe contention before its waiter is queued.
            waiter = new AsyncTaskCompletionSource<Disposer>();
            // Register before publishing the waiter: older cancellation sources
            // may reject registration after disposal, leaving nothing to enqueue.
            registration = ct.Register(() => waiter.TrySetCanceled(ct));
            this.queue.Enqueue(waiter);
        }

#if NETCOREAPP || NETSTANDARD2_1
        return new(WaitAsync(waiter, registration));
#else
        return WaitAsync(waiter, registration);
#endif
    }

    private static async Task<Disposer> WaitAsync(
        AsyncTaskCompletionSource<Disposer> waiter, CancellationTokenRegistration registration)
    {
        using (registration)
            return await waiter.Task.ConfigureAwait(false);
    }

    private void Unlock()
    {
        while (true)
        {
            AsyncTaskCompletionSource<Disposer> waiter;
            lock (this.queue)
            {
                Debug.Assert(this.held);
                if (this.queue.Count == 0)
                {
                    this.held = false;
                    return;
                }
                waiter = this.queue.Dequeue();
            }

            // Keep ownership reserved until a live waiter accepts it. Completion
            // and cancellation-registration cleanup run outside the queue monitor.
            if (waiter.TrySetResult(this.disposer)) return;
        }
    }

    public sealed class Disposer : IDisposable
    {
        private readonly AsyncLock parent;

        internal Disposer(AsyncLock parent) =>
            this.parent = parent;

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Dispose() =>
            this.parent.Unlock();
    }
}
