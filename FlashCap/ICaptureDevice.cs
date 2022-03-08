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
        public readonly int Timestamp;

        public FrameArrivedEventArgs(IntPtr pData, int size, int timestamp)
        {
            this.Data = pData;
            this.Size = size;
            this.Timestamp = timestamp;
        }
    }

    public enum PixelFormats
    {
        RGB = 0,
        Jpeg = 4,
        Png = 5,
        YUY2 = 0x32595559,
        UYVY = 0x59565955,
    }

    public interface ICaptureDevice : IDisposable
    {
        int Width { get; }
        int Height { get; }
        int BitsPerPixel { get; }
        PixelFormats PixelFormat { get; }

        event EventHandler<FrameArrivedEventArgs> FrameArrived;

        void Start();
        void Stop();

        void Capture(FrameArrivedEventArgs e, PixelBuffer buffer);
    }
}
