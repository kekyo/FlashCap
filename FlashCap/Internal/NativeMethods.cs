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

        public static Platforms GetRuntimePlatform()
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

        // https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/aa366535(v=vs.85)
        [DllImport("ntdll", EntryPoint = "RtlCopyMemory")]
        private static extern unsafe void RtlCopyMemory(IntPtr dest, IntPtr src, IntPtr length);

        [DllImport("libc", EntryPoint = "memcpy")]
        private static extern unsafe void memcpy(IntPtr dest, IntPtr src, IntPtr length);

        public unsafe delegate void CopyMemoryDelegate(
            IntPtr pDestination, IntPtr pSource, IntPtr length);

        public static unsafe readonly CopyMemoryDelegate CopyMemory =
            Environment.OSVersion.Platform == PlatformID.Win32NT ?
                new CopyMemoryDelegate(RtlCopyMemory) :
                new CopyMemoryDelegate(memcpy);

        ////////////////////////////////////////////////////////////////////////

        [StructLayout(LayoutKind.Sequential)]
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

        private static int CalculateClrUsed(
            PixelFormats pixelFormat, short biPlanes, short biBitCount)
        {
            if (pixelFormat != PixelFormats.RGB)
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
            PixelFormats pixelFormat, short biPlanes, short biBitCount) =>
            sizeof(RAW_BITMAPINFOHEADER) +
            CalculateClrUsed(pixelFormat, biPlanes, biBitCount) * sizeof(RGBQUAD);

        private static unsafe int CalculateImageSize(
            int biWidth, int biHeight, short biPlanes, short biBitCount) =>
            ((biWidth * GetClrBits(biPlanes, biBitCount) + 31) & ~31) / 8 * biHeight;

        [StructLayout(LayoutKind.Sequential)]
        public struct RAW_BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public PixelFormats biCompression;
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
                NativeMethods.CalculateImageSize(this.biWidth, this.biHeight, this.biPlanes, this.biBitCount);
        }

        public struct BITMAPINFOHEADER
        {
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public PixelFormats biCompression;
            public RGBQUAD[]? bmiColors;

            // https://docs.microsoft.com/en-us/windows/win32/gdi/storing-an-image

            public int GetClrBits() =>
                NativeMethods.GetClrBits(this.biPlanes, this.biBitCount);
            public int CalculateClrUsed() =>
                NativeMethods.CalculateClrUsed(this.biCompression, this.biPlanes, this.biBitCount);
            public int CalculateRawSize() =>
                NativeMethods.CalculateRawSize(this.biCompression, this.biPlanes, this.biBitCount);
            public int CalculateImageSize() =>
                NativeMethods.CalculateImageSize(this.biWidth, this.biHeight, this.biPlanes, this.biBitCount);

            public unsafe RAW_BITMAPINFOHEADER* ToNative()
            {
                var biClrUsed = this.CalculateClrUsed();

                if (biClrUsed != (this.bmiColors?.Length ?? 0))
                {
                    throw new ArgumentException(
                        $"Invalid BITMAPINFOHEADER format: biClrUsed: {biClrUsed} != {this.bmiColors?.Length ?? 0}");
                }

                var biSize = this.CalculateRawSize();
                var pBih = Marshal.AllocCoTaskMem(biSize);

                var pBihRaw = (RAW_BITMAPINFOHEADER*)pBih;
                pBihRaw->biSize = biSize;
                pBihRaw->biWidth = this.biWidth;
                pBihRaw->biHeight = this.biHeight;
                pBihRaw->biPlanes = this.biPlanes;
                pBihRaw->biBitCount = this.biBitCount;
                pBihRaw->biCompression = this.biCompression;
                pBihRaw->biSizeImage = this.CalculateImageSize();
                pBihRaw->biXPelsPerMeter = 0;
                pBihRaw->biYPelsPerMeter = 0;
                pBihRaw->biClrUsed = this.bmiColors?.Length ?? 0;
                pBihRaw->biClrImportant = 0;

                if (pBihRaw->biClrUsed >= 1)
                {
                    fixed (RGBQUAD* pColor = this.bmiColors)
                    {
                        CopyMemory(
                            (IntPtr)(pBihRaw + 1),
                            (IntPtr)pColor,
                            (IntPtr)(biClrUsed * sizeof(RGBQUAD)));
                    }
                }

                return pBihRaw;
            }

            public static unsafe BITMAPINFOHEADER ToManaged(IntPtr p)
            {
                var pRawBih = (RAW_BITMAPINFOHEADER*)p.ToPointer();

                if (pRawBih->biSize < sizeof(RAW_BITMAPINFOHEADER))
                {
                    throw new ArgumentException(
                        $"Invalid BITMAPINFOHEADER format: biSize: {pRawBih->biSize} < {sizeof(RAW_BITMAPINFOHEADER)}");
                }

                var biSize = pRawBih->CalculateRawSize();
                if (pRawBih->biSize != biSize)
                {
                    throw new ArgumentException(
                        $"Invalid BITMAPINFOHEADER format: biSize: {biSize} != {pRawBih->biSize}");
                }

                var biClrUsed = pRawBih->CalculateClrUsed();
                if (biClrUsed != pRawBih->biClrUsed)
                {
                    throw new ArgumentException(
                        $"Invalid BITMAPINFOHEADER format: biClrUsed: {biClrUsed} != {pRawBih->biClrUsed}");
                }

                var biSizeImage = pRawBih->CalculateImageSize();
                if (biSizeImage != pRawBih->biSizeImage)
                {
                    throw new ArgumentException(
                        $"Invalid BITMAPINFOHEADER format: biSizeImage: {biSizeImage} != {pRawBih->biSizeImage}");
                }

                var bih = new BITMAPINFOHEADER();
                bih.biWidth = pRawBih->biWidth;
                bih.biHeight = pRawBih->biHeight;
                bih.biPlanes = pRawBih->biPlanes;
                bih.biBitCount = pRawBih->biBitCount;
                bih.biCompression = pRawBih->biCompression;

                bih.bmiColors = new RGBQUAD[biClrUsed];

                if (biClrUsed >= 1)
                {
                    fixed (RGBQUAD* pColor = bih.bmiColors)
                    {
                        CopyMemory(
                            (IntPtr)pColor,
                            (IntPtr)(pRawBih + 1),
                            (IntPtr)(biClrUsed * sizeof(RGBQUAD)));
                    }
                }

                return bih;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack=2)]
        public struct RAW_BITMAPFILEHEADER
        {
            public byte bfType0;
            public byte bfType1;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }

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
    }
}
