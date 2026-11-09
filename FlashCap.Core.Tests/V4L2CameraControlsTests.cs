////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using FlashCap.Internal;
using NUnit.Framework;
using static FlashCap.Internal.NativeMethods_V4L2;

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed class V4L2CameraControlsTests
{
    private const uint Iris = 0x009a0911;
    private const uint Priority = 0x009a0903;
    private const uint ColorEffect = 0x0098091f;

    private sealed class Camera
    {
        internal readonly Dictionary<uint, v4l2_queryctrl> Ranges = new();
        internal readonly Dictionary<uint, int> Values = new();
        internal readonly HashSet<(uint, int)> Menus = new();
        internal uint Refused;

        internal void Add(uint id, int min, int max, int value)
        {
            this.Ranges.Add(id, new v4l2_queryctrl { id = id, minimum = min, maximum = max, step = 1, default_value = value });
            this.Values.Add(id, value);
        }

        internal V4L2CameraControls Create() => new(
            id => this.Ranges.TryGetValue(id, out var range) ? range : null,
            id => this.Values.TryGetValue(id, out var value) ? value : null,
            (id, value) =>
            {
                if (id == this.Refused || !this.Values.ContainsKey(id)) return false;
                this.Values[id] = value;
                return true;
            },
            (id, value) => this.Menus.Contains((id, value)));
    }

    [TestCase(CaptureDeviceControlTypes.Brightness, 0x00980900u)]
    [TestCase(CaptureDeviceControlTypes.Contrast, 0x00980901u)]
    [TestCase(CaptureDeviceControlTypes.Saturation, 0x00980902u)]
    [TestCase(CaptureDeviceControlTypes.Hue, 0x00980903u)]
    [TestCase(CaptureDeviceControlTypes.Gamma, 0x00980910u)]
    [TestCase(CaptureDeviceControlTypes.Gain, 0x00980913u)]
    [TestCase(CaptureDeviceControlTypes.PowerLineFrequency, 0x00980918u)]
    [TestCase(CaptureDeviceControlTypes.WhiteBalance, 0x0098091au)]
    [TestCase(CaptureDeviceControlTypes.Sharpness, 0x0098091bu)]
    [TestCase(CaptureDeviceControlTypes.BacklightCompensation, 0x0098091cu)]
    [TestCase(CaptureDeviceControlTypes.Exposure, 0x009a0902u)]
    [TestCase(CaptureDeviceControlTypes.Pan, 0x009a0908u)]
    [TestCase(CaptureDeviceControlTypes.Tilt, 0x009a0909u)]
    [TestCase(CaptureDeviceControlTypes.Focus, 0x009a090au)]
    [TestCase(CaptureDeviceControlTypes.Zoom, 0x009a090du)]
    [TestCase(CaptureDeviceControlTypes.Iris, Iris)]
    [TestCase(CaptureDeviceControlTypes.AutoExposurePriority, Priority)]
    public void AdvertisesAndChangesOnlySupportedControls(CaptureDeviceControlTypes type, uint id)
    {
        var camera = new Camera();
        camera.Add(id, 0, 1, 0);
        var controls = camera.Create();
        var range = controls.GetControls().Single();
        Assert.That(range.Type, Is.EqualTo(type));
        Assert.That(range.Minimum, Is.Zero);
        Assert.That(range.Maximum, Is.EqualTo(1));
        Assert.That(range.SupportsAuto, Is.False);
        Assert.That(controls.GetValue(type)!.Value, Is.Zero);
        Assert.That(controls.SetValue(type, 1, false), Is.True);
        Assert.That(controls.GetValue(type)!.Value, Is.EqualTo(1));
        Assert.That(controls.SetValue(type, 0, true), Is.False);
        Assert.That(controls.GetValue(CaptureDeviceControlTypes.Roll), Is.Null);
        Assert.That(controls.SetValue(CaptureDeviceControlTypes.Roll, 0, false), Is.False);
    }

    [TestCase(0, true, true)]
    [TestCase(1, false, false)]
    [TestCase(2, false, true)]
    [TestCase(3, true, false)]
    public void ReportsExposureAndIrisAutomaticModesIndividually(int mode, bool exposureAuto, bool irisAuto)
    {
        var camera = new Camera();
        camera.Add(V4L2_CID_EXPOSURE_ABSOLUTE, 1, 1000, 100);
        camera.Add(Iris, 1, 100, 20);
        camera.Add(V4L2_CID_EXPOSURE_AUTO, 0, 3, mode);
        var controls = camera.Create();
        Assert.That(controls.GetValue(CaptureDeviceControlTypes.Exposure)!.IsAuto, Is.EqualTo(exposureAuto));
        Assert.That(controls.GetValue(CaptureDeviceControlTypes.Iris)!.IsAuto, Is.EqualTo(irisAuto));
    }

    [TestCase(CaptureDeviceControlTypes.Exposure, 0, false, 2)]
    [TestCase(CaptureDeviceControlTypes.Exposure, 1, false, 1)]
    [TestCase(CaptureDeviceControlTypes.Exposure, 2, false, 2)]
    [TestCase(CaptureDeviceControlTypes.Exposure, 3, false, 1)]
    [TestCase(CaptureDeviceControlTypes.Exposure, 0, true, 0)]
    [TestCase(CaptureDeviceControlTypes.Exposure, 1, true, 3)]
    [TestCase(CaptureDeviceControlTypes.Exposure, 2, true, 0)]
    [TestCase(CaptureDeviceControlTypes.Exposure, 3, true, 3)]
    [TestCase(CaptureDeviceControlTypes.Iris, 0, false, 3)]
    [TestCase(CaptureDeviceControlTypes.Iris, 1, false, 1)]
    [TestCase(CaptureDeviceControlTypes.Iris, 2, false, 1)]
    [TestCase(CaptureDeviceControlTypes.Iris, 3, false, 3)]
    [TestCase(CaptureDeviceControlTypes.Iris, 0, true, 0)]
    [TestCase(CaptureDeviceControlTypes.Iris, 1, true, 2)]
    [TestCase(CaptureDeviceControlTypes.Iris, 2, true, 2)]
    [TestCase(CaptureDeviceControlTypes.Iris, 3, true, 0)]
    public void ChangingOneAutomaticModePreservesTheOtherWhenSupported(
        CaptureDeviceControlTypes type, int initialMode, bool automatic, int expectedMode)
    {
        var camera = new Camera();
        camera.Add(V4L2_CID_EXPOSURE_ABSOLUTE, 1, 1000, 100);
        camera.Add(Iris, 1, 100, 20);
        camera.Add(V4L2_CID_EXPOSURE_AUTO, 0, 3, initialMode);
        foreach (var mode in new[] { 0, 1, 2, 3 }) camera.Menus.Add((V4L2_CID_EXPOSURE_AUTO, mode));
        var controls = camera.Create();
        Assert.That(controls.GetControls().All(control => control.SupportsAuto), Is.True);
        Assert.That(controls.SetValue(type, automatic ? int.MaxValue : 30, automatic), Is.True);
        Assert.That(camera.Values[V4L2_CID_EXPOSURE_AUTO], Is.EqualTo(expectedMode));
        Assert.That(controls.GetValue(type)!.IsAuto, Is.EqualTo(automatic));
        Assert.That(controls.GetValue(type)!.Value, Is.EqualTo(automatic ? type == CaptureDeviceControlTypes.Iris ? 20 : 100 : 30));
    }

    [TestCase(3)] // Aperture-priority webcams.
    [TestCase(0)] // Devices that couple both automatic modes.
    public void UsesAvailableExposureMenuItems(int automaticMode)
    {
        var camera = new Camera();
        camera.Add(V4L2_CID_EXPOSURE_ABSOLUTE, 1, 1000, 100);
        camera.Add(V4L2_CID_EXPOSURE_AUTO, 0, 3, 1);
        camera.Menus.UnionWith(new[] { (V4L2_CID_EXPOSURE_AUTO, 1), (V4L2_CID_EXPOSURE_AUTO, automaticMode) });
        var controls = camera.Create();
        Assert.That(controls.GetControls().Single().SupportsAuto, Is.True);
        Assert.That(controls.SetValue(CaptureDeviceControlTypes.Exposure, 0, true), Is.True);
        Assert.That(camera.Values[V4L2_CID_EXPOSURE_AUTO], Is.EqualTo(automaticMode));
        Assert.That(controls.SetValue(CaptureDeviceControlTypes.Exposure, 50, false), Is.True);
        Assert.That(camera.Values[V4L2_CID_EXPOSURE_AUTO], Is.EqualTo(1));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void FailedManualModeSwitchDoesNotWriteAnIgnoredValue(bool exposure)
    {
        var camera = new Camera();
        var id = exposure ? V4L2_CID_EXPOSURE_ABSOLUTE : V4L2_CID_FOCUS_ABSOLUTE;
        var autoId = exposure ? V4L2_CID_EXPOSURE_AUTO : V4L2_CID_FOCUS_AUTO;
        camera.Add(id, 1, 100, 20);
        camera.Add(autoId, 0, 3, exposure ? 3 : 1);
        camera.Menus.UnionWith(new[] { (autoId, 1), (autoId, 3) });
        camera.Refused = autoId;
        Assert.That(camera.Create().SetValue(exposure ? CaptureDeviceControlTypes.Exposure : CaptureDeviceControlTypes.Focus, 40, false), Is.False);
        Assert.That(camera.Values[id], Is.EqualTo(20));
    }

    [Test]
    public void UnsupportedIrisCannotChangeTheExposureMode()
    {
        var camera = new Camera();
        camera.Add(V4L2_CID_EXPOSURE_ABSOLUTE, 1, 1000, 100);
        camera.Add(V4L2_CID_EXPOSURE_AUTO, 0, 3, 1);
        foreach (var mode in new[] { 0, 1, 2, 3 }) camera.Menus.Add((V4L2_CID_EXPOSURE_AUTO, mode));
        Assert.That(camera.Create().SetValue(CaptureDeviceControlTypes.Iris, 0, true), Is.False);
        Assert.That(camera.Values[V4L2_CID_EXPOSURE_AUTO], Is.EqualTo(1));
    }

    [Test]
    public void AutomaticModeRequiresBothAutomaticAndManualChoices()
    {
        var camera = new Camera();
        camera.Add(Iris, 1, 100, 20);
        camera.Add(V4L2_CID_EXPOSURE_AUTO, 0, 3, 1);
        camera.Menus.UnionWith(new[] { (V4L2_CID_EXPOSURE_AUTO, 1), (V4L2_CID_EXPOSURE_AUTO, 3) });
        Assert.That(camera.Create().GetControls().Single().SupportsAuto, Is.False);
        Assert.That(camera.Create().SetValue(CaptureDeviceControlTypes.Iris, 0, true), Is.False);
    }

    [Test]
    public void ColorEnableMapsOnlyNoneAndBlackAndWhiteEffects()
    {
        var camera = new Camera();
        camera.Add(ColorEffect, 0, 2, 0);
        camera.Menus.UnionWith(new[] { (ColorEffect, 0), (ColorEffect, 1), (ColorEffect, 2) });
        var controls = camera.Create();
        var range = controls.GetControls().Single();
        Assert.That(range.Type, Is.EqualTo(CaptureDeviceControlTypes.ColorEnable));
        Assert.That(range.Minimum, Is.Zero);
        Assert.That(range.Maximum, Is.EqualTo(1));
        Assert.That(range.DefaultValue, Is.EqualTo(1));
        Assert.That(controls.GetValue(range.Type)!.Value, Is.EqualTo(1));
        Assert.That(controls.SetValue(range.Type, 0, false), Is.True);
        Assert.That(camera.Values[ColorEffect], Is.EqualTo(1));
        camera.Values[ColorEffect] = 2; // Sepia is neither of the two exposed modes.
        Assert.That(controls.GetValue(range.Type), Is.Null);
        Assert.That(controls.SetValue(range.Type, 2, false), Is.False);
        Assert.That(controls.SetValue(range.Type, 1, false), Is.True);
        Assert.That(camera.Values[ColorEffect], Is.Zero);
    }

    [TestCase(0)]
    [TestCase(1)]
    public void ColorEnableRequiresBothColorAndMonochrome(int onlyMode)
    {
        var camera = new Camera();
        camera.Add(ColorEffect, 0, 2, onlyMode);
        camera.Menus.Add((ColorEffect, onlyMode));
        var controls = camera.Create();
        Assert.That(controls.GetControls(), Is.Empty);
        Assert.That(controls.GetValue(CaptureDeviceControlTypes.ColorEnable), Is.Null);
        Assert.That(controls.SetValue(CaptureDeviceControlTypes.ColorEnable, 1, false), Is.False);
    }
}
