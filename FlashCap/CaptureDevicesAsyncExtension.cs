////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

#if !(NET35 || NET40 || NET45 || NETSTANDARD1_3)
using FlashCap.Devices;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace FlashCap;

public static partial class CaptureDevicesExtension
{
    /// <summary>Enumerates available capture devices asynchronously.</summary>
    /// <param name="captureDevices">The device enumerator.</param>
    /// <param name="ct">Cancels enumeration.</param>
    /// <returns>A finite sequence of available descriptors.</returns>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Default enumeration includes DirectShow. Use a platform-specific Devices type for Native AOT.")]
#endif
    public static IAsyncEnumerable<CaptureDeviceDescriptor> EnumerateDescriptorsAsync(
        this CaptureDevices captureDevices, CancellationToken ct = default) =>
        captureDevices.InternalEnumerateDescriptorsAsync(ct);

    /// <summary>Gets all available capture devices asynchronously.</summary>
    /// <param name="captureDevices">The device enumerator.</param>
    /// <param name="ct">Cancels enumeration.</param>
    /// <returns>The available descriptors.</returns>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Default enumeration includes DirectShow. Use a platform-specific Devices type for Native AOT.")]
#endif
    public static Task<CaptureDeviceDescriptor[]> GetDescriptorsAsync(
        this CaptureDevices captureDevices, CancellationToken ct = default) =>
        CollectDescriptorsAsync(captureDevices.InternalEnumerateDescriptorsAsync(ct));

    private static async Task<CaptureDeviceDescriptor[]> CollectDescriptorsAsync(
        IAsyncEnumerable<CaptureDeviceDescriptor> descriptors)
    {
        var result = new List<CaptureDeviceDescriptor>();
        await foreach (var descriptor in descriptors.ConfigureAwait(false))
            result.Add(descriptor);
        return result.ToArray();
    }

    /// <summary>Enumerates available devices using the V4L2 backend.</summary>
    /// <param name="captureDevices">The device enumerator.</param>
    /// <param name="ct">Cancels enumeration.</param>
    /// <returns>A finite sequence of available descriptors.</returns>
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "This platform-specific backend does not require unreferenced code.")]
#endif
    public static IAsyncEnumerable<CaptureDeviceDescriptor> EnumerateDescriptorsAsync(
        this V4L2Devices captureDevices, CancellationToken ct = default) =>
        captureDevices.InternalEnumerateDescriptorsAsync(ct);

    /// <summary>Gets all available devices using the V4L2 backend.</summary>
    /// <param name="captureDevices">The device enumerator.</param>
    /// <param name="ct">Cancels enumeration.</param>
    /// <returns>The available descriptors.</returns>
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "This platform-specific backend does not require unreferenced code.")]
#endif
    public static Task<CaptureDeviceDescriptor[]> GetDescriptorsAsync(
        this V4L2Devices captureDevices, CancellationToken ct = default) =>
        CollectDescriptorsAsync(captureDevices.InternalEnumerateDescriptorsAsync(ct));

    /// <summary>Enumerates available devices using the AVFoundation backend.</summary>
    /// <param name="captureDevices">The device enumerator.</param>
    /// <param name="ct">Cancels enumeration.</param>
    /// <returns>A finite sequence of available descriptors.</returns>
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "This platform-specific backend does not require unreferenced code.")]
#endif
    public static IAsyncEnumerable<CaptureDeviceDescriptor> EnumerateDescriptorsAsync(
        this AVFoundationDevices captureDevices, CancellationToken ct = default) =>
        captureDevices.InternalEnumerateDescriptorsAsync(ct);

    /// <summary>Gets all available devices using the AVFoundation backend.</summary>
    /// <param name="captureDevices">The device enumerator.</param>
    /// <param name="ct">Cancels enumeration.</param>
    /// <returns>The available descriptors.</returns>
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "This platform-specific backend does not require unreferenced code.")]
#endif
    public static Task<CaptureDeviceDescriptor[]> GetDescriptorsAsync(
        this AVFoundationDevices captureDevices, CancellationToken ct = default) =>
        CollectDescriptorsAsync(captureDevices.InternalEnumerateDescriptorsAsync(ct));

    /// <summary>Enumerates available devices using the MediaFoundation backend.</summary>
    /// <param name="captureDevices">The device enumerator.</param>
    /// <param name="ct">Cancels enumeration.</param>
    /// <returns>A finite sequence of available descriptors.</returns>
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "This platform-specific backend does not require unreferenced code.")]
#endif
    public static IAsyncEnumerable<CaptureDeviceDescriptor> EnumerateDescriptorsAsync(
        this MediaFoundationDevices captureDevices, CancellationToken ct = default) =>
        captureDevices.InternalEnumerateDescriptorsAsync(ct);

    /// <summary>Gets all available devices using the MediaFoundation backend.</summary>
    /// <param name="captureDevices">The device enumerator.</param>
    /// <param name="ct">Cancels enumeration.</param>
    /// <returns>The available descriptors.</returns>
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "This platform-specific backend does not require unreferenced code.")]
#endif
    public static Task<CaptureDeviceDescriptor[]> GetDescriptorsAsync(
        this MediaFoundationDevices captureDevices, CancellationToken ct = default) =>
        CollectDescriptorsAsync(captureDevices.InternalEnumerateDescriptorsAsync(ct));
}
#endif
