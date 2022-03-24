////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;

namespace FlashCap.FrameProcessors
{
    internal sealed class DelegatedFrameProcessor : FrameProcessor
    {
        private readonly FrameArrivedDelegate frameArrived; 

        public DelegatedFrameProcessor(
            FrameArrivedDelegate frameArrived) =>
            this.frameArrived = frameArrived;

        public override void OnFrameArrived(
            CaptureDevice captureDevice,
            IntPtr pData, int size, long timestampMicroseconds) =>
            this.frameArrived!(captureDevice, pData, size, timestampMicroseconds);
    }
}
