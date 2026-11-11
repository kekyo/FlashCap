////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed class AsyncLockTests
{
    [Test]
    public async Task ConcurrentAcquisitionAndReleasePreserveExclusiveOwnership()
    {
        var mutex = new AsyncLock();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        using var phase = new Barrier(2);
        var active = 0;
        var completed = 0;
        // Align an acquisition with the owner's release on separate threads.
        // Each round starts with a single owner and no outstanding waiters.
        var owner = Task.Factory.StartNew(() =>
        {
            try
            {
                for (var iteration = 0; iteration < 10000; iteration++)
                {
                    using (mutex.LockAsync(cancellation.Token).GetAwaiter().GetResult())
                    {
                        Assert.That(Interlocked.Increment(ref active), Is.EqualTo(1));
                        phase.SignalAndWait(cancellation.Token);
                        Interlocked.Decrement(ref active);
                    }
                    phase.SignalAndWait(cancellation.Token);
                }
            }
            catch { cancellation.Cancel(); throw; }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        var contender = Task.Factory.StartNew(() =>
        {
            try
            {
                for (var iteration = 0; iteration < 10000; iteration++)
                {
                    phase.SignalAndWait(cancellation.Token);
                    using (mutex.LockAsync(cancellation.Token).GetAwaiter().GetResult())
                    {
                        Assert.That(Interlocked.Increment(ref active), Is.EqualTo(1));
                        completed++;
                        Interlocked.Decrement(ref active);
                    }
                    phase.SignalAndWait(cancellation.Token);
                }
            }
            catch { cancellation.Cancel(); throw; }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        var workers = Task.WhenAll(owner, contender);
        try { await workers; }
        catch
        {
            TestContext.Out.WriteLine(workers.Exception);
            throw;
        }
        Assert.That(completed, Is.EqualTo(10000));
        using var final = await mutex.LockAsync(cancellation.Token);
    }

    [Test]
    public async Task CanceledWaitersDoNotBlockTheNextOwner()
    {
        var mutex = new AsyncLock();
        using var cancellation = new CancellationTokenSource();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var owner = await mutex.LockAsync(timeout.Token);
        var canceled = mutex.LockAsync(cancellation.Token).AsTask();
        var next = mutex.LockAsync(timeout.Token).AsTask();
        Assert.That(next.IsCompleted, Is.False);
        cancellation.Cancel();
        Assert.CatchAsync<OperationCanceledException>(async () => await canceled);
        Assert.That(next.IsCompleted, Is.False);
        owner.Dispose();
        Task<AsyncLock.Disposer> last;
        using (await next)
        {
            last = mutex.LockAsync(timeout.Token).AsTask();
            Assert.That(last.IsCompleted, Is.False);
        }
        using var final = await last;
    }

    [Test]
    public async Task CancellationBeforeAcquisitionDoesNotTakeOwnership()
    {
        var mutex = new AsyncLock();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            using var ignored = await mutex.LockAsync(cancellation.Token);
        });
        using var owner = await mutex.LockAsync(CancellationToken.None);
    }

    [Test]
    public async Task CancellationRacingWithReleaseDoesNotLoseTheLock()
    {
        var mutex = new AsyncLock();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        for (var iteration = 0; iteration < 1000; iteration++)
        {
            using var cancellation = new CancellationTokenSource();
            var owner = await mutex.LockAsync(timeout.Token);
            var pending = mutex.LockAsync(cancellation.Token).AsTask();
            await Task.WhenAll(Task.Run(cancellation.Cancel), Task.Run(owner.Dispose));
            try { using var granted = await pending; }
            catch (OperationCanceledException) { }
            using var next = await mutex.LockAsync(timeout.Token);
        }
    }
}
