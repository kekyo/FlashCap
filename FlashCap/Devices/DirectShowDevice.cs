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
using System.Runtime.InteropServices;

namespace FlashCap.Devices
{
    public sealed class DirectShowDevice : ICaptureDevice
    {
        internal DirectShowDevice(IntPtr handle, int identity, bool transcodeIfYUV)
        {
        }

        public void Dispose()
        {
        }

        public VideoCharacteristics Characteristics =>
            throw new NotImplementedException();

#pragma warning disable CS0067
        public event EventHandler<FrameArrivedEventArgs>? FrameArrived;

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop() =>
            throw new NotImplementedException();

        public void Capture(FrameArrivedEventArgs e, PixelBuffer buffer) =>
            throw new NotImplementedException();
    }
}
