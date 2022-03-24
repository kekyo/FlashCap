////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.FrameProcessors;
using System;

namespace FlashCap
{
    public enum DeviceTypes
    {
        VideoForWindows,
        DirectShow,
        V4L2,
    }

    public interface ICaptureDeviceDescriptor
    {
        object Identity { get; }
        DeviceTypes DeviceType { get; }
        string Name { get; }
        string Description { get; }
        VideoCharacteristics[] Characteristics { get; }

        ICaptureDevice OpenWithFrameProcessor(
            VideoCharacteristics characteristics,
            bool transcodeIfYUV,
            FrameProcessor frameProcessor);
    }
}
