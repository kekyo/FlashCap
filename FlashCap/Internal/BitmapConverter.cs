////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

namespace FlashCap.Internal
{
    internal static class BitmapConverter
    {
        private static unsafe void ConvertFromUYVY(
            int width, int height, byte* pFrom, byte* pTo)
        {
            for (var y = 0; y < height; y++)
            {
                byte* pFromBase = pFrom + (height - y - 1) * width * 2;

                for (var x = 0; x < width; x += 2)
                {
                    double u = pFromBase[0];
                    double y1 = pFromBase[1];
                    double v = pFromBase[2];
                    double y2 = pFromBase[3];

                    *pTo++ = Saturate(y1 + 1.773 * (u - 128.0));
                    *pTo++ = Saturate(y1 - 0.344 * (v - 128.0) - 0.714 * (u - 128.0));
                    *pTo++ = Saturate(y1 + 1.403 * (v - 128.0));

                    *pTo++ = Saturate(y2 + 1.773 * (u - 128.0));
                    *pTo++ = Saturate(y2 - 0.344 * (v - 128.0) - 0.714 * (u - 128.0));
                    *pTo++ = Saturate(y2 + 1.403 * (v - 128.0));

                    pFromBase += 4;
                }
            }
        }

        private static unsafe void ConvertFromYUY2(
            int width, int height, byte* pFrom, byte* pTo)
        {
            for (var y = 0; y < height; y++)
            {
                byte* pFromBase = pFrom + (height - y - 1) * width * 2;

                for (var x = 0; x < width; x += 2)
                {
                    double y1 = pFromBase[0];
                    double u = pFromBase[1];
                    double y2 = pFromBase[2];
                    double v = pFromBase[3];

                    *pTo++ = Saturate(y1 + 1.773 * (u - 128.0));
                    *pTo++ = Saturate(y1 - 0.344 * (v - 128.0) - 0.714 * (u - 128.0));
                    *pTo++ = Saturate(y1 + 1.403 * (v - 128.0));

                    *pTo++ = Saturate(y2 + 1.773 * (u - 128.0));
                    *pTo++ = Saturate(y2 - 0.344 * (v - 128.0) - 0.714 * (u - 128.0));
                    *pTo++ = Saturate(y2 + 1.403 * (v - 128.0));

                    pFromBase += 4;
                }
            }
        }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static byte Saturate(double value) =>
            value < 0 ? (byte)0 :
            value > 255 ? (byte)255 :
            (byte)value;

        public static int? GetRequiredBufferSize(
            int width, int height, NativeMethods.CompressionModes compressionMode)
        {
            switch (compressionMode)
            {
                case NativeMethods.CompressionModes.BI_UYVY:
                case NativeMethods.CompressionModes.BI_YUY2:
                    return width * height * 3;
                case NativeMethods.CompressionModes.BI_RGB:
                case NativeMethods.CompressionModes.BI_JPEG:
                case NativeMethods.CompressionModes.BI_PNG:
                    return null;
                default:
                    throw new ArgumentException();
            }
        }

        public static unsafe void Convert(
            int width, int height, NativeMethods.CompressionModes compressionMode,
            byte* pFrom, byte* pTo)
        {
            switch (compressionMode)
            {
                case NativeMethods.CompressionModes.BI_UYVY:
                    ConvertFromUYVY(width, height, pFrom, pTo);
                    break;
                case NativeMethods.CompressionModes.BI_YUY2:
                    ConvertFromYUY2(width, height, pFrom, pTo);
                    break;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
