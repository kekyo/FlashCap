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
using System.Threading.Tasks;

namespace FlashCap.Internal;

public enum YUV2RGBConversionStandard
{
    Auto,
    BT_601,
    BT_709,
    BT_2020,
}
internal static class BitmapTranscoder
{
    private static readonly int scatteringBase = Environment.ProcessorCount;

    // Preffered article: https://docs.microsoft.com/en-us/windows/win32/medfound/recommended-8-bit-yuv-formats-for-video-rendering#420-formats-16-bits-per-pixel

    private static unsafe void TranscodeFromYUV_BT_601(
       int width, int height, bool performFullRange, bool isUYVY,
       byte* pFrom, byte* pTo)
    {
        var scatter = height / scatteringBase;
        Parallel.For(0, (height + scatter - 1) / scatter, ys =>
        {
            var y = ys * scatter;
            var myi = Math.Min(height - y, scatter);
            int c1, c2, d, e, cc1, cc2;

            for (var yi = 0; yi < myi; yi++)
            {
                byte* pFromBase = pFrom + (height - (y + yi) - 1) * width * 2;
                byte* pToBase = pTo + (y + yi) * width * 3;

                for (var x = 0; x < width; x += 2)
                {
                    //Color profile ITU-R BT.601 Limited Range
                    //matrix 1.1643, 1.16430, 1.1643,
                    //       0.0,   -0.39173, 2.0170,
                    //       1.5958,-0.81290, 0.0
                    //converts to rounded int (multiply by 256)
                    //298,  298, 298,
                    //0   ,-100, 516
                    //409, -208, 0

                    if (isUYVY)
                    {
                        d = pFromBase[0] - 128;  // U
                        c1 = pFromBase[1] - 16;  // Y1
                        e = pFromBase[2] - 128;  // V
                        c2 = pFromBase[3] - 16;  // Y2
                    }
                    else
                    {
                        c1 = pFromBase[0] - 16;   // Y1
                        d = pFromBase[1] - 128;   // U
                        c2 = pFromBase[2] - 16;   // Y2
                        e = pFromBase[3] - 128;   // V
                    }

                    cc1 = 298 * c1; // (Y1-0.0625)*1.164
                    cc2 = 298 * c2; // (Y2-0.0625)*1.164

                    *pToBase++ = Clip((cc1 + 516 * d + 128) >> 8);   // B1
                    *pToBase++ = Clip((cc1 - 100 * d - 208 * e + 128) >> 8);   // G1
                    *pToBase++ = Clip((cc1 + 409 * e + 128) >> 8);   // R1

                    *pToBase++ = Clip((cc2 + 516 * d + 128) >> 8);   // B2
                    *pToBase++ = Clip((cc2 - 100 * d - 208 * e + 128) >> 8);   // G2
                    *pToBase++ = Clip((cc2 + 409 * e + 128) >> 8);   // R2

                    pFromBase += 4;
                }
            }
        });
    }

    private static unsafe void TranscodeFromYUV_BT_709(
    int width, int height, bool performFullRange, bool isUYVY,
    byte* pFrom, byte* pTo)
    {
        var scatter = height / scatteringBase;
        Parallel.For(0, (height + scatter - 1) / scatter, ys =>
        {
            var y = ys * scatter;
            var myi = Math.Min(height - y, scatter);
            int c1, c2, d, e, cc1, cc2;

            for (var yi = 0; yi < myi; yi++)
            {
                byte* pFromBase = pFrom + (height - (y + yi) - 1) * width * 2;
                byte* pToBase = pTo + (y + yi) * width * 3;

                for (var x = 0; x < width; x += 2)
                {
                    //Color profile ITU-R BT.709 Limited Range
                    //matrix 1.0,     1.0,    1.0,
                    //       0.0,    -0.1873, 1.8556,
                    //       1.5748, -0.4681, 0.0
                    // converts to rounded int (multiply by 256)
                    //256, 256, 256,
                    //0   ,-48, 475
                    //403,-120, 0

                    if (isUYVY)
                    {
                        d = pFromBase[0] - 128;  // U
                        c1 = pFromBase[1] - 16;  // Y1
                        e = pFromBase[2] - 128;  // V
                        c2 = pFromBase[3] - 16;  // Y2
                    }
                    else
                    {
                        c1 = pFromBase[0] - 16;   // Y1
                        d = pFromBase[1] - 128;   // U
                        c2 = pFromBase[2] - 16;   // Y2
                        e = pFromBase[3] - 128;   // V
                    }

                    cc1 = 256 * c1;
                    cc2 = 256 * c2;

                    *pToBase++ = Clip((cc1 + 475 * d + 128) >> 8);   // B1
                    *pToBase++ = Clip((cc1 - 48 * d - 120 * e + 128) >> 8);   // G1
                    *pToBase++ = Clip((cc1 + 403 * e + 128) >> 8);   // R1

                    *pToBase++ = Clip((cc2 + 475 * d + 128) >> 8);   // B2
                    *pToBase++ = Clip((cc2 - 48 * d - 120 * e + 128) >> 8);   // G2
                    *pToBase++ = Clip((cc2 + 403 * e + 128) >> 8);   // R2

                    pFromBase += 4;
                }
            }
        });
    }

    private static unsafe void TranscodeFromYUV_BT_2020(
        int width, int height, bool performFullRange, bool isUYVY,
        byte* pFrom, byte* pTo)
    {
        var scatter = height / scatteringBase;
        Parallel.For(0, (height + scatter - 1) / scatter, ys =>
        {
            var y = ys * scatter;
            var myi = Math.Min(height - y, scatter);
            int c1, c2, d, e, cc1, cc2;

            for (var yi = 0; yi < myi; yi++)
            {
                byte* pFromBase = pFrom + (height - (y + yi) - 1) * width * 2;
                byte* pToBase = pTo + (y + yi) * width * 3;

                for (var x = 0; x < width; x += 2)
                {
                    //Color profile ITU-R BT.2020 Limited Range
                    //matrix 1,0,     1.0,       1.0,
                    //       0.0,    -0.1645531, 1.8814,
                    //       1.4746, -0.571353,  0.0
                    //converts to rounded int (multiply by 256)
                    //256, 256, 256,
                    //0   ,-42, 482
                    //377, -146, 0

                    if (isUYVY)
                    {
                        d = pFromBase[0] - 128;  // U
                        c1 = pFromBase[1] - 16;  // Y1
                        e = pFromBase[2] - 128;  // V
                        c2 = pFromBase[3] - 16;  // Y2
                    }
                    else
                    {
                        c1 = pFromBase[0] - 16;   // Y1
                        d = pFromBase[1] - 128;   // U
                        c2 = pFromBase[2] - 16;   // Y2
                        e = pFromBase[3] - 128;   // V
                    }


                    cc1 = 256 * c1;
                    cc2 = 256 * c2;

                    *pToBase++ = Clip((cc1 + 482 * d + 128) >> 8);   // B1
                    *pToBase++ = Clip((cc1 - 42 * d - 146 * e + 128) >> 8);   // G1
                    *pToBase++ = Clip((cc1 + 377 * e + 128) >> 8);   // R1

                    *pToBase++ = Clip((cc2 + 482 * d + 128) >> 8);   // B2
                    *pToBase++ = Clip((cc2 - 42 * d - 146 * e + 128) >> 8);   // G2
                    *pToBase++ = Clip((cc2 + 377 * e + 128) >> 8);   // R2

                    pFromBase += 4;
                }
            }
        });
    }

    private static unsafe void TranscodeFromYUV(
        int width, int height,
        YUV2RGBConversionStandard conversionStandard,
        bool isUYVY,
        bool performFullRange,
       byte* pFrom, byte* pTo)
    {
        switch (conversionStandard)
        {
            case YUV2RGBConversionStandard.BT_601:
                TranscodeFromYUV_BT_601(width, height, performFullRange, isUYVY, pFrom, pTo);
                break;
            case YUV2RGBConversionStandard.BT_709:
                TranscodeFromYUV_BT_709(width, height, performFullRange, isUYVY, pFrom, pTo);
                break;
            case YUV2RGBConversionStandard.BT_2020:
                TranscodeFromYUV_BT_2020(width, height, performFullRange, isUYVY, pFrom, pTo);
                break;
            case YUV2RGBConversionStandard.Auto:
                // determine the color conversion based on the width of the frame
                if (width < 1920) // SD
                    TranscodeFromYUV_BT_601(width, height, performFullRange, isUYVY, pFrom, pTo);
                else if (width < 3840) // HD
                    TranscodeFromYUV_BT_709(width, height, performFullRange, isUYVY, pFrom, pTo);
                else // UHD or larger
                    TranscodeFromYUV_BT_2020(width, height, performFullRange, isUYVY, pFrom, pTo);
                break;
            default:
                TranscodeFromYUV_BT_601(width, height, performFullRange, isUYVY, pFrom, pTo);
                break;
        }
    }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static byte Clip(int value) =>
        value < 0 ? (byte)0 :
        value > 255 ? (byte)255 :
        (byte)value;

    public static int? GetRequiredBufferSize(
        int width, int height, NativeMethods.Compression compression)
    {
        switch (compression)
        {
            case NativeMethods.Compression.UYVY:
            case NativeMethods.Compression.YUYV:
            case NativeMethods.Compression.YUY2:
            case NativeMethods.Compression.HDYC:
                return width * height * 3;
            default:
                return null;
        }
    }

    public static unsafe void Transcode(
        int width, int height,
        NativeMethods.Compression compression, 
        YUV2RGBConversionStandard conversionStandard, 
        bool performFullRange,
        byte* pFrom, byte* pTo)
    {
        switch (compression)
        {
            case NativeMethods.Compression.UYVY:
            case NativeMethods.Compression.HDYC:
                TranscodeFromYUV(width, height, conversionStandard, true, performFullRange, pFrom, pTo);
                break;
            case NativeMethods.Compression.YUYV:
            case NativeMethods.Compression.YUY2:
                TranscodeFromYUV(width, height, conversionStandard, false, performFullRange, pFrom, pTo);
                break;
            default:
                throw new ArgumentException();
        }
    }
}
