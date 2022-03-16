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

        [DllImport("libc")]
        private static extern void memcpy(IntPtr dest, IntPtr src, IntPtr length);

        public delegate void CopyMemoryDelegate(
            IntPtr pDestination, IntPtr pSource, IntPtr length);

        public static unsafe readonly CopyMemoryDelegate CopyMemory =
            CurrentPlatform == Platforms.Windows ?
                RtlCopyMemory : memcpy;

        ////////////////////////////////////////////////////////////////////////

        [DllImport("ole32")]
        private static extern IntPtr CoTaskMemAlloc(IntPtr size);
        [DllImport("ole32")]
        private static extern void CoTaskMemFree(IntPtr ptr);
        [DllImport("ntdll")]
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

        public static unsafe readonly AllocateMemoryDelegate AllocateMemory =
            CurrentPlatform == Platforms.Windows ?
                AllocateWindows : AllocatePosix;
        public static unsafe readonly FreeMemoryDelegate FreeMemory =
            CurrentPlatform == Platforms.Windows ?
                CoTaskMemFree : free;

        ////////////////////////////////////////////////////////////////////////

#if NETSTANDARD1_3
        public delegate int THREAD_START_ROUTINE(IntPtr parameter);
        public delegate void QueueUserWorkItemDelegate(THREAD_START_ROUTINE function, IntPtr parameter, int flags);

        [DllImport("kernel32", EntryPoint="QueueUserWorkItem")]
        private static extern void WindowsQueueUserWorkItem(
            THREAD_START_ROUTINE function, IntPtr parameter, int flags);

        private static void PosixQueueUserWorkItem(
            THREAD_START_ROUTINE function, IntPtr parameter, int flags) =>
            System.Threading.Tasks.Task.Run(() => function(parameter));

        public static readonly QueueUserWorkItemDelegate QueueUserWorkItem =
            CurrentPlatform == Platforms.Windows ?
                WindowsQueueUserWorkItem : PosixQueueUserWorkItem;
#endif

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
            sizeof(BITMAPINFOHEADER) +
            CalculateClrUsed(pixelFormat, biPlanes, biBitCount) * sizeof(RGBQUAD);

        private static unsafe int CalculateImageSize(
            PixelFormats pixelFormat,
            int biWidth, int biHeight, short biPlanes, short biBitCount) =>
            pixelFormat switch
            {
                PixelFormats.JPEG => 0,
                PixelFormats.PNG => 0,
                PixelFormats.MJPG => 0,
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

        public static unsafe VideoCharacteristics? CreateVideoCharacteristics(
            IntPtr pih, int framesPer1000Second)
        {
            var pBih = (BITMAPINFOHEADER*)pih.ToPointer();
            if (Enum.IsDefined(typeof(PixelFormats), pBih->biCompression))
            {
                return new VideoCharacteristics(
                    pBih->biCompression, pBih->biBitCount,
                    pBih->biWidth, pBih->biHeight, framesPer1000Second);
            }
            else
            {
                return null;
            }
        }
    }
}
