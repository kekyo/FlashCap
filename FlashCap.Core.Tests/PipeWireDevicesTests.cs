////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Devices;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Core.Tests;

#pragma warning disable CA1416 // Cancellation and the synchronous guard must run before native loading.
[TestFixture]
public sealed class PipeWireDevicesTests
{
    [Test]
    public void SynchronousEnumerationAlwaysThrows()
    {
        CaptureDevices devices = new PipeWireDevices();
        Assert.Throws<NotSupportedException>(() => devices.InternalEnumerateDescriptors());
    }

    [Test]
    public async Task CancelledEnumerationDoesNotLoadLibrariesOrRequestCameraAccess()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await using var enumerator = new PipeWireDevices().InternalEnumerateDescriptorsAsync(default)
            .GetAsyncEnumerator(cancellation.Token);
        Assert.ThrowsAsync<OperationCanceledException>(async () => await enumerator.MoveNextAsync());
    }
}
#pragma warning restore CA1416
