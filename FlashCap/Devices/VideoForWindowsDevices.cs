////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCap.Devices
{
    public sealed class VideoForWindowsDevices : ICaptureDevices
    {
        public IEnumerable<ICaptureDeviceDescriptor> EnumerateDescriptors() =>
            Enumerable.Range(0, NativeMethods_VideoForWindows.MaxVideoForWindowsDevices).
            Collect(index =>
            {
                var name = new StringBuilder(256);
                var description = new StringBuilder(256);

                if (NativeMethods_VideoForWindows.capGetDriverDescription(
                    (uint)index, name, name.Length, description, description.Length))
                {
                    var n = name.ToString().Trim();
                    var d = description.ToString().Trim();

                    return (ICaptureDeviceDescriptor)new VideoForWindowsDeviceDescriptor(   // Requires casting on net20
                        index,
                        string.IsNullOrEmpty(n) ? "Default" : n,
                        string.IsNullOrEmpty(d) ? "VideoForWindows default" : d,
                        new[] {
                            new VideoCharacteristics(PixelFormats.MJPG, 24, 1920, 1080, 30000),
                            new VideoCharacteristics(PixelFormats.MJPG, 24, 1600, 1200, 30000),
                            new VideoCharacteristics(PixelFormats.MJPG, 24, 1280, 960, 30000),
                            new VideoCharacteristics(PixelFormats.MJPG, 24, 1024, 768, 30000),
                            new VideoCharacteristics(PixelFormats.MJPG, 24, 640, 480, 30000),
                            new VideoCharacteristics(PixelFormats.MJPG, 24, 640, 480, 15000),
                            new VideoCharacteristics(PixelFormats.YUY2, 16, 640, 480, 30000),
                            new VideoCharacteristics(PixelFormats.YUY2, 16, 640, 480, 15000),
                        });
                }
                else
                {
                    return null;
                }
            });
    }
}
