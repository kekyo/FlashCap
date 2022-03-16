////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;

namespace FlashCap
{
    public sealed class FrameArrivedEventArgs : EventArgs
    {
        internal double timestampMilliseconds;
        internal IntPtr pData;
        internal int size;

        internal FrameArrivedEventArgs()
        {
        }

        public FrameArrivedEventArgs(IntPtr pData, int size, TimeSpan timestamp)
        {
            this.pData = pData;
            this.size = size;
            this.timestampMilliseconds = timestamp.TotalMilliseconds;
        }

        // HACK: Zero allocation backdoor.
        internal void Update(IntPtr pData, int size, double timestampMilliseconds)
        {
            this.pData = pData;
            this.size = size;
            this.timestampMilliseconds = timestampMilliseconds;
        }

        public IntPtr Data =>
            this.pData;
        public int Size =>
            this.size;
        public TimeSpan Timestamp =>
            TimeSpan.FromMilliseconds(this.timestampMilliseconds);
    }
}
