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
        RGB8,
        RGB15,
        RGB16,
        RGB24,
        RGB32,
        ARGB32,
        JPEG,
        PNG,
        UYVY,
        YUYV,
    }

    public sealed class VideoCharacteristics :
        IEquatable<VideoCharacteristics>, IComparable<VideoCharacteristics>
    {
        public readonly PixelFormats PixelFormat;
        public readonly int Width;
        public readonly int Height;
        public readonly Fraction FramesPerSecond;
        public readonly string Description;
        public readonly string RawPixelFormat;

        public VideoCharacteristics(
            PixelFormats pixelFormat,
            int width, int height,
            Fraction framesPerSecond)
        {
            this.PixelFormat = pixelFormat;
            this.Width = width;
            this.Height = height;
            this.FramesPerSecond = framesPerSecond;
            this.Description = pixelFormat.ToString();
            this.RawPixelFormat = pixelFormat.ToString();
        }

        public VideoCharacteristics(
            PixelFormats pixelFormat,
            int width, int height,
            Fraction framesPerSecond,
            string description,
            string rawPixelFormat)
        {
            this.PixelFormat = pixelFormat;
            this.Width = width;
            this.Height = height;
            this.FramesPerSecond = framesPerSecond;
            this.Description = description;
            this.RawPixelFormat = rawPixelFormat;
        }

        public short RGBBitsPerPixel =>
            this.PixelFormat switch
            {
                PixelFormats.RGB8 => 8,
                PixelFormats.RGB15 => 16,   // 15
                PixelFormats.RGB24 => 24,
                PixelFormats.ARGB32 => 32,
                _ => 0,
            };
        
        public bool IsCompression =>
            this.PixelFormat switch
            {
                PixelFormats.JPEG => true,
                PixelFormats.PNG => true,
                _ => false,
            };

        public override int GetHashCode() =>
            this.PixelFormat.GetHashCode() ^
            this.Width.GetHashCode() ^
            this.Height.GetHashCode() ^
            this.FramesPerSecond.GetHashCode();

        public bool Equals(VideoCharacteristics? other) =>
            other is { } &&
            this.PixelFormat == other.PixelFormat &&
            this.Width == other.Width &&
            this.Height == other.Height &&
            this.FramesPerSecond == other.FramesPerSecond;

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
            if (this.FramesPerSecond.CompareTo(other!.FramesPerSecond) is { } c3 && c3 != 0)
            {
                return c3;
            }
            return 0;
        }

        public override string ToString() =>
            $"{this.Width}x{this.Height} [{this.PixelFormat}/{this.RawPixelFormat}, {(double)this.FramesPerSecond}fps]";
    }
}
