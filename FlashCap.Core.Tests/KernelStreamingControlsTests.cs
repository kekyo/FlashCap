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
using System.Runtime.InteropServices;
using FlashCap.Internal;
using NUnit.Framework;

#pragma warning disable CA1416 // Fake COM objects exercise the ABI without Windows.

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed unsafe class KernelStreamingControlsTests
{
    private struct Control
    {
        internal void** Vtable;
        internal Guid Set;
        internal uint Property;
        internal int References;
        internal int Value;
        internal int Mode;
        internal int SetCount;
    }

    [TestCase(CaptureDeviceControlTypes.PowerLineFrequency, 13u, 2, 1)]
    [TestCase(CaptureDeviceControlTypes.AutoExposurePriority, 19u, 0, 1)]
    public void ExposesNativeDefaultsAndChangesValuesWithoutAnAutoFlag(
        CaptureDeviceControlTypes type, uint property, int expectedDefault, int changed)
    {
        void** vtable = stackalloc void*[4];
        vtable[0] = (void*)(delegate* unmanaged[Stdcall]<Control*, Guid*, void**, int>)&Query;
        vtable[2] = (void*)(delegate* unmanaged[Stdcall]<Control*, uint>)&Release;
        vtable[3] = (void*)(delegate* unmanaged[Stdcall]<Control*, byte*, uint, byte*, uint, uint*, int>)&Property;
        var control = new Control { Vtable = vtable, Property = property, Value = expectedDefault,
            Set = new Guid(property == 13 ? "C6E13360-30AC-11D0-A18C-00A0C9118956" : "C6E13370-30AC-11D0-A18C-00A0C9118956") };
        using (var controls = new KernelStreamingControls((IntPtr)(&control)))
        {
            var ranges = controls.GetControls();
            Assert.That(ranges, Has.Length.EqualTo(1));
            Assert.That(ranges[0].Type, Is.EqualTo(type));
            Assert.That(ranges[0].Minimum, Is.EqualTo(property == 13 ? 1 : 0));
            Assert.That(ranges[0].Maximum, Is.EqualTo(property == 13 ? 2 : 1));
            Assert.That(ranges[0].DefaultValue, Is.EqualTo(expectedDefault));
            Assert.That(ranges[0].SupportsAuto, Is.False);
            Assert.That(controls.GetValue(type)!.Value, Is.EqualTo(expectedDefault));
            Assert.That(controls.SetValue(type, changed, false), Is.True);
            Assert.That(controls.GetValue(type)!.Value, Is.EqualTo(changed));
            Assert.That(controls.GetValue(type)!.IsAuto, Is.False);
            Assert.That(controls.SetValue(type, 0, true), Is.False);
            Assert.That(controls.SetValue(type, 99, false), Is.False);
            Assert.That(control.SetCount, Is.EqualTo(1));
            Assert.That(control.References, Is.EqualTo(1));
        }
        Assert.That(control.References, Is.Zero);
    }

    [TestCase(1)] // Driver operation failure.
    [TestCase(2)] // Read-only property.
    [TestCase(3)] // Oversized description.
    [TestCase(4)] // Truncated members.
    [TestCase(5)] // Unsupported interface.
    public void UnusablePropertiesAreNotAdvertised(int mode)
    {
        void** vtable = stackalloc void*[4];
        vtable[0] = (void*)(delegate* unmanaged[Stdcall]<Control*, Guid*, void**, int>)&Query;
        vtable[2] = (void*)(delegate* unmanaged[Stdcall]<Control*, uint>)&Release;
        vtable[3] = (void*)(delegate* unmanaged[Stdcall]<Control*, byte*, uint, byte*, uint, uint*, int>)&Property;
        var control = new Control { Vtable = vtable, Mode = mode, Property = 13,
            Set = new Guid("C6E13360-30AC-11D0-A18C-00A0C9118956") };
        using (var controls = new KernelStreamingControls((IntPtr)(&control)))
        {
            Assert.That(controls.GetControls(), Is.Empty);
            Assert.That(controls.GetValue(CaptureDeviceControlTypes.Iris), Is.Null);
            Assert.That(controls.SetValue(CaptureDeviceControlTypes.Iris, 0, false), Is.False);
            if (mode is 1 or 5)
            {
                Assert.That(controls.GetValue(CaptureDeviceControlTypes.PowerLineFrequency), Is.Null);
                Assert.That(controls.SetValue(CaptureDeviceControlTypes.PowerLineFrequency, 1, false), Is.False);
            }
        }
        Assert.That(control.References, Is.Zero);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int Query(Control* self, Guid* iid, void** value)
    {
        *value = null;
        if (self->Mode == 5 || *iid != new Guid("28F54685-06FD-11D2-B27A-00A0C9223196")) return unchecked((int)0x80004002);
        *value = self;
        self->References++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static uint Release(Control* self) => (uint)--self->References;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int Property(Control* self, byte* request, uint requestSize, byte* data, uint size, uint* returned)
    {
        *returned = 0;
        if (requestSize < 24 || *(Guid*)request != self->Set || *(uint*)(request + 16) != self->Property || self->Mode == 1)
            return unchecked((int)0x80070490);
        var flags = *(uint*)(request + 20);
        if (flags == 0x200 || flags == 0x10000)
        {
            if (size < 40) return unchecked((int)0x8007007A);
            var words = (uint*)data;
            words[0] = self->Mode == 2 ? 1u : 3u;
            words[1] = self->Mode == 3 ? uint.MaxValue : flags == 0x200 ? 72u : 60u;
            words[8] = 1;
            *returned = Math.Min(size, words[1]);
            if (size >= words[1])
            {
                // UVC reports a STEPPING_LONG payload tagged as MEMBER_RANGES.
                words[10] = flags == 0x200 ? 1u : 3u;
                words[11] = flags == 0x200 ? 16u : 4u;
                words[12] = 1;
                words[13] = flags == 0x10000 ? 1u : 0u;
                words[14] = flags == 0x10000 ? self->Property == 13 ? 2u : 0u : 1u;
                if (flags == 0x200)
                {
                    // Real cameras report a 0..0 range for the boolean exposure-priority control.
                    words[16] = self->Property == 13 ? 1u : 0u;
                    words[17] = self->Property == 13 ? 2u : 0u;
                }
                if (self->Mode == 4) *returned = 50;
            }
            return 0;
        }
        if (requestSize != 40 || size != 40) return unchecked((int)0x80070057);
        if (flags == 1)
        {
            *(int*)(data + 24) = self->Value;
            *returned = 40;
            return 0;
        }
        if (flags == 2)
        {
            if (*(uint*)(data + 28) != 0 || *(uint*)(data + 20) != 2) return unchecked((int)0x80070057);
            self->Value = *(int*)(data + 24);
            self->SetCount++;
            *returned = 40;
            return 0;
        }
        return unchecked((int)0x80070057);
    }
}
