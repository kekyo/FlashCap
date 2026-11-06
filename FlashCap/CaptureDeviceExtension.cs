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

public static class CaptureDeviceExtension
{
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Task StartAsync(this CaptureDevice captureDevice, CancellationToken ct = default) =>
        captureDevice.InternalStartAsync(ct);

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Task StopAsync(this CaptureDevice captureDevice, CancellationToken ct = default) =>
        captureDevice.InternalStopAsync(ct);

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Task<bool> ShowPropertyPageAsync(
        this CaptureDevice captureDevice, IntPtr parentWindow, CancellationToken ct = default) =>
        captureDevice.InternalShowPropertyPageAsync(parentWindow, ct);

    /// <summary>
    /// Gets the camera controls the device offers, with their ranges in the native unit of the device.
    /// </summary>
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Task<CaptureDeviceControl[]> GetControlsAsync(
        this CaptureDevice captureDevice, CancellationToken ct = default) =>
        captureDevice.InternalGetControlsAsync(ct);

    /// <summary>
    /// Gets the current value of a camera control.
    /// </summary>
    /// <returns>null when the device does not offer the control.</returns>
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Task<CaptureDeviceControlValue?> GetControlValueAsync(
        this CaptureDevice captureDevice, CaptureDeviceControlTypes type, CancellationToken ct = default) =>
        captureDevice.InternalGetControlValueAsync(type, ct);

    /// <summary>
    /// Sets a camera control.
    /// </summary>
    /// <param name="captureDevice">Opened capture device.</param>
    /// <param name="type">Kind of the control.</param>
    /// <param name="value">Value in the native unit of the device, ignored when isAuto is true.</param>
    /// <param name="isAuto">Lets the device set the value itself, when the control supports it.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>false when the device does not offer the control or refused the value.</returns>
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Task<bool> SetControlValueAsync(
        this CaptureDevice captureDevice, CaptureDeviceControlTypes type, int value, bool isAuto = false, CancellationToken ct = default) =>
        captureDevice.InternalSetControlValueAsync(type, value, isAuto, ct);
}
