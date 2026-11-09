////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal.MediaFoundation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed class MediaFoundationWorkerTests
{
    [Test]
    public async Task OperationsAndCleanupUseTheInitializedThreadInOrder()
    {
        var events = new List<string>();
        var thread = 0;
        var cleanupThread = 0;
        var worker = new MediaFoundationWorker(
            () => { thread = Environment.CurrentManagedThreadId; events.Add("initialize"); },
            () => { cleanupThread = Environment.CurrentManagedThreadId; events.Add("cleanup"); });
        try
        {
            var first = worker.InvokeAsync(() => { events.Add("first"); return Environment.CurrentManagedThreadId; }, CancellationToken.None);
            var second = worker.InvokeAsync(() => { events.Add("second"); return Environment.CurrentManagedThreadId; }, CancellationToken.None);
            Assert.That(await first, Is.EqualTo(thread));
            Assert.That(await second, Is.EqualTo(thread));
        }
        finally
        {
            await worker.DisposeAsync();
        }
        Assert.That(cleanupThread, Is.EqualTo(thread));
        Assert.That(events, Is.EqualTo(new[] { "initialize", "first", "second", "cleanup" }));
        Assert.That(thread, Is.Not.Zero);
    }

    [Test]
    public async Task CanceledQueuedOperationDoesNotChangeTheDevice()
    {
        using var entered = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        using var cancellation = new CancellationTokenSource();
        var changed = false;
        var worker = new MediaFoundationWorker(() => { }, () => { });
        var blocker = worker.InvokeAsync(() => { entered.Set(); resume.Wait(); return 0; }, CancellationToken.None);
        try
        {
            entered.Wait();
            var canceled = worker.InvokeAsync(() => changed = true, cancellation.Token);
            cancellation.Cancel();
            resume.Set();
            await blocker;
            Assert.ThrowsAsync<TaskCanceledException>(async () => await canceled);
            Assert.That(changed, Is.False);
            Assert.That(await worker.InvokeAsync(() => 42, CancellationToken.None), Is.EqualTo(42));
        }
        finally
        {
            resume.Set();
            await worker.DisposeAsync();
        }
    }

    [Test]
    public async Task DisposalWaitsForAnActiveOperationAndRejectsNewOperations()
    {
        using var entered = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        var cleanedUp = false;
        var worker = new MediaFoundationWorker(() => { }, () => cleanedUp = true);
        var operation = worker.InvokeAsync(() => { entered.Set(); resume.Wait(); return 42; }, CancellationToken.None);
        try
        {
            entered.Wait();
            var disposal = worker.DisposeAsync();
            Assert.That(disposal.IsCompleted, Is.False);
            Assert.That(cleanedUp, Is.False);
            Assert.ThrowsAsync<ObjectDisposedException>(async () => await worker.InvokeAsync(() => 0, CancellationToken.None));
            resume.Set();
            Assert.That(await operation, Is.EqualTo(42));
            await disposal;
            Assert.That(cleanedUp, Is.True);
            await worker.DisposeAsync();
        }
        finally
        {
            resume.Set();
            await worker.DisposeAsync();
        }
    }

    [Test]
    public async Task AnOperationFailureDoesNotPreventCleanupOrLaterOperations()
    {
        var cleanedUp = false;
        var worker = new MediaFoundationWorker(() => { }, () => cleanedUp = true);
        try
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await worker.InvokeAsync<int>(() => throw new InvalidOperationException("driver failure"), CancellationToken.None));
            Assert.That(await worker.InvokeAsync(() => 42, CancellationToken.None), Is.EqualTo(42));
        }
        finally
        {
            await worker.DisposeAsync();
        }
        Assert.That(cleanedUp, Is.True);
    }

    [Test]
    public void InitializationFailureFailsQueuedOperationsWithoutUninitializing()
    {
        var cleanedUp = false;
        var worker = new MediaFoundationWorker(
            () => throw new InvalidOperationException("initialization failure"),
            () => cleanedUp = true);
        Assert.ThrowsAsync<InvalidOperationException>(async () => await worker.InvokeAsync(() => 42, CancellationToken.None));
        Assert.ThrowsAsync<InvalidOperationException>(async () => await worker.DisposeAsync());
        Assert.That(cleanedUp, Is.False);
    }
}
