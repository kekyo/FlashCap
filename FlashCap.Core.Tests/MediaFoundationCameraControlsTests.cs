////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal.MediaFoundation;
using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MediaFoundation = FlashCap.Internal.NativeMethods_MediaFoundation;

#pragma warning disable CA1416 // The fake COM objects do not invoke Windows APIs.

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed unsafe class MediaFoundationCameraControlsTests
{
    private struct Source
    {
        internal void** Vtable;
        internal Control* VideoProcAmp;
        internal Control* CameraControl;
    }

    private struct Control
    {
        internal void** Vtable;
        internal int Property;
        internal int References;
        internal int Minimum;
        internal int Maximum;
        internal int Caps;
        internal int Value;
        internal int Flags;
        internal int Result;
        internal int SetCount;
    }

    [TestCase(CaptureDeviceControlTypes.Brightness, false, 0)]
    [TestCase(CaptureDeviceControlTypes.Contrast, false, 1)]
    [TestCase(CaptureDeviceControlTypes.Hue, false, 2)]
    [TestCase(CaptureDeviceControlTypes.Saturation, false, 3)]
    [TestCase(CaptureDeviceControlTypes.Sharpness, false, 4)]
    [TestCase(CaptureDeviceControlTypes.Gamma, false, 5)]
    [TestCase(CaptureDeviceControlTypes.WhiteBalance, false, 7)]
    [TestCase(CaptureDeviceControlTypes.BacklightCompensation, false, 8)]
    [TestCase(CaptureDeviceControlTypes.Gain, false, 9)]
    [TestCase(CaptureDeviceControlTypes.Pan, true, 0)]
    [TestCase(CaptureDeviceControlTypes.Tilt, true, 1)]
    [TestCase(CaptureDeviceControlTypes.Zoom, true, 3)]
    [TestCase(CaptureDeviceControlTypes.Exposure, true, 4)]
    [TestCase(CaptureDeviceControlTypes.Focus, true, 6)]
    public void ExposesAndChangesTheSupportedNativeProperty(CaptureDeviceControlTypes type, bool camera, int property)
    {
        void** sourceVtable = stackalloc void*[3];
        sourceVtable[0] = (void*)(delegate* unmanaged[Stdcall]<Source*, Guid*, void**, int>)&QueryInterface;
        void** controlVtable = stackalloc void*[6];
        controlVtable[2] = (void*)(delegate* unmanaged[Stdcall]<Control*, uint>)&Release;
        controlVtable[3] = (void*)(delegate* unmanaged[Stdcall]<Control*, int, int*, int*, int*, int*, int*, int>)&GetRange;
        controlVtable[4] = (void*)(delegate* unmanaged[Stdcall]<Control*, int, int, int, int>)&Set;
        controlVtable[5] = (void*)(delegate* unmanaged[Stdcall]<Control*, int, int*, int*, int>)&Get;
        var control = new Control { Vtable = controlVtable, Property = property, Minimum = -10, Maximum = 10, Caps = 3, Value = -4, Flags = 2 };
        var source = new Source { Vtable = sourceVtable, VideoProcAmp = camera ? null : &control, CameraControl = camera ? &control : null };

        using (var controls = new CameraControls((MediaFoundation.IUnknown*)&source))
        {
            var ranges = controls.GetControls();
            Assert.That(ranges, Has.Length.EqualTo(1));
            Assert.That(ranges[0].Type, Is.EqualTo(type));
            Assert.That(ranges[0].Minimum, Is.EqualTo(-10));
            Assert.That(ranges[0].Maximum, Is.EqualTo(10));
            Assert.That(ranges[0].Step, Is.EqualTo(2));
            Assert.That(ranges[0].DefaultValue, Is.EqualTo(0));
            Assert.That(ranges[0].SupportsAuto, Is.True);
            Assert.That(controls.GetValue(type)!.Value, Is.EqualTo(-4));
            Assert.That(controls.GetValue(type)!.IsAuto, Is.False);
            Assert.That(controls.SetValue(type, 6, false), Is.True);
            Assert.That(control.Value, Is.EqualTo(6));
            Assert.That(control.Flags, Is.EqualTo(2));
            Assert.That(controls.SetValue(type, int.MaxValue, true), Is.True);
            Assert.That(control.Value, Is.EqualTo(6), "Auto must use an in-range value, ignoring the caller's value.");
            Assert.That(control.Flags, Is.EqualTo(1));
            Assert.That(controls.GetValue(type)!.IsAuto, Is.True);
            Assert.That(control.References, Is.EqualTo(1));
        }
        Assert.That(control.References, Is.Zero);
    }

    [Test]
    public void MissingInterfacesReturnUnsupportedResults()
    {
        void** vtable = stackalloc void*[3];
        vtable[0] = (void*)(delegate* unmanaged[Stdcall]<Source*, Guid*, void**, int>)&QueryInterface;
        var source = new Source { Vtable = vtable };
        using var controls = new CameraControls((MediaFoundation.IUnknown*)&source);
        Assert.That(controls.GetControls(), Is.Empty);
        Assert.That(controls.GetValue(CaptureDeviceControlTypes.Focus), Is.Null);
        Assert.That(controls.SetValue(CaptureDeviceControlTypes.Focus, 1, false), Is.False);
        Assert.That(controls.GetValue(CaptureDeviceControlTypes.PowerLineFrequency), Is.Null);
        Assert.That(controls.SetValue((CaptureDeviceControlTypes)999, 0, true), Is.False);
    }

    [Test]
    public void FailedNativeOperationsReturnUnsupportedResultsAndReleaseInterfaces()
    {
        void** sourceVtable = stackalloc void*[3];
        sourceVtable[0] = (void*)(delegate* unmanaged[Stdcall]<Source*, Guid*, void**, int>)&QueryInterface;
        void** controlVtable = stackalloc void*[6];
        controlVtable[2] = (void*)(delegate* unmanaged[Stdcall]<Control*, uint>)&Release;
        controlVtable[3] = (void*)(delegate* unmanaged[Stdcall]<Control*, int, int*, int*, int*, int*, int*, int>)&GetRange;
        controlVtable[4] = (void*)(delegate* unmanaged[Stdcall]<Control*, int, int, int, int>)&Set;
        controlVtable[5] = (void*)(delegate* unmanaged[Stdcall]<Control*, int, int*, int*, int>)&Get;
        var control = new Control { Vtable = controlVtable, Property = 4, Result = unchecked((int)0x80004005) };
        var source = new Source { Vtable = sourceVtable, CameraControl = &control };
        using (var controls = new CameraControls((MediaFoundation.IUnknown*)&source))
        {
            Assert.That(controls.GetControls(), Is.Empty);
            Assert.That(controls.GetValue(CaptureDeviceControlTypes.Exposure), Is.Null);
            Assert.That(controls.SetValue(CaptureDeviceControlTypes.Exposure, -4, false), Is.False);
            Assert.That(controls.SetValue(CaptureDeviceControlTypes.Exposure, 0, true), Is.False);
            Assert.That(control.SetCount, Is.EqualTo(1), "A failed Get must prevent an Auto Set with an invalid value.");
        }
        Assert.That(control.References, Is.Zero);
    }

    [TestCase(1, false)]
    [TestCase(2, false)]
    [TestCase(3, true)]
    public void AutoToggleRequiresBothManualAndAutomaticModes(int caps, bool supportsAuto)
    {
        void** sourceVtable = stackalloc void*[3];
        sourceVtable[0] = (void*)(delegate* unmanaged[Stdcall]<Source*, Guid*, void**, int>)&QueryInterface;
        void** controlVtable = stackalloc void*[6];
        controlVtable[2] = (void*)(delegate* unmanaged[Stdcall]<Control*, uint>)&Release;
        controlVtable[3] = (void*)(delegate* unmanaged[Stdcall]<Control*, int, int*, int*, int*, int*, int*, int>)&GetRange;
        var control = new Control { Vtable = controlVtable, Property = 0, Minimum = 0, Maximum = 10, Caps = caps };
        var source = new Source { Vtable = sourceVtable, VideoProcAmp = &control };
        using var controls = new CameraControls((MediaFoundation.IUnknown*)&source);
        Assert.That(controls.GetControls()[0].SupportsAuto, Is.EqualTo(supportsAuto));
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int QueryInterface(Source* source, Guid* iid, void** result)
    {
        var control = *iid == new Guid("C6E13360-30AC-11D0-A18C-00A0C9118956") ? source->VideoProcAmp :
            *iid == new Guid("C6E13370-30AC-11D0-A18C-00A0C9118956") ? source->CameraControl : null;
        *result = control;
        if (control is null)
        {
            return unchecked((int)0x80004002);
        }
        control->References++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static uint Release(Control* control) => (uint)--control->References;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int GetRange(Control* control, int property, int* min, int* max, int* step, int* defaultValue, int* caps)
    {
        *min = control->Minimum;
        *max = control->Maximum;
        *step = 2;
        *defaultValue = 0;
        *caps = control->Caps;
        return property == control->Property ? control->Result : unchecked((int)0x80070490);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int Get(Control* control, int property, int* value, int* flags)
    {
        *value = control->Value;
        *flags = control->Flags;
        return property == control->Property ? control->Result : unchecked((int)0x80070490);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int Set(Control* control, int property, int value, int flags)
    {
        control->SetCount++;
        control->Value = value;
        control->Flags = flags;
        return property == control->Property ? control->Result : unchecked((int)0x80070490);
    }
}
