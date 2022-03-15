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
        private double timestampMilliseconds;
        private IntPtr pData;
        private int size;

        internal FrameArrivedEventArgs()
        {
        }

        public FrameArrivedEventArgs(IntPtr pData, int size, double timestampMilliseconds)
        {
            this.pData = pData;
            this.size = size;
            this.timestampMilliseconds = timestampMilliseconds;
        }

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

    public interface ICaptureDevice : IDisposable
    {
        VideoCharacteristics Characteristics { get; }

        event EventHandler<FrameArrivedEventArgs> FrameArrived;

        void Start();
        void Stop();

        void Capture(FrameArrivedEventArgs e, PixelBuffer buffer);
    }
}
