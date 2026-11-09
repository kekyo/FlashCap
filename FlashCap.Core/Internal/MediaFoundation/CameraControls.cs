#if FLASHCAP_MEDIAFOUNDATION
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
using System.Runtime.Versioning;
using static FlashCap.Internal.NativeMethods_MediaFoundation;

namespace FlashCap.Internal.MediaFoundation;

[SupportedOSPlatform("windows6.1")]
internal sealed unsafe class CameraControls : IDisposable
{
    private const int Auto = 1;
    private const int Manual = 2;

    // Property IDs are the VideoProcAmpProperty and CameraControlProperty values
    // in strmif.h, not the ordinals of FlashCap's public control enum.
    private static readonly KeyValuePair<CaptureDeviceControlTypes, int>[] videoProperties =
    [
        new(CaptureDeviceControlTypes.Brightness, 0),
        new(CaptureDeviceControlTypes.Contrast, 1),
        new(CaptureDeviceControlTypes.Hue, 2),
        new(CaptureDeviceControlTypes.Saturation, 3),
        new(CaptureDeviceControlTypes.Sharpness, 4),
        new(CaptureDeviceControlTypes.Gamma, 5),
        new(CaptureDeviceControlTypes.WhiteBalance, 7),
        new(CaptureDeviceControlTypes.BacklightCompensation, 8),
        new(CaptureDeviceControlTypes.Gain, 9),
    ];

    private static readonly KeyValuePair<CaptureDeviceControlTypes, int>[] cameraProperties =
    [
        new(CaptureDeviceControlTypes.Pan, 0),
        new(CaptureDeviceControlTypes.Tilt, 1),
        new(CaptureDeviceControlTypes.Zoom, 3),
        new(CaptureDeviceControlTypes.Exposure, 4),
        new(CaptureDeviceControlTypes.Focus, 6),
    ];

    private CameraControlInterface* videoProcAmp;
    private CameraControlInterface* cameraControl;

    internal CameraControls(IUnknown* source)
    {
        try
        {
            this.videoProcAmp = Query(source, in IID_IAMVideoProcAmp);
            this.cameraControl = Query(source, in IID_IAMCameraControl);
        }
        catch
        {
            this.Dispose();
            throw;
        }
    }

    private static CameraControlInterface* Query(IUnknown* source, in Guid iid)
    {
        var result = source->QueryInterface(in iid, out var value);
        if (result == unchecked((int)0x80004002)) // E_NOINTERFACE: optional control interface.
        {
            return null;
        }
        MediaFoundationHelpers.ThrowIfFailed(result, "IUnknown.QueryInterface(camera controls)");
        return (CameraControlInterface*)value;
    }

    internal CaptureDeviceControl[] GetControls()
    {
        var controls = new List<CaptureDeviceControl>();
        AddRanges(controls, this.videoProcAmp, videoProperties);
        AddRanges(controls, this.cameraControl, cameraProperties);
        return controls.ToArray();
    }

    private static void AddRanges(
        List<CaptureDeviceControl> controls,
        CameraControlInterface* native,
        KeyValuePair<CaptureDeviceControlTypes, int>[] properties)
    {
        if (native is null)
        {
            return;
        }
        foreach (var property in properties)
        {
            if (native->GetRange(property.Value, out var min, out var max, out var step, out var defaultValue, out var caps) >= 0 &&
                max >= min)
            {
                controls.Add(new CaptureDeviceControl(
                    property.Key, min, max, step, defaultValue, (caps & (Auto | Manual)) == (Auto | Manual)));
            }
        }
    }

    internal CaptureDeviceControlValue? GetValue(CaptureDeviceControlTypes type)
    {
        var native = this.Find(type, out var property);
        return native is not null && native->Get(property, out var value, out var flags) >= 0 ?
            new CaptureDeviceControlValue(value, (flags & Auto) != 0) : null;
    }

    internal bool SetValue(CaptureDeviceControlTypes type, int value, bool isAuto)
    {
        var native = this.Find(type, out var property);
        if (native is null)
        {
            return false;
        }
        // Auto still requires an in-range value even though the driver chooses the setting.
        if (isAuto && native->Get(property, out value, out _) < 0)
        {
            return false;
        }
        return native->Set(property, value, isAuto ? Auto : Manual) >= 0;
    }

    private CameraControlInterface* Find(CaptureDeviceControlTypes type, out int propertyId)
    {
        foreach (var property in videoProperties)
        {
            if (property.Key == type)
            {
                propertyId = property.Value;
                return this.videoProcAmp;
            }
        }
        foreach (var property in cameraProperties)
        {
            if (property.Key == type)
            {
                propertyId = property.Value;
                return this.cameraControl;
            }
        }
        propertyId = 0;
        return null;
    }

    public void Dispose()
    {
        Release(this.videoProcAmp);
        this.videoProcAmp = null;
        Release(this.cameraControl);
        this.cameraControl = null;
    }
}
#endif
