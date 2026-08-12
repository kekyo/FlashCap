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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32.Foundation;

namespace FlashCap.Internal.MediaFoundation;

internal static class MediaFoundationHelpers
{
    internal static void ThrowIfFailed(HRESULT result, string operation)
    {
        if (result.Failed)
        {
            throw new InvalidOperationException(
                $"FlashCap: {operation} failed (HRESULT=0x{unchecked((uint)result.Value):X8}).");
        }
    }

    internal static void TraceFailure(string operation, Exception exception) =>
        Trace.WriteLine($"FlashCap: Media Foundation {operation} failed: {exception}");

    internal static async Task WaitAsync(Task task, CancellationToken ct)
    {
#if NET6_0_OR_GREATER
        await task.WaitAsync(ct).ConfigureAwait(false);
#else
        if (task.IsCompleted)
        {
            await task.ConfigureAwait(false);
            return;
        }

        var cancellation = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = ct.Register(() => cancellation.TrySetResult(true));
        if (await Task.WhenAny(task, cancellation.Task).ConfigureAwait(false) != task)
        {
            ct.ThrowIfCancellationRequested();
        }
        await task.ConfigureAwait(false);
#endif
    }
}
#endif
