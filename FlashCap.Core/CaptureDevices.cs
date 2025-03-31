////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
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
using System.Linq;
using System.Runtime.CompilerServices;

namespace FlashCap;

public class CaptureDevices
{
    protected readonly BufferPool DefaultBufferPool;

    public CaptureDevices() :
        this(new DefaultBufferPool())
    {
    }
    
    public CaptureDevices(BufferPool defaultBufferPool) =>
        this.DefaultBufferPool = defaultBufferPool;

    protected virtual IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors() =>
        NativeMethods.CurrentPlatform switch
        {
            NativeMethods.Platforms.Windows =>
                new DirectShowDevices(this.DefaultBufferPool).OnEnumerateDescriptors().
                Concat(new VideoForWindowsDevices(this.DefaultBufferPool).OnEnumerateDescriptors()),
            NativeMethods.Platforms.Linux =>
                new V4L2Devices().OnEnumerateDescriptors(),
            NativeMethods.Platforms.MacOS =>
                new AVFoundationDevices().OnEnumerateDescriptors(),
            _ =>
                ArrayEx.Empty<CaptureDeviceDescriptor>(),
        };

    internal IEnumerable<CaptureDeviceDescriptor> InternalEnumerateDescriptors() =>
        this.OnEnumerateDescriptors();
}
