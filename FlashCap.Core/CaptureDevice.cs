////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap;

public abstract class CaptureDevice :
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1
    IAsyncDisposable,
#endif
    IDisposable
{
    private readonly AsyncLock locker = new();

    protected CaptureDevice()
    {
    }

    ~CaptureDevice() =>
        this.DisposeAsync(false, default).
        ConfigureAwait(false).
        GetAwaiter().
        GetResult();

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Dispose()
    {
        this.DisposeAsync(true, default).
            ConfigureAwait(false).
            GetAwaiter().
            GetResult();
        GC.SuppressFinalize(this);
    }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1
    ValueTask IAsyncDisposable.DisposeAsync() =>
        new(this.DisposeAsync(true, default));
#endif

    public async Task DisposeAsync(CancellationToken ct = default)
    {
        using var _ = await locker.LockAsync(ct).
            ConfigureAwait(false);

        await this.DisposeAsync(true, ct);
    }

    protected virtual Task DisposeAsync(bool disposing, CancellationToken ct) =>
        TaskCompat.CompletedTask;

    protected abstract Task OnInitializeAsync(
        object identity,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        FrameProcessor frameProcessor,
        CancellationToken ct);

    public VideoCharacteristics Characteristics { get; protected set; } = null!;
    public bool IsRunning { get; protected set; }

    protected abstract Task OnStartAsync(CancellationToken ct);
    protected abstract Task OnStopAsync(CancellationToken ct);

    protected abstract void OnCapture(
        IntPtr pData, int size, long timestampMicroseconds, long frameIndex, PixelBuffer buffer);

    //////////////////////////////////////////////////////////////////////////

    internal Task InternalInitializeAsync(
        object identity,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        FrameProcessor frameProcessor,
        CancellationToken ct) =>
        this.OnInitializeAsync(identity, characteristics, transcodeIfYUV, frameProcessor, ct);

    internal async Task InternalStartAsync(CancellationToken ct)
    {
        using var _ = await locker.LockAsync(ct).
            ConfigureAwait(false);

        await this.OnStartAsync(ct);
    }

    internal async Task InternalStopAsync(CancellationToken ct)
    {
        using var _ = await locker.LockAsync(ct).
            ConfigureAwait(false);

        await this.OnStopAsync(ct);
    }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal void InternalOnCapture(
        IntPtr pData, int size, long timestampMicroseconds, long frameIndex, PixelBuffer buffer) =>
        this.OnCapture(pData, size, timestampMicroseconds, frameIndex, buffer);
}
