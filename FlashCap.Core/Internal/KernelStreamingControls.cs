////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FlashCap.Internal;

/// <summary>
/// Owns an IKsControl reference for properties outside IAMVideoProcAmp/IAMCameraControl.
/// Calls and disposal must use the thread owning the capture source.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed unsafe class KernelStreamingControls : IDisposable
{
    private static readonly Guid videoSet = new("C6E13360-30AC-11D0-A18C-00A0C9118956");
    private static readonly Guid cameraSet = new("C6E13370-30AC-11D0-A18C-00A0C9118956");
    private void* native;

    // ks.h/ksmedia.h: KSPROPERTY followed by LONG Value, ULONG Flags and Capabilities.
    // KSIDENTIFIER's LONGLONG alignment makes the complete structure 40 bytes on Windows.
    // https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ksmedia/ns-ksmedia-ksproperty_videoprocamp_s
    [StructLayout(LayoutKind.Explicit, Size = 40)]
    private struct Request
    {
        [FieldOffset(0)] internal Guid Set;
        [FieldOffset(16)] internal uint Id;
        [FieldOffset(20)] internal uint Flags;
        [FieldOffset(24)] internal int Value;
    }

    internal KernelStreamingControls(IntPtr source)
    {
        var iid = new Guid("28F54685-06FD-11D2-B27A-00A0C9223196");
        void* result = null;
        var hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)(*(void***)source)[0])(
            (void*)source, &iid, &result);
        if (hr != unchecked((int)0x80004002)) // Optional IKsControl interface.
        {
            Marshal.ThrowExceptionForHR(hr);
            this.native = result;
        }
    }

    internal CaptureDeviceControl[] GetControls()
    {
        var controls = new List<CaptureDeviceControl>();
        foreach (var type in new[] { CaptureDeviceControlTypes.PowerLineFrequency, CaptureDeviceControlTypes.AutoExposurePriority })
        {
            TryMap(type, out var set, out var id);
            var support = this.ReadDescription(set, id, 0x200); // KSPROPERTY_TYPE_BASICSUPPORT
            if (support is null || (BitConverter.ToUInt32(support, 0) & 3) != 3 || this.GetValue(type) is null)
            {
                continue;
            }
            int min, max, step;
            if (type == CaptureDeviceControlTypes.AutoExposurePriority)
            {
                // The documented boolean values are authoritative. UVC drivers can return
                // a zero-filled range even while the current value is 1.
                // https://learn.microsoft.com/en-us/windows-hardware/drivers/stream/ksproperty-cameracontrol-auto-exposure-priority
                min = 0;
                max = 1;
                step = 1;
            }
            else if (!TryReadBounds(support, false, out min, out max, out step) || min < 0 || max > 3)
            {
                continue;
            }
            var defaults = this.ReadDescription(set, id, 0x10000); // KSPROPERTY_TYPE_DEFAULTVALUES
            var defaultValue = defaults is not null && TryReadBounds(defaults, true, out var value, out _, out _) &&
                value >= min && value <= max ? value : min;
            controls.Add(new CaptureDeviceControl(type, min, max, step, defaultValue, false));
        }
        return controls.ToArray();
    }

    internal CaptureDeviceControlValue? GetValue(CaptureDeviceControlTypes type)
    {
        if (!TryMap(type, out var set, out var id)) return null;
        var request = new Request { Set = set, Id = id, Flags = 1 };
        var data = new byte[40];
        fixed (byte* pointer = data) *(Request*)pointer = request;
        if (this.Invoke(ref request, data, out var returned) < 0 || returned < 36) return null;
        return new CaptureDeviceControlValue(BitConverter.ToInt32(data, 24), false);
    }

    internal bool SetValue(CaptureDeviceControlTypes type, int value, bool isAuto)
    {
        if (isAuto || !TryMap(type, out var set, out var id) || value < 0 ||
            value > (type == CaptureDeviceControlTypes.PowerLineFrequency ? 3 : 1)) return false;
        var request = new Request { Set = set, Id = id, Flags = 2, Value = value };
        var data = new byte[40];
        fixed (byte* pointer = data) *(Request*)pointer = request;
        return this.Invoke(ref request, data, out _) >= 0;
    }

    private static bool TryMap(CaptureDeviceControlTypes type, out Guid set, out uint id)
    {
        set = type == CaptureDeviceControlTypes.PowerLineFrequency ? videoSet : cameraSet;
        id = type == CaptureDeviceControlTypes.PowerLineFrequency ? 13u : 19u;
        return type is CaptureDeviceControlTypes.PowerLineFrequency or CaptureDeviceControlTypes.AutoExposurePriority;
    }

    private byte[]? ReadDescription(Guid set, uint id, uint flags)
    {
        var request = new Request { Set = set, Id = id, Flags = flags };
        var data = new byte[40];
        if (this.Invoke(ref request, data, out var returned) < 0 || returned < 4) return null;
        if (returned == 4) return data; // Access flags only; no range/default data.
        if (returned < 40) return null;
        var size = BitConverter.ToUInt32(data, 4);
        if (size < 40 || size > 65536) return null;
        if (size > data.Length)
        {
            data = new byte[size];
            if (this.Invoke(ref request, data, out returned) < 0 || returned < size || returned > data.Length) return null;
        }
        return data;
    }

    private static bool TryReadBounds(byte[] data, bool defaults, out int min, out int max, out int step)
    {
        min = max = 0;
        step = 1;
        var count = BitConverter.ToUInt32(data, 32);
        var offset = 40;
        for (uint index = 0; index < count; index++)
        {
            if (offset > data.Length - 16) return false;
            var kind = BitConverter.ToUInt32(data, offset);
            var size = BitConverter.ToUInt32(data, offset + 4);
            var members = BitConverter.ToUInt32(data, offset + 8);
            var flags = BitConverter.ToUInt32(data, offset + 12);
            offset += 16;
            var length = (ulong)size * members;
            if (length > (ulong)(data.Length - offset)) return false;
            if (members != 0 && (!defaults || (flags & 1) != 0))
            {
                if (!defaults && (kind == 1 || kind == 2) && size == 16)
                {
                    // Some UVC drivers tag KSPROPERTY_STEPPING_LONG as MEMBER_RANGES.
                    step = Math.Max(1, BitConverter.ToInt32(data, offset));
                    min = BitConverter.ToInt32(data, offset + 8);
                    max = BitConverter.ToInt32(data, offset + 12);
                    return max >= min;
                }
                if (!defaults && kind == 1 && size == 8)
                {
                    min = BitConverter.ToInt32(data, offset);
                    max = BitConverter.ToInt32(data, offset + 4);
                    return max >= min;
                }
                if (kind == 3 && size == 4)
                {
                    min = max = BitConverter.ToInt32(data, offset);
                    for (uint member = 1; !defaults && member < members; member++)
                    {
                        var value = BitConverter.ToInt32(data, offset + (int)member * 4);
                        min = Math.Min(min, value);
                        max = Math.Max(max, value);
                    }
                    return true;
                }
            }
            offset += (int)length;
        }
        return false;
    }

    // IKsControl::KsProperty is vtable slot 3, with 32-bit ULONG lengths on both x86/x64.
    // https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ksproxy/nf-ksproxy-ikscontrol-ksproperty
    private int Invoke(ref Request request, byte[] data, out uint returned)
    {
        returned = 0;
        if (this.native is null) return unchecked((int)0x80004002);
        fixed (Request* property = &request)
        fixed (byte* pointer = data)
        fixed (uint* size = &returned)
            return ((delegate* unmanaged[Stdcall]<void*, Request*, uint, byte*, uint, uint*, int>)(*(void***)this.native)[3])(
                this.native, property, 40, pointer, (uint)data.Length, size);
    }

    public void Dispose()
    {
        if (this.native is not null)
        {
            _ = ((delegate* unmanaged[Stdcall]<void*, uint>)(*(void***)this.native)[2])(this.native);
            this.native = null;
        }
    }
}
