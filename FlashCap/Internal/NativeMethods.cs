////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using FlashCap.Utilities;

namespace FlashCap.Internal
{
    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        // https://stackoverflow.com/questions/38790802/determine-operating-system-in-net-core
        public enum Platforms
        {
            Windows,
            Linux,
            MacOS,
            Other,
        }

        private static Platforms GetRuntimePlatform()
        {
            var windir = Environment.GetEnvironmentVariable("windir");
            if (!string.IsNullOrEmpty(windir) &&
                windir.Contains(Path.DirectorySeparatorChar.ToString()) &&
                Directory.Exists(windir))
            {
                return Platforms.Windows;
            }
            else if (File.Exists(@"/proc/sys/kernel/ostype"))
            {
                var osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
                if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
                {
                    return Platforms.Linux;
                }
                else
                {
                    return Platforms.Other;
                }
            }
            else if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
            {
                return Platforms.MacOS;
            }
            else
            {
                return Platforms.Other;
            }
        }

        public static readonly Platforms CurrentPlatform =
            GetRuntimePlatform();

        ////////////////////////////////////////////////////////////////////////

        // https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/aa366535(v=vs.85)
        [DllImport("ntdll")]
        private static extern void RtlCopyMemory(IntPtr dest, IntPtr src, IntPtr length);
        [DllImport("kernel32")]
        private static extern void RtlMoveMemory(IntPtr dest, IntPtr src, IntPtr length);

        [DllImport("libc")]
        private static extern void memcpy(IntPtr dest, IntPtr src, IntPtr length);

        public delegate void CopyMemoryDelegate(
            IntPtr pDestination, IntPtr pSource, IntPtr length);

        public static unsafe readonly CopyMemoryDelegate CopyMemory =
            CurrentPlatform == Platforms.Windows ?
                (IntPtr.Size == 4 ? RtlMoveMemory : RtlCopyMemory) :
                memcpy;

        ////////////////////////////////////////////////////////////////////////

        [DllImport("ole32")]
        private static extern IntPtr CoTaskMemAlloc(IntPtr size);
        [DllImport("ole32")]
        private static extern void CoTaskMemFree(IntPtr ptr);
        [DllImport("kernel32")]
        private static extern void RtlZeroMemory(IntPtr ptr, IntPtr size);

        [DllImport("libc")]
        private static extern IntPtr malloc(IntPtr size);
        [DllImport("libc")]
        private static extern void free(IntPtr ptr);
        [DllImport("libc")]
        private static extern IntPtr memset(IntPtr ptr, int c, IntPtr size);

        public delegate IntPtr AllocateMemoryDelegate(
            IntPtr size);
        public delegate void FreeMemoryDelegate(
            IntPtr ptr);

        private static IntPtr AllocateWindows(IntPtr size)
        {
            var ptr = CoTaskMemAlloc(size);
            RtlZeroMemory(ptr, size);
            return ptr;
        }
        private static IntPtr AllocatePosix(IntPtr size)
        {
            var ptr = malloc(size);
            memset(ptr, 0, size);
            return ptr;
        }

        public static readonly AllocateMemoryDelegate AllocateMemory =
            CurrentPlatform == Platforms.Windows ?
                AllocateWindows : AllocatePosix;
        public static readonly FreeMemoryDelegate FreeMemory =
            CurrentPlatform == Platforms.Windows ?
                CoTaskMemFree : free;

        ////////////////////////////////////////////////////////////////////////

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        private static int GetClrBits(
            short biPlanes, short biBitCount)
        {
            var clrBits = biPlanes * biBitCount;
            if (clrBits != 1)
            {
                if (clrBits <= 4)
                {
                    clrBits = 4;
                }
                else if (clrBits <= 8)
                {
                    clrBits = 8;
                }
                else if (clrBits <= 16)
                {
                    clrBits = 16;
                }
                else if (clrBits <= 24)
                {
                    clrBits = 24;
                }
                else
                {
                    clrBits = 32;
                }
            }
            return clrBits;
        }

        public enum Compression
        {
            RGB = 0,             // BI_RGB
            JPEG = 4,            // BI_JPEG
            PNG = 5,             // BI_PNG
            ARGB = 0x42475241,   // FOURCC
            RGB2 = 0x32424752,   // FOURCC
            YUY2 = 0x32595559,   // FOURCC
            YUYV = 0x56595559,   // FOURCC
            UYVY = 0x59565955,   // FOURCC
            MJPG = 0x47504A4D,   // FOURCC
        }

        private static int CalculateClrUsed(
            Compression compression, short biPlanes, short biBitCount)
        {
            if (compression != Compression.RGB)
            {
                return 0;
            }
            else
            {
                var clrBits = GetClrBits(biPlanes, biBitCount);
                return (clrBits < 24) ? (1 << clrBits) : 0;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        private static unsafe int CalculateRawSize(
            Compression compression, short biPlanes, short biBitCount) =>
            sizeof(BITMAPINFOHEADER) +
            CalculateClrUsed(compression, biPlanes, biBitCount) * sizeof(RGBQUAD);

        private static int CalculateImageSize(
            Compression compression,
            int biWidth, int biHeight, short biPlanes, short biBitCount) =>
            compression switch
            {
                Compression.JPEG => 0,
                Compression.PNG => 0,
                Compression.MJPG => 0,
                _ => ((biWidth * GetClrBits(biPlanes, biBitCount) + 31) & ~31) / 8 * biHeight,
            };

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public Compression biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;

            public int GetClrBits() =>
                NativeMethods.GetClrBits(this.biPlanes, this.biBitCount);
            public int CalculateClrUsed() =>
                NativeMethods.CalculateClrUsed(this.biCompression, this.biPlanes, this.biBitCount);
            public int CalculateRawSize() =>
                NativeMethods.CalculateRawSize(this.biCompression, this.biPlanes, this.biBitCount);
            public int CalculateImageSize() =>
                NativeMethods.CalculateImageSize(
                    this.biCompression, this.biWidth, this.biHeight,
                    this.biPlanes, this.biBitCount);
        }

        [StructLayout(LayoutKind.Sequential, Pack=2)]
        public struct BITMAPFILEHEADER
        {
            public byte bfType0;
            public byte bfType1;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }

        ////////////////////////////////////////////////////////////////////////

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VIDEOINFOHEADER
        {
            public RECT rcSource;
            public RECT rcTarget;
            public int dwBitRate;
            public int dwBitErrorRate;
            public long AvgTimePerFrame;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VIDEOINFOHEADER2
        {
            public RECT rcSource;
            public RECT rcTarget;
            public int dwBitRate;
            public int dwBitErrorRate;
            public long AvgTimePerFrame;
            public int dwInterlaceFlags;
            public int dwCopyProtectFlags;
            public int dwPictAspectRatioX;
            public int dwPictAspectRatioY;
            public int dwControlFlags;    // dwReserved1
            public int dwReserved2;
        }
                
        ////////////////////////////////////////////////////////////////////////

        // https://en.wikipedia.org/wiki/Computer_display_standard
        public readonly struct Resolution
        {
            public readonly int Width;
            public readonly int Height;

            private Resolution(int width, int height)
            {
                this.Width = width;
                this.Height = height;
            }

            public static Resolution Create(int width, int height) =>
                new Resolution(width, height);
        }
        
        public static readonly Resolution[] DefactoStandardResolutions = new[]
        {
            Resolution.Create(10240, 4320),
            Resolution.Create(7680, 4800),
            Resolution.Create(7680, 4320),
            Resolution.Create(7680, 3200),
            Resolution.Create(6400, 4800),
            Resolution.Create(6400, 4096),
            Resolution.Create(5120, 4096),
            Resolution.Create(5120, 3200),
            Resolution.Create(5120, 2880),
            Resolution.Create(5120, 2160),
            Resolution.Create(4500, 3000),
            Resolution.Create(4096, 3072),
            Resolution.Create(4096, 2160),
            Resolution.Create(3840, 2400),
            Resolution.Create(3840, 2160),
            Resolution.Create(3840, 1600),
            Resolution.Create(3440, 1440),
            Resolution.Create(3200, 2400),
            Resolution.Create(3200, 2048),
            Resolution.Create(3000, 2000),
            Resolution.Create(2960, 1440),
            Resolution.Create(2880, 1800),
            Resolution.Create(2880, 1440),
            Resolution.Create(2560, 2048),
            Resolution.Create(2560, 1600),
            Resolution.Create(2560, 1440),
            Resolution.Create(2560, 1080),
            Resolution.Create(2160, 1440),
            Resolution.Create(2048, 1536),
            Resolution.Create(2048, 1152),
            Resolution.Create(2048, 1080),
            Resolution.Create(1920, 1440),
            Resolution.Create(1920, 1280),
            Resolution.Create(1920, 1200),
            Resolution.Create(1920, 1080),
            Resolution.Create(1680, 1050),
            Resolution.Create(1600, 1200),
            Resolution.Create(1600, 900),
            Resolution.Create(1440, 900),
            Resolution.Create(1400, 1050),
            Resolution.Create(1366, 768),
            Resolution.Create(1360, 768),
            Resolution.Create(1280, 1024),
            Resolution.Create(1280, 960),
            Resolution.Create(1280, 720),
            Resolution.Create(1152, 900),
            Resolution.Create(1152, 870),
            Resolution.Create(1152, 864),
            Resolution.Create(1056, 400),
            Resolution.Create(1024, 768),
            Resolution.Create(832, 624),
            Resolution.Create(800, 600),
            Resolution.Create(720, 576),
            Resolution.Create(720, 480),
            Resolution.Create(720, 400),
            Resolution.Create(720, 350),
            Resolution.Create(640, 480),
            Resolution.Create(640, 400),
            Resolution.Create(640, 350),
            Resolution.Create(640, 200),
            Resolution.Create(512, 384),
            Resolution.Create(480, 272),
            Resolution.Create(480, 720),
            Resolution.Create(320, 240),
            Resolution.Create(320, 200),
            Resolution.Create(240, 160),
            Resolution.Create(160, 200),
            Resolution.Create(160, 144),
            Resolution.Create(160, 128),
            Resolution.Create(160, 120),
        };
 
        public static readonly Fraction[] DefactoStandardFramesPerSecond = new[]
        {
            Fraction.Create(120),
            Fraction.Create(120000, 1001),
            Fraction.Create(60),
            Fraction.Create(60000, 1001),
            Fraction.Create(50),
            Fraction.Create(30),
            Fraction.Create(30000, 1001),
            Fraction.Create(25),
            Fraction.Create(24),
            Fraction.Create(24000, 1001),
            Fraction.Create(20),
            Fraction.Create(15),
            Fraction.Create(12),
            Fraction.Create(12000, 1001),
            Fraction.Create(10),
            Fraction.Create(5),
        };
        
        ////////////////////////////////////////////////////////////////////////

        public static string GetFourCCString(int fourcc)
        {
            var sb = new StringBuilder();
            sb.Append((char)(byte)fourcc);
            sb.Append((char)(byte)(fourcc >> 8));
            sb.Append((char)(byte)(fourcc >> 16));
            sb.Append((char)(byte)(fourcc >> 24));
            return sb.ToString();
        }
        
        public static VideoCharacteristics? CreateVideoCharacteristics(
            Compression compression,
            int width, int height, int clrBits,
            Fraction framesPerSecond,
            string? rawPixelFormat = null)
        {
            static PixelFormats? GetRGBPixelFormat(int clrBits) =>
                clrBits switch
                {
                    8 => PixelFormats.RGB8,
                    16 => PixelFormats.RGB15, // BI_RGB is 15bit (RGB555, NOT RGB565)
                    24 => PixelFormats.RGB24,
                    32 => PixelFormats.ARGB32,
                    _ => null,
                };
            
            if (compression switch
            {
                Compression.RGB => GetRGBPixelFormat(clrBits),
                Compression.RGB2 => GetRGBPixelFormat(clrBits),
                Compression.ARGB => PixelFormats.ARGB32,
                Compression.MJPG => PixelFormats.JPEG,
                Compression.JPEG => PixelFormats.JPEG,
                Compression.PNG => PixelFormats.PNG,
                Compression.UYVY => PixelFormats.UYVY,
                Compression.YUYV => PixelFormats.YUYV,
                Compression.YUY2 => PixelFormats.YUYV,
                _ => null,
            } is { } pixelFormat)
            {
                return new VideoCharacteristics(
                    pixelFormat, width, height,
                    framesPerSecond.Reduce(),
                    compression.ToString(),
                    rawPixelFormat ?? GetFourCCString((int)compression));
            }
            else
            {
                return null;
            }
        }
        
        public static unsafe VideoCharacteristics? CreateVideoCharacteristics(
            IntPtr pih, Fraction framesPerSecond, string? rawPixelFormat = null)
        {
            var pBih = (BITMAPINFOHEADER*)pih.ToPointer();
            return CreateVideoCharacteristics(
                pBih->biCompression, pBih->biWidth, pBih->biHeight,
                pBih->GetClrBits(), framesPerSecond,
                rawPixelFormat);
        }
    }
}
