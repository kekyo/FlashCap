////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace FlashCap.FrameProcessors
{
    public delegate void FrameArrivedDelegate(
        ICaptureDevice captureDevice,
        IntPtr pData, int size, TimeSpan timestamp);

    internal sealed class DelegatedFrameProcessor : FrameProcessor
    {
        private readonly FrameArrivedDelegate frameArrived; 

        public DelegatedFrameProcessor(
            FrameArrivedDelegate frameArrived) =>
            this.frameArrived = frameArrived;

        public override void OnFrameArrived(
            ICaptureDevice captureDevice,
            IntPtr pData, int size, TimeSpan timestamp) =>
            this.frameArrived!(captureDevice, pData, size, timestamp);
    }

    internal sealed class ConstraintDelegatedFrameProcessor : FrameProcessor
    {
        private readonly FrameArrivedDelegate frameArrived;
        private volatile int isin;

        public ConstraintDelegatedFrameProcessor(
            FrameArrivedDelegate frameArrived) =>
            this.frameArrived = frameArrived;

        public override void OnFrameArrived(
            ICaptureDevice captureDevice,
            IntPtr pData, int size, TimeSpan timestamp)
        {
            if (Interlocked.Increment(ref this.isin) == 1)
            {
                try
                {
                    this.frameArrived!(captureDevice, pData, size, timestamp);
                }
                finally
                {
                    Interlocked.Decrement(ref this.isin);
                }
            }
            else
            {
                Interlocked.Decrement(ref this.isin);
            }
        }
    }
}
