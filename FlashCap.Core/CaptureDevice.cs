////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap;

public abstract class CaptureDevice : IDisposable
{
    protected CaptureDevice()
    {
    }

    ~CaptureDevice() =>
        this.Dispose(false);

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    protected abstract Task OnInitializeAsync(
        object identity,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        FrameProcessor frameProcessor,
        CancellationToken ct);

    public VideoCharacteristics Characteristics { get; protected set; } = null!;
    public bool IsRunning { get; protected set; }

    protected abstract void OnStart();
    protected abstract void OnStop();

    protected abstract void OnCapture(
        IntPtr pData, int size, long timestampMicroseconds, long frameIndex, PixelBuffer buffer);

    //////////////////////////////////////////////////////////////////////////

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Task InternalInitializeAsync(
        object identity,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        FrameProcessor frameProcessor,
        CancellationToken ct) =>
        this.OnInitializeAsync(identity, characteristics, transcodeIfYUV, frameProcessor, ct);
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal void InternalStart() =>
        this.OnStart();
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal void InternalStop() =>
        this.OnStop();
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal void InternalOnCapture(
        IntPtr pData, int size, long timestampMicroseconds, long frameIndex, PixelBuffer buffer) =>
        this.OnCapture(pData, size, timestampMicroseconds, frameIndex, buffer);
}
