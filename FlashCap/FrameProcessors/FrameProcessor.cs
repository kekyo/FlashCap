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
    public abstract class FrameProcessor
    {
        protected FrameProcessor()
        {
        }

        public abstract void OnFrameArrived(
            ICaptureDevice captureDevice,
            IntPtr pData, int size, TimeSpan timestamp);
    }
}
