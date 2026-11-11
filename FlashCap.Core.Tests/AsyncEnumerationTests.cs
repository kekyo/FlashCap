////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed class AsyncEnumerationTests
{
    [Test]
    public async Task SynchronousBackendReturnsDescriptorsWithoutSchedulingWork()
    {
        var devices = new TestDevices();
        await using var enumerator = devices.InternalEnumerateDescriptorsAsync(default).GetAsyncEnumerator();
        Assert.That(devices.Visited, Is.Zero);

        var next = enumerator.MoveNextAsync();
        Assert.That(next.IsCompleted, Is.True);
        Assert.That(await next, Is.True);
        Assert.That(enumerator.Current, Is.SameAs(devices.Descriptor));
        Assert.That(await enumerator.MoveNextAsync(), Is.False);
        Assert.That(devices.Disposed, Is.True);
    }

    [Test]
    public async Task StoppingEnumerationDisposesTheSynchronousIterator()
    {
        var devices = new TestDevices();
        var enumerator = devices.InternalEnumerateDescriptorsAsync(default).GetAsyncEnumerator();
        Assert.That(await enumerator.MoveNextAsync(), Is.True);
        await enumerator.DisposeAsync();
        Assert.That(devices.Disposed, Is.True);
    }

    [Test]
    public async Task CancellationDoesNotStartSynchronousEnumeration()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var devices = new TestDevices();
        await using var enumerator = devices.InternalEnumerateDescriptorsAsync(default)
            .GetAsyncEnumerator(cancellation.Token);
        Assert.ThrowsAsync<OperationCanceledException>(async () => await enumerator.MoveNextAsync());
        Assert.That(devices.Visited, Is.Zero);
    }

    [Test]
    public async Task MethodCancellationIsObservedBetweenDescriptors()
    {
        using var cancellation = new CancellationTokenSource();
        var devices = new TestDevices();
        await using var enumerator = devices.InternalEnumerateDescriptorsAsync(cancellation.Token).GetAsyncEnumerator();
        Assert.That(await enumerator.MoveNextAsync(), Is.True);
        cancellation.Cancel();
        Assert.ThrowsAsync<OperationCanceledException>(async () => await enumerator.MoveNextAsync());
    }

    private sealed class TestDevices : CaptureDevices
    {
        internal readonly CaptureDeviceDescriptor Descriptor = new TestDescriptor();
        internal int Visited;
        internal bool Disposed;

        protected override IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors()
        {
            try
            {
                this.Visited++;
                yield return this.Descriptor;
            }
            finally
            {
                this.Disposed = true;
            }
        }
    }

    private sealed class TestDescriptor : CaptureDeviceDescriptor
    {
        internal TestDescriptor() : base("Test", "Test", Array.Empty<VideoCharacteristics>(), new DefaultBufferPool()) { }
        public override object Identity => "test";
        public override DeviceTypes DeviceType => DeviceTypes.V4L2;
        protected override Task<CaptureDevice> OnOpenWithFrameProcessorAsync(
            VideoCharacteristics characteristics, TranscodeFormats transcodeFormat,
            FrameProcessor frameProcessor, CancellationToken ct) => throw new NotSupportedException();
    }
}
