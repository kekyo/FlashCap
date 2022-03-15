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
        private readonly double timestampMilliseconds;

        public readonly IntPtr Data;
        public readonly int Size;

        public FrameArrivedEventArgs(IntPtr pData, int size, double timestampMilliseconds)
        {
            this.Data = pData;
            this.Size = size;
            this.timestampMilliseconds = timestampMilliseconds;
        }

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
