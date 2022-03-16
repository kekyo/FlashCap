////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace FlashCap.Utilities
{
    public sealed class PixelFormatComparer : IComparer<PixelFormats>
    {
        private PixelFormatComparer()
        {
        }

        private static int GetComparableCode(PixelFormats pixelFormat) =>
            pixelFormat switch
            {
                PixelFormats.MJPG => 0,
                PixelFormats.JPEG => 10,
                PixelFormats.RGB => 30,
                PixelFormats.RGBA => 40,
                PixelFormats.PNG => 50,
                _ => 20,
            };

        public int Compare(PixelFormats x, PixelFormats y) =>
            GetComparableCode(x).CompareTo(GetComparableCode(y));

        public static readonly PixelFormatComparer Instance =
            new PixelFormatComparer();
    }
}
