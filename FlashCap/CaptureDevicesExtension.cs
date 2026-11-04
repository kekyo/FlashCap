////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq;
using FlashCap.Devices;

namespace FlashCap;

public static class CaptureDevicesExtension
{
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Default device enumeration includes DirectShow, which requires runtime-generated COM interop. Use a platform-specific Devices type for Native AOT.")]
#endif
    public static IEnumerable<CaptureDeviceDescriptor> EnumerateDescriptors(
        this CaptureDevices captureDevices) =>
        captureDevices.InternalEnumerateDescriptors();

#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Default device enumeration includes DirectShow, which requires runtime-generated COM interop. Use a platform-specific Devices type for Native AOT.")]
#endif
    public static CaptureDeviceDescriptor[] GetDescriptors(
        this CaptureDevices captureDevices) =>
        captureDevices.InternalEnumerateDescriptors().ToArray();

#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The Media Foundation override does not require unreferenced code.")]
#endif
    public static IEnumerable<CaptureDeviceDescriptor> EnumerateDescriptors(
        this MediaFoundationDevices captureDevices) =>
        captureDevices.InternalEnumerateDescriptors();

#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The Media Foundation override does not require unreferenced code.")]
#endif
    public static CaptureDeviceDescriptor[] GetDescriptors(
        this MediaFoundationDevices captureDevices) =>
        captureDevices.InternalEnumerateDescriptors().ToArray();

    /// <summary>
    /// Enumerates capture device descriptors using the V4L2 backend.
    /// </summary>
    /// <param name="captureDevices">The V4L2 device enumerator.</param>
    /// <returns>The available capture device descriptors.</returns>
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The V4L2 override does not require unreferenced code.")]
#endif
    public static IEnumerable<CaptureDeviceDescriptor> EnumerateDescriptors(
        this V4L2Devices captureDevices) =>
        captureDevices.InternalEnumerateDescriptors();

    /// <summary>
    /// Gets capture device descriptors using the V4L2 backend.
    /// </summary>
    /// <param name="captureDevices">The V4L2 device enumerator.</param>
    /// <returns>An array of the available capture device descriptors.</returns>
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The V4L2 override does not require unreferenced code.")]
#endif
    public static CaptureDeviceDescriptor[] GetDescriptors(
        this V4L2Devices captureDevices) =>
        captureDevices.InternalEnumerateDescriptors().ToArray();

    /// <summary>
    /// Enumerates capture device descriptors using the AVFoundation backend.
    /// </summary>
    /// <param name="captureDevices">The AVFoundation device enumerator.</param>
    /// <returns>The available capture device descriptors.</returns>
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The AVFoundation override does not require unreferenced code.")]
#endif
    public static IEnumerable<CaptureDeviceDescriptor> EnumerateDescriptors(
        this AVFoundationDevices captureDevices) =>
        captureDevices.InternalEnumerateDescriptors();

    /// <summary>
    /// Gets capture device descriptors using the AVFoundation backend.
    /// </summary>
    /// <param name="captureDevices">The AVFoundation device enumerator.</param>
    /// <returns>An array of the available capture device descriptors.</returns>
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The AVFoundation override does not require unreferenced code.")]
#endif
    public static CaptureDeviceDescriptor[] GetDescriptors(
        this AVFoundationDevices captureDevices) =>
        captureDevices.InternalEnumerateDescriptors().ToArray();
}
