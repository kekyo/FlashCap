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
        public readonly IntPtr Data;
        public readonly int Size;
        public readonly TimeSpan Timestamp;

        public FrameArrivedEventArgs(IntPtr pData, int size, TimeSpan timestamp)
        {
            this.Data = pData;
            this.Size = size;
            this.Timestamp = timestamp;
        }
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
