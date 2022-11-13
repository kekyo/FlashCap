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

    public VideoCharacteristics Characteristics { get; protected set; } = null!;
    public bool IsRunning { get; protected set; }

    protected abstract void OnStart();
    protected abstract void OnStop();

    internal void InternalStart() =>
        this.OnStart();
    internal void InternalStop() =>
        this.OnStop();

    protected abstract void OnCapture(
        IntPtr pData, int size, long timestampMicroseconds, long frameIndex, PixelBuffer buffer);

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal void InternalOnCapture(
        IntPtr pData, int size, long timestampMicroseconds, long frameIndex, PixelBuffer buffer) =>
        this.OnCapture(pData, size, timestampMicroseconds, frameIndex, buffer);
}
