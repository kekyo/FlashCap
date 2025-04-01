////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap;

public static class ObservableCaptureDeviceExtension
{
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Task StartAsync(this ObservableCaptureDevice observableCaptureDevice, CancellationToken ct = default) =>
        observableCaptureDevice.InternalStartAsync(ct);

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Task StopAsync(this ObservableCaptureDevice observableCaptureDevice, CancellationToken ct = default) =>
        observableCaptureDevice.InternalStopAsync(ct);

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IDisposable Subscribe(
        this ObservableCaptureDevice observableCaptureDevice,
        IObserver<PixelBufferScope> observer) =>
        observableCaptureDevice.InternalSubscribe(observer);
}
