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
    /// <param name="captureDevice">Opened capture device.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Controls exposed through the selected backend, or an empty array when none are available.</returns>
    /// <remarks>Equal minimum and maximum values indicate a fixed value. Absence does not distinguish unsupported hardware from backend limitations.</remarks>
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Task<CaptureDeviceControl[]> GetControlsAsync(
        this CaptureDevice captureDevice, CancellationToken ct = default) =>
        captureDevice.InternalGetControlsAsync(ct);

    /// <summary>
    /// Gets the current value of a camera control.
    /// </summary>
    /// <param name="captureDevice">Opened capture device.</param>
    /// <param name="type">Kind of the control.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current value, or null when unsupported, unavailable or not representable by this control.</returns>
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
    /// <returns>Whether the driver accepted the request; false when the control is unsupported or the operation failed.</returns>
    /// <remarks>
    /// Read the value again to verify that the device applied it. Some devices only apply certain controls during capture.
    /// On V4L2, automatic exposure and iris may change together. Read both controls again after changing either.
    /// </remarks>
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Task<bool> SetControlValueAsync(
        this CaptureDevice captureDevice, CaptureDeviceControlTypes type, int value, bool isAuto = false, CancellationToken ct = default) =>
        captureDevice.InternalSetControlValueAsync(type, value, isAuto, ct);
}
