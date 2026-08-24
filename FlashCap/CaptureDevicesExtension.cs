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
}
