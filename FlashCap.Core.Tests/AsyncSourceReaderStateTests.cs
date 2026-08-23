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
using System.Threading.Tasks;

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed class AsyncSourceReaderStateTests
{
    [Test]
    public void StartRequestsFirstSampleOnlyOnce()
    {
        var requests = 0;
        var state = new AsyncSourceReaderState(() => requests++, () => { });

        Assert.That(state.Start(), Is.True);
        Assert.That(state.Start(), Is.True);

        Assert.That(requests, Is.EqualTo(1));
        Assert.That(state.Completion.IsCompleted, Is.False);
        Assert.That(state.StopRequested.IsCompleted, Is.False);
    }

    [Test]
    public async Task StopBeforeStartCompletesWithoutFlushing()
    {
        var flushes = 0;
        var state = new AsyncSourceReaderState(() => { }, () => flushes++);

        state.Stop();
        state.Stop();

        Assert.That(state.Completion.IsCompletedSuccessfully, Is.True);
        await state.Completion;
        Assert.That(flushes, Is.EqualTo(0));
        Assert.That(state.Start(), Is.False);
        Assert.That(state.EnterCallback(out var processSample), Is.False);
        Assert.That(processSample, Is.False);
    }

    [Test]
    public async Task RunningStateRequestsSamplesAndCompletesAfterFlush()
    {
        var requests = 0;
        var flushes = 0;
        var state = new AsyncSourceReaderState(() => requests++, () => flushes++);

        Assert.That(state.Start(), Is.True);
        Assert.That(state.EnterCallback(out var processSample), Is.True);
        Assert.That(processSample, Is.True);
        state.ExitCallback(null, true);
        state.Stop();

        Assert.That(requests, Is.EqualTo(2));
        Assert.That(flushes, Is.EqualTo(1));
        Assert.That(state.Completion.IsCompleted, Is.False);

        state.OnFlushed();
        Assert.That(state.Completion.IsCompletedSuccessfully, Is.True);
        await state.Completion;

        state.Stop();
        Assert.That(flushes, Is.EqualTo(1));
    }

    [Test]
    public async Task CompletionWaitsForActiveCallbackAfterFlush()
    {
        var state = new AsyncSourceReaderState(() => { }, () => { });
        Assert.That(state.Start(), Is.True);
        Assert.That(state.EnterCallback(out var processSample), Is.True);
        Assert.That(processSample, Is.True);

        state.Stop();
        state.OnFlushed();

        Assert.That(state.Completion.IsCompleted, Is.False);

        state.ExitCallback(null, false);
        Assert.That(state.Completion.IsCompletedSuccessfully, Is.True);
        await state.Completion;
    }

    [Test]
    public async Task CallbackFailureRequestsStopAndFaultsAfterFlush()
    {
        var failure = new InvalidOperationException("Callback failed.");
        var state = new AsyncSourceReaderState(() => { }, () => { });
        Assert.That(state.Start(), Is.True);
        Assert.That(state.EnterCallback(out _), Is.True);

        state.ExitCallback(failure, false);

        Assert.That(state.StopRequested.IsCompletedSuccessfully, Is.True);
        await state.StopRequested;
        Assert.That(state.Completion.IsCompleted, Is.False);

        state.Stop();
        state.OnFlushed();

        Assert.That(state.Completion.IsFaulted, Is.True);
        var thrown = Assert.ThrowsAsync<InvalidOperationException>(
            () => state.Completion);
        Assert.That(thrown, Is.SameAs(failure));
    }

    [Test]
    public async Task RequestingNextSampleFailureRequestsStop()
    {
        var requests = 0;
        var failure = new InvalidOperationException("Request failed.");
        var state = new AsyncSourceReaderState(
            () =>
            {
                if (++requests == 2)
                {
                    throw failure;
                }
            },
            () => { });
        Assert.That(state.Start(), Is.True);
        Assert.That(state.EnterCallback(out _), Is.True);

        state.ExitCallback(null, true);

        Assert.That(state.StopRequested.IsCompletedSuccessfully, Is.True);
        await state.StopRequested;
        Assert.That(requests, Is.EqualTo(2));

        state.Stop();
        state.OnFlushed();
        Assert.That(state.Completion.IsFaulted, Is.True);
        var thrown = Assert.ThrowsAsync<InvalidOperationException>(
            () => state.Completion);
        Assert.That(thrown, Is.SameAs(failure));
    }

    [Test]
    public void StartPropagatesRequestFailureAndFaultsCompletion()
    {
        var failure = new InvalidOperationException("Startup request failed.");
        var state = new AsyncSourceReaderState(() => throw failure, () => { });

        var thrown = Assert.Throws<InvalidOperationException>(() => state.Start());
        Assert.That(thrown, Is.SameAs(failure));

        Assert.That(state.Completion.IsFaulted, Is.True);
        thrown = Assert.ThrowsAsync<InvalidOperationException>(
            () => state.Completion);
        Assert.That(thrown, Is.SameAs(failure));
        Assert.That(state.StopRequested.IsCompleted, Is.False);
    }

    [Test]
    public void FlushFailureIsReportedAndFaultsCompletion()
    {
        var failure = new InvalidOperationException("Flush failed.");
        var state = new AsyncSourceReaderState(() => { }, () => throw failure);
        Assert.That(state.Start(), Is.True);

        state.Stop();

        Assert.That(state.FlushFailure, Is.SameAs(failure));
        Assert.That(state.Completion.IsFaulted, Is.True);
        var thrown = Assert.ThrowsAsync<InvalidOperationException>(
            () => state.Completion);
        Assert.That(thrown, Is.SameAs(failure));
    }

    [Test]
    public void ExitCallbackRequiresActiveCallback()
    {
        var state = new AsyncSourceReaderState(() => { }, () => { });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            state.ExitCallback(null, false));

        Assert.That(exception!.Message, Is.EqualTo("No asynchronous Source Reader callback is active."));
    }
}
