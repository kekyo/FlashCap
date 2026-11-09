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
using static FlashCap.Internal.NativeMethods_V4L2;

namespace FlashCap.Internal;

// Camera controls implementation.
// https://www.kernel.org/doc/html/latest/userspace-api/media/v4l/control.html
internal sealed class V4L2CameraControls
{
    private sealed class ControlMapping
    {
        public readonly uint Id;
        // Control switching the automatic mode, 0 without it.
        public readonly uint AutoId;

        public ControlMapping(uint id, uint autoId = 0)
        {
            this.Id = id;
            this.AutoId = autoId;
        }
    }

    private static readonly Dictionary<CaptureDeviceControlTypes, ControlMapping> mappings = new()
    {
        { CaptureDeviceControlTypes.Brightness, new(V4L2_CID_BRIGHTNESS) },
        { CaptureDeviceControlTypes.Contrast, new(V4L2_CID_CONTRAST) },
        { CaptureDeviceControlTypes.Hue, new(V4L2_CID_HUE, V4L2_CID_HUE_AUTO) },
        { CaptureDeviceControlTypes.Saturation, new(V4L2_CID_SATURATION) },
        { CaptureDeviceControlTypes.Sharpness, new(V4L2_CID_SHARPNESS) },
        { CaptureDeviceControlTypes.Gamma, new(V4L2_CID_GAMMA) },
        { CaptureDeviceControlTypes.WhiteBalance, new(V4L2_CID_WHITE_BALANCE_TEMPERATURE, V4L2_CID_AUTO_WHITE_BALANCE) },
        { CaptureDeviceControlTypes.BacklightCompensation, new(V4L2_CID_BACKLIGHT_COMPENSATION) },
        { CaptureDeviceControlTypes.Gain, new(V4L2_CID_GAIN, V4L2_CID_AUTOGAIN) },
        { CaptureDeviceControlTypes.PowerLineFrequency, new(V4L2_CID_POWER_LINE_FREQUENCY) },
        { CaptureDeviceControlTypes.Pan, new(V4L2_CID_PAN_ABSOLUTE) },
        { CaptureDeviceControlTypes.Tilt, new(V4L2_CID_TILT_ABSOLUTE) },
        { CaptureDeviceControlTypes.Zoom, new(V4L2_CID_ZOOM_ABSOLUTE) },
        { CaptureDeviceControlTypes.Exposure, new(V4L2_CID_EXPOSURE_ABSOLUTE, V4L2_CID_EXPOSURE_AUTO) },
        { CaptureDeviceControlTypes.Focus, new(V4L2_CID_FOCUS_ABSOLUTE, V4L2_CID_FOCUS_AUTO) },
        { CaptureDeviceControlTypes.Iris, new(V4L2_CID_IRIS_ABSOLUTE, V4L2_CID_EXPOSURE_AUTO) },
        { CaptureDeviceControlTypes.AutoExposurePriority, new(V4L2_CID_EXPOSURE_AUTO_PRIORITY) },
        { CaptureDeviceControlTypes.ColorEnable, new(V4L2_CID_COLORFX) },
    };

    private readonly Func<uint, v4l2_queryctrl?> query;
    private readonly Func<uint, int?> get;
    private readonly Func<uint, int, bool> set;
    private readonly Func<uint, int, bool> menu;

    internal V4L2CameraControls(Func<uint, v4l2_queryctrl?> query, Func<uint, int?> get,
        Func<uint, int, bool> set, Func<uint, int, bool> menu)
    {
        this.query = query;
        this.get = get;
        this.set = set;
        this.menu = menu;
    }

    internal CaptureDeviceControl[] GetControls()
    {
        var controls = new List<CaptureDeviceControl>();
        foreach (var entry in mappings)
        {
            if (this.query(entry.Value.Id) is not { } range) continue;
            if (entry.Key == CaptureDeviceControlTypes.ColorEnable)
            {
                if (this.SupportsColorEnable())
                    controls.Add(new CaptureDeviceControl(entry.Key, 0, 1, 1, range.default_value == V4L2_COLORFX_NONE ? 1 : 0, false));
                continue;
            }
            var supportsAuto = entry.Value.AutoId != 0 && this.query(entry.Value.AutoId) is { } automatic &&
                (entry.Value.AutoId == V4L2_CID_EXPOSURE_AUTO ?
                    this.HasExposureMode(entry.Key, true) && this.HasExposureMode(entry.Key, false) :
                    automatic.minimum <= 0 && automatic.maximum >= 1);
            controls.Add(new CaptureDeviceControl(entry.Key, range.minimum, range.maximum, range.step, range.default_value, supportsAuto));
        }
        return controls.ToArray();
    }

    internal CaptureDeviceControlValue? GetValue(CaptureDeviceControlTypes type)
    {
        if (!mappings.TryGetValue(type, out var mapping) || this.get(mapping.Id) is not { } value) return null;
        if (type == CaptureDeviceControlTypes.ColorEnable)
            return this.SupportsColorEnable() && (value == V4L2_COLORFX_NONE || value == V4L2_COLORFX_BW) ?
                new CaptureDeviceControlValue(value == V4L2_COLORFX_NONE ? 1 : 0, false) : null;
        var isAuto = mapping.AutoId != 0 && this.get(mapping.AutoId) is { } mode &&
            (mapping.AutoId == V4L2_CID_EXPOSURE_AUTO ? IsExposureAuto(type, mode) : mode != 0);
        return new CaptureDeviceControlValue(value, isAuto);
    }

    internal bool SetValue(CaptureDeviceControlTypes type, int value, bool isAuto)
    {
        if (!mappings.TryGetValue(type, out var mapping) || this.query(mapping.Id) is null) return false;
        if (type == CaptureDeviceControlTypes.ColorEnable)
            return !isAuto && (value == 0 || value == 1) && this.SupportsColorEnable() &&
                this.set(mapping.Id, value == 1 ? V4L2_COLORFX_NONE : V4L2_COLORFX_BW);
        if (mapping.AutoId == 0 || this.query(mapping.AutoId) is null)
            return !isAuto && this.set(mapping.Id, value);

        if (this.get(mapping.AutoId) is not { } current) return false;
        var desired = isAuto ? 1 : 0;
        if (mapping.AutoId == V4L2_CID_EXPOSURE_AUTO)
        {
            if (current < V4L2_EXPOSURE_AUTO || current > V4L2_EXPOSURE_APERTURE_PRIORITY) return false;
            // Exposure and iris share one mode. Preserve the companion mode when
            // available; restricted menus may require changing both together.
            // https://www.kernel.org/doc/html/latest/userspace-api/media/v4l/ext-ctrls-camera.html
            var exposure = type == CaptureDeviceControlTypes.Exposure ? isAuto : IsExposureAuto(CaptureDeviceControlTypes.Exposure, current);
            var iris = type == CaptureDeviceControlTypes.Iris ? isAuto : IsExposureAuto(CaptureDeviceControlTypes.Iris, current);
            desired = exposure ? iris ? V4L2_EXPOSURE_AUTO : V4L2_EXPOSURE_APERTURE_PRIORITY :
                iris ? V4L2_EXPOSURE_SHUTTER_PRIORITY : V4L2_EXPOSURE_MANUAL;
            if (!this.menu(mapping.AutoId, desired))
            {
                // Many webcams offer only aperture priority, others only full auto.
                desired = -1;
                foreach (var mode in new[] { V4L2_EXPOSURE_MANUAL, V4L2_EXPOSURE_APERTURE_PRIORITY,
                    V4L2_EXPOSURE_SHUTTER_PRIORITY, V4L2_EXPOSURE_AUTO })
                {
                    if (IsExposureAuto(type, mode) == isAuto && this.menu(mapping.AutoId, mode))
                    {
                        desired = mode;
                        break;
                    }
                }
                if (desired < 0) return false;
            }
        }

        // The device ignores a manual value while its automatic mode is on.
        // A failed switch must not be reported as a successful manual update.
        if (current != desired && !this.set(mapping.AutoId, desired)) return false;
        return isAuto || this.set(mapping.Id, value);
    }

    private bool SupportsColorEnable() =>
        this.query(V4L2_CID_COLORFX) is not null &&
        this.menu(V4L2_CID_COLORFX, V4L2_COLORFX_NONE) && this.menu(V4L2_CID_COLORFX, V4L2_COLORFX_BW);

    private bool HasExposureMode(CaptureDeviceControlTypes type, bool automatic)
    {
        for (var mode = V4L2_EXPOSURE_AUTO; mode <= V4L2_EXPOSURE_APERTURE_PRIORITY; mode++)
            if (IsExposureAuto(type, mode) == automatic && this.menu(V4L2_CID_EXPOSURE_AUTO, mode)) return true;
        return false;
    }

    private static bool IsExposureAuto(CaptureDeviceControlTypes type, int mode) =>
        mode == V4L2_EXPOSURE_AUTO || mode == (type == CaptureDeviceControlTypes.Iris ?
            V4L2_EXPOSURE_SHUTTER_PRIORITY : V4L2_EXPOSURE_APERTURE_PRIORITY);
}
