////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;

namespace FlashCap.Devices;

#if NET6_0_OR_GREATER // Remove this when dropping net5.0. Its required because RequiresUnreferencedCode was not allowed on class level in net5.0
[RequiresUnreferencedCode("Direct show depends on COM runtime generation and might require unreferenced code.")]
#endif
[SupportedOSPlatform("windows")]
public sealed class DirectShowDevices : CaptureDevices
{
    public DirectShowDevices() :
        this(new DefaultBufferPool())
    {
    }

    public DirectShowDevices(BufferPool defaultBufferPool) :
        base(defaultBufferPool)
    {
    }

    protected override IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors() =>
        NativeMethods_DirectShow.EnumerateDeviceMoniker(
            NativeMethods_DirectShow.CLSID_VideoInputDeviceCategory).
        Collect(moniker => moniker.GetPropertyBag() is { } pb ?
            pb.SafeReleaseBlock(pb =>
                pb.GetValue("FriendlyName", default(string))?.Trim() is { } n &&
                (string.IsNullOrEmpty(n) ? "Unknown" : n!) is { } name &&
                pb.GetValue("DevicePath", default(string))?.Trim() is { } devicePath ?
                    (CaptureDeviceDescriptor)new DirectShowDeviceDescriptor(
                        devicePath, name,
                        pb.GetValue("Description", default(string))?.Trim() ?? $"{name} (DirectShow)",
                        moniker.BindToObject(
                            null, null, in NativeMethods_DirectShow.IID_IBaseFilter, out var cs) == 0 &&
                        cs is NativeMethods_DirectShow.IBaseFilter captureSource ?
                            captureSource.SafeReleaseBlock(
                                captureSource => captureSource.EnumeratePins().
                                Collect(pin =>
                                    pin.GetPinInfo() is { } pinInfo &&
                                    pinInfo.dir == NativeMethods_DirectShow.PIN_DIRECTION.Output ?
                                        pin : null).
                                SelectMany(pin =>
                                    pin.EnumerateFormats().
                                    Collect(format => format.CreateVideoCharacteristics())).
                                Distinct().
                                OrderByDescending(vc => vc).
                                ToArray()) :
                            ArrayEx.Empty<VideoCharacteristics>(),
                        this.DefaultBufferPool) :
                    null) :
            null);
}
