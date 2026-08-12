////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
// Copyright (c) Yoh Deadfall (@YohDeadfall)
// Copyright (c) Felipe Ferreira Quintella (@ffquintella)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Devices;
using FlashCap.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace FlashCap;

/// <summary>
/// By default, the following backends are considered for <see cref="OnEnumerateDescriptors"/>:
/// <list type= "bullet">
/// <item><description><see cref="DirectShowDevices"/> (windows)</description></item>
/// <item><description><see cref="VideoForWindowsDevices"/> (windows)</description></item>
/// <item><description><c>MediaFoundationDevices</c> (Windows 7 or greater) - Supported on net48, netstandard2.0 or greater, .NET 5.0 or greater</description></item>
/// <item><description><see cref="V4L2Devices"/> (linux)</description></item>
/// <item><description><see cref="AVFoundationDevices"/> (macOs)</description></item>
/// </list>
/// </summary>
public class CaptureDevices
{
    protected readonly BufferPool DefaultBufferPool;

    public CaptureDevices() :
        this(new DefaultBufferPool())
    {
    }

    public CaptureDevices(BufferPool defaultBufferPool)
    {
        DefaultBufferPool = defaultBufferPool;
    }

    [RequiresUnreferencedCode("OnEnumerateDescriptors adds DirectShow which requires unreferenced code. Use platform-specific Devices directly.")]
    protected virtual IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors()
    {
        if (NativeMethods.IsWindows())
        {
            var descriptors = new DirectShowDevices(this.DefaultBufferPool).OnEnumerateDescriptors();
            descriptors = descriptors.Concat(new VideoForWindowsDevices(this.DefaultBufferPool).OnEnumerateDescriptors());
#if FLASHCAP_MEDIAFOUNDATION
            if (NativeMethods.IsWindowsVersionAtLeast(6, 1))
                descriptors = descriptors.Concat(new MediaFoundationDevices(this.DefaultBufferPool).OnEnumerateDescriptors());
#endif
            return descriptors;
        }
        if (NativeMethods.IsLinux())
        {
            return new V4L2Devices().OnEnumerateDescriptors();
        }
        if (NativeMethods.IsMacOS())
        {
            return new AVFoundationDevices().OnEnumerateDescriptors();
        }
        return ArrayEx.Empty<CaptureDeviceDescriptor>();
    }

    [RequiresUnreferencedCode("OnEnumerateDescriptors adds DirectShow which requires unreferenced code. Use platform-specific Devices directly.")]
    internal IEnumerable<CaptureDeviceDescriptor> InternalEnumerateDescriptors() =>
        this.OnEnumerateDescriptors();
}
