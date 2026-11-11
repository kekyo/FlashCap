////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal.PipeWire;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed class PipeWireControlTests
{
    private const uint Brightness = 0x20001, Exposure = 0x19a0902, ExposureAuto = 0x19a0901;
    private const uint Focus = 0x19a090a, FocusAuto = 0x19a090c;

    // POD values are constructed independently, using the public native SPA layout.
    private static byte[] Info(uint id, uint type, uint choice, params int[] values)
    {
        var words = new uint[] { 0, 15, 0x40001, 1, 1, 0, 4, 3, id, 0,
            3, 0, (uint)(16 + values.Length * 4), 19, choice, 0, 4, type }
            .Concat(values.Select(value => unchecked((uint)value))).ToList();
        if ((words.Count & 1) != 0) words.Add(0);
        words[0] = (uint)words.Count * 4 - 8;
        return words.SelectMany(BitConverter.GetBytes).ToArray();
    }

    private static byte[] Props(uint id, uint type, int value, uint flags = 0) =>
        new uint[] { 32, 15, 0x40002, 2, id, flags, 4, type, unchecked((uint)value), 0 }
            .SelectMany(BitConverter.GetBytes).ToArray();

    [Test]
    public void IntegerControlsPreserveSignedNativeRangesAndRejectInvalidRequests()
    {
        var controls = new PipeWireControls(false);
        controls.AddInfo(Info(Brightness, 4, 2, 0, -64, 64, 2));
        controls.SetValues(new[] { Props(Brightness, 4, -12) });
        var range = controls.GetControls().Single();
        Assert.That((range.Minimum, range.Maximum, range.Step, range.DefaultValue), Is.EqualTo((-64, 64, 2, 0)));
        Assert.That(controls.GetValue(range.Type)!.Value, Is.EqualTo(-12));
        Assert.That(controls.CreateWrites(range.Type, 65, false), Is.Null);
        Assert.That(controls.CreateWrites(range.Type, -63, false), Is.Null);
        Assert.That(controls.CreateWrites(range.Type, 0, true), Is.Null);
        Assert.That(controls.CreateWrites(range.Type, -10, false)!.Single(), Is.EqualTo(Props(Brightness, 4, -10)));
        controls.SetValues(new[] { Props(Brightness, 4, -12, 1) });
        Assert.That(controls.CreateWrites(range.Type, -10, false), Is.Null);
    }

    [Test]
    public void FloatingPointControlsUseThousandthsAndKeepTheNativeValueType()
    {
        var controls = new PipeWireControls(false);
        var numbers = new[] { 0.5f, -1f, 1f }.Select(BitConverter.SingleToInt32Bits).ToArray();
        controls.AddInfo(Info(Brightness, 6, 1, numbers));
        controls.SetValues(new[] { Props(Brightness, 6, BitConverter.SingleToInt32Bits(0.25f)) });
        var range = controls.GetControls().Single();
        Assert.That((range.Minimum, range.Maximum, range.Step, range.DefaultValue), Is.EqualTo((-1000, 1000, 1, 500)));
        Assert.That(controls.GetValue(range.Type)!.Value, Is.EqualTo(250));
        var write = controls.CreateWrites(range.Type, -125, false)!.Single();
        Assert.That(write, Is.EqualTo(Props(Brightness, 6, BitConverter.SingleToInt32Bits(-0.125f))));
        Assert.That(controls.Matches(write), Is.False);
        controls.SetValues(new[] { write });
        Assert.That(controls.Matches(write), Is.True);
    }

    [Test]
    public void V4L2AutomaticFocusIsSwitchedBeforeItsManualValue()
    {
        var controls = new PipeWireControls(true);
        controls.AddInfo(Info(Focus, 4, 2, 0, 0, 255, 5));
        controls.AddInfo(Info(FocusAuto, 2, 3, 1, 1, 0));
        controls.SetValues(new[] { Props(Focus, 4, 0), Props(FocusAuto, 2, 1) });
        Assert.That(controls.GetControls().Single().SupportsAuto, Is.True);
        Assert.That(controls.GetValue(CaptureDeviceControlTypes.Focus)!.IsAuto, Is.True);
        Assert.That(controls.CreateWrites(CaptureDeviceControlTypes.Focus, 25, false),
            Is.EqualTo(new[] { Props(FocusAuto, 2, 0), Props(Focus, 4, 25) }));
        Assert.That(controls.CreateWrites(CaptureDeviceControlTypes.Focus, 23, false), Is.Null);
        controls.SetValues(new[] { Props(Focus, 4, 0) });
        Assert.That(controls.CreateWrites(CaptureDeviceControlTypes.Focus, 25, false), Is.Null);
    }

    [Test]
    public void V4L2ExposureMenuCanBeProvidedOnlyThroughLabels()
    {
        var controls = new PipeWireControls(true);
        controls.AddInfo(Info(Exposure, 4, 2, 250, 3, 2047, 1));
        var menu = Info(ExposureAuto, 4, 3, 3).Concat(new uint[] {
            4, 0, 64, 14, 4, 4, 1, 0, 2, 8, 65, 0, 4, 4, 3, 0, 2, 8, 66, 0,
        }.SelectMany(BitConverter.GetBytes)).ToArray();
        BitConverter.GetBytes(menu.Length - 8).CopyTo(menu, 0);
        controls.AddInfo(menu);
        controls.SetValues(new[] { Props(Exposure, 4, 250), Props(ExposureAuto, 4, 3) });
        Assert.That(controls.GetControls().Single().SupportsAuto, Is.True);
        Assert.That(controls.CreateWrites(CaptureDeviceControlTypes.Exposure, 300, false),
            Is.EqualTo(new[] { Props(ExposureAuto, 4, 1), Props(Exposure, 4, 300) }));
        controls.SetValues(new[] { Props(Exposure, 4, 300), Props(ExposureAuto, 4, 1) });
        Assert.That(controls.CreateWrites(CaptureDeviceControlTypes.Exposure, 300, true),
            Is.EqualTo(new[] { Props(ExposureAuto, 4, 3) }));
    }

    [Test]
    public void ProviderPrivateControlsAreNotMisidentifiedAsV4L2()
    {
        var controls = new PipeWireControls(false);
        controls.AddInfo(Info(Focus, 4, 2, 0, 0, 255, 5));
        controls.SetValues(new[] { Props(Focus, 4, 0) });
        Assert.That(controls.GetControls(), Is.Empty);
        Assert.That(controls.GetValue(CaptureDeviceControlTypes.Focus), Is.Null);
        Assert.That(controls.CreateWrites(CaptureDeviceControlTypes.Focus, 25, false), Is.Null);
    }

    [Test]
    public void UnavailableReadbackCannotVerifyAWrite()
    {
        var controls = new PipeWireControls(false);
        controls.AddInfo(Info(Brightness, 4, 2, 128, 0, 255, 1));
        controls.SetValues(Array.Empty<byte[]>());
        Assert.That(controls.GetValue(CaptureDeviceControlTypes.Brightness), Is.Null);
        Assert.That(controls.CreateWrites(CaptureDeviceControlTypes.Brightness, 128, false), Is.Null);
        Assert.That(controls.Matches(Props(Brightness, 4, 128)), Is.False);
    }

    [TestCase(0, 1)]
    [TestCase(100, 0)]
    [TestCase(100, -1)]
    public void InvalidRangesAreOmitted(int maximum, int step)
    {
        var controls = new PipeWireControls(false);
        controls.AddInfo(Info(Brightness, 4, 2, 50, 1, maximum, step));
        Assert.That(controls.GetControls(), Is.Empty);
    }

    [Test]
    public void TruncatedControlPodsAreRejectedBeforeAccessingTheirValues()
    {
        var controls = new PipeWireControls(false);
        Assert.Throws<InvalidDataException>(() => controls.AddInfo(Info(Brightness, 4, 2, 0, 0, 100, 1).Take(30).ToArray()));
        Assert.Throws<InvalidDataException>(() => controls.SetValues(new[] { new byte[16] }));
    }
}
