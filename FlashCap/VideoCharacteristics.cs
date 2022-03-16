////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Utilities;
using System;

namespace FlashCap
{
    public enum PixelFormats
    {
        RGB = 0,
        JPEG = 4,
        PNG = 5,
        YUY2 = 0x32595559,   // FOURCC
        UYVY = 0x59565955,   // FOURCC
        MJPG = 0x47504a4d,   // FOURCC
        RGBA = 0x41424752,   // FOURCC
    }

    public sealed class VideoCharacteristics :
        IEquatable<VideoCharacteristics>, IComparable<VideoCharacteristics>
    {
        public readonly PixelFormats PixelFormat;
        public readonly int BitsPerPixel;
        public readonly int Width;
        public readonly int Height;
        public readonly int FramesPer1000Second;

        public VideoCharacteristics(
            PixelFormats pixelFormat, int bitsPerPixel,
            int width, int height, int framesPer1000Second)
        {
            this.PixelFormat = pixelFormat;
            this.BitsPerPixel = bitsPerPixel;
            this.Width = width;
            this.Height = height;
            this.FramesPer1000Second = framesPer1000Second;
        }

        public override int GetHashCode() =>
            this.PixelFormat.GetHashCode() ^
            this.BitsPerPixel.GetHashCode() ^
            this.Width.GetHashCode() ^
            this.Height.GetHashCode() ^
            this.FramesPer1000Second.GetHashCode();

        public bool Equals(VideoCharacteristics? other) =>
            other is { } &&
            this.PixelFormat == other.PixelFormat &&
            this.BitsPerPixel == other.BitsPerPixel &&
            this.Width == other.Width &&
            this.Height == other.Height &&
            this.FramesPer1000Second == other.FramesPer1000Second;

        public override bool Equals(object? obj) =>
            this.Equals(obj as VideoCharacteristics);

        public int CompareTo(VideoCharacteristics? other)
        {
            if (PixelFormatComparer.Instance.Compare(this.PixelFormat, other!.PixelFormat) is { } c4 && c4 != 0)
            {
                return c4;
            }
            if (this.Width.CompareTo(other!.Width) is { } c1 && c1 != 0)
            {
                return c1;
            }
            if (this.Height.CompareTo(other!.Height) is { } c2 && c2 != 0)
            {
                return c2;
            }
            if (this.FramesPer1000Second.CompareTo(other!.FramesPer1000Second) is { } c3 && c3 != 0)
            {
                return c3;
            }
            if (this.BitsPerPixel.CompareTo(other!.BitsPerPixel) is { } c5 && c5 != 0)
            {
                return c5;
            }
            return 0;
        }

        public override string ToString() =>
            $"{this.Width}x{this.Height} [{this.PixelFormat}, {this.BitsPerPixel}bits, {this.FramesPer1000Second / 1000.0}fps]";
    }
}
