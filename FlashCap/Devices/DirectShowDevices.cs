////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlashCap.Devices
{
    public sealed class DirectShowDevices : ICaptureDevices
    {
        private static unsafe VideoCharacteristics CreateVideoCharacteristics(
            IntPtr pih, long avgTimePerFrame)
        {
            var pBih = (NativeMethods.RAW_BITMAPINFOHEADER*)pih.ToPointer();
            return new VideoCharacteristics(
                pBih->biCompression, pBih->biBitCount,
                pBih->biWidth, pBih->biHeight,
                (int)(1.0 / avgTimePerFrame * 10_000_000_000.0));
        }

        public IEnumerable<ICaptureDeviceDescriptor> EnumerateDescriptors() =>
            NativeMethods_DirectShow.EnumerateDeviceMoniker(
                NativeMethods_DirectShow.CLSID_VideoInputDeviceCategory).
            Collect(moniker => moniker.GetPropertyBag() is { } pb ?
                pb.SafeReleaseBlock(pb =>
                    pb.GetValue("FriendlyName", default(string))?.Trim() is { } n &&
                    (string.IsNullOrEmpty(n) ? "Unknown" : n!) is { } name &&
                    pb.GetValue("DevicePath", default(string))?.Trim() is { } devicePath ?
                        (ICaptureDeviceDescriptor)new DirectShowDeviceDescriptor(   // Requires casting on net20
                            devicePath, name,
                            pb.GetValue("Description", default(string))?.Trim() ?? name,
                            moniker.BindToObject(
                                null, null, in NativeMethods_DirectShow.IID_IBaseFilter, out var bf) == 0 &&
                            bf is NativeMethods_DirectShow.IBaseFilter baseFilter ?
                                baseFilter.SafeReleaseBlock(bf => baseFilter.EnumeratePins().
                                    Collect(pin =>
                                        pin.GetPinInfo() is { } pinInfo &&
                                        pinInfo.dir == NativeMethods_DirectShow.PIN_DIRECTION.Output ?
                                            pin : null).
                                    SelectMany(pin =>
                                        pin.EnumerateFormats().
                                        Select(format => CreateVideoCharacteristics(
                                            format.BitmapInfoHeader,
                                            format.VideoInformation.AvgTimePerFrame))).
                                    Distinct().
                                    OrderByDescending(vc => vc).
                                    ToArray()) :
                                ArrayEx.Empty<VideoCharacteristics>()) :
                        null) :
                null);
    }
}
