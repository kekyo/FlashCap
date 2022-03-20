////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using FlashCap.Utilities;

namespace FlashCap.Internal
{
    internal static class NativeMethods_V4L2
    {
        [Flags]
        public enum OPENBITS
        {
            O_RDONLY = 0,
            O_WRONLY = 1,
            O_RDWR = 2,
        }
        
        [DllImport("libc", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
        public static extern int open(
            [MarshalAs(UnmanagedType.LPStr)] string pathname, OPENBITS flag);
        [DllImport("libc", CallingConvention=CallingConvention.Cdecl)]
        public static extern int read(
            int fd, byte[] buffer, int length);
        [DllImport("libc", CallingConvention=CallingConvention.Cdecl)]
        public static extern int write(
            int fd, byte[] buffer, int count);
        [DllImport("libc", CallingConvention=CallingConvention.Cdecl)]
        public static extern int close(int fd);

        [Flags]
        public enum POLLBITS : short
        {
            POLLIN = 0x01,
            POLLPRI = 0x02,
            POLLOUT = 0x04,
            POLLERR = 0x08,
            POLLHUP = 0x10,
            POLLNVAL = 0x20,
            POLLRDNORM = 0x40,
            POLLRDBAND = 0x80,
            POLLWRNORM = 0x100,
            POLLWRBAND = 0x200,
            POLLMSG = 0x400,
            POLLREMOVE = 0x1000,
            POLLRDHUP = 0x2000,
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct pollfd
        {
            public int fd;
            public POLLBITS events;
            public POLLBITS revents;
        }
        
        [DllImport("libc", CallingConvention=CallingConvention.Cdecl)]
        public static extern int poll(
            pollfd[] fds, int nfds, int timeout);
        
        ///////////////////////////////////////////////////////////

        [Flags]
        public enum v4l2_caps : uint
        {
            VIDEO_CAPTURE = 0x00000001,
            VIDEO_CAPTURE_MPLANE = 0x00001000,
            VIDEO_OUTPUT = 0x00000002,
            VIDEO_OUTPUT_MPLANE = 0x00002000, 
            VIDEO_M2M = 0x00004000,
            VIDEO_M2M_MPLANE = 0x00008000,
            VIDEO_OVERLAY = 0x00000004,
            VBI_CAPTURE = 0x00000010,
            VBI_OUTPUT = 0x00000020,
            SLICED_VBI_CAPTURE = 0x00000040,
            SLICED_VBI_OUTPUT = 0x00000080,
            RDS_CAPTURE = 0x00000100,
            VIDEO_OUTPUT_OVERLAY = 0x00000200,
            HW_FREQ_SEEK = 0x00000400,
            RDS_OUTPUT = 0x00000800,
            TUNER = 0x00010000,
            AUDIO = 0x00020000,
            RADIO = 0x00040000,
            MODULATOR = 0x00080000,
            SDR_CAPTURE = 0x00100000,
            EXT_PIX_FORMAT = 0x00200000,
            SDR_OUTPUT = 0x00400000,
            META_CAPTURE = 0x00800000,
            READWRITE = 0x01000000,
            ASYNCIO = 0x02000000,
            STREAMING = 0x04000000,
            META_OUTPUT = 0x08000000,
            TOUCH = 0x10000000,
            DEVICE_CAPS = 0x80000000,
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
        public struct v4l2_capability
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=16)] public string driver;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string card;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string bus_info;
            public int version;
            public v4l2_caps capabilities;
            public v4l2_caps device_caps;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=3)] public int[] reserved;
        }
        
        [DllImport("libc", EntryPoint="ioctl", CallingConvention=CallingConvention.Cdecl)]
        private static extern int ioctl(
            int fd, uint request, out v4l2_capability caps);
        private const uint VIDIOC_QUERYCAP = 0x80685600;
        public static int ioctl(
            int fd, out v4l2_capability caps) =>
            ioctl(fd, VIDIOC_QUERYCAP, out caps);
          
        ///////////////////////////////////////////////////////////

        public enum v4l2_inputtype
        {
            TUNER = 1,
            CAMERA = 2,
            TOUCH = 3,
        }

        [Flags]
        public enum v4l2_inputstatus
        {
            NO_POWER = 0x00000001,
            NO_SIGNAL = 0x00000002,
            NO_COLOR = 0x00000004,
            HFLIP = 0x00000010,
            VFLIP = 0x00000020,
            NO_H_LOCK = 0x00000100,
            COLOR_KILL = 0x00000200,
            NO_V_LOCK = 0x00000400,
            NO_STD_LOCK = 0x00000800,
            NO_SYNC = 0x00010000,
            NO_EQU = 0x00020000,
            NO_CARRIER = 0x00040000,
            MACROVISION = 0x01000000,
            NO_ACCESS = 0x02000000,
            VTR = 0x04000000,
        }

        [Flags]
        public enum v4l2_inputcapabilities
        {
            DV_TIMINGS = 0x00000002,
            STD = 0x00000004,
            NATIVE_SIZE = 0x00000008,
        }

        [Flags]
        public enum v4l2_std_id : long
        {
            PAL_B = 0x00000001,
            PAL_B1 = 0x00000002,
            PAL_G = 0x00000004,
            PAL_H = 0x00000008,
            PAL_I = 0x00000010,
            PAL_D = 0x00000020,
            PAL_D1 = 0x00000040,
            PAL_K = 0x00000080,
            PAL_M = 0x00000100,
            PAL_N = 0x00000200,
            PAL_Nc = 0x00000400,
            PAL_60 = 0x00000800,
            NTSC_M = 0x00001000,
            NTSC_M_JP = 0x00002000,
            NTSC_443 = 0x00004000,
            NTSC_M_KR = 0x00008000,
            SECAM_B = 0x00010000,
            SECAM_D = 0x00020000,
            SECAM_G = 0x00040000,
            SECAM_H = 0x00080000,
            SECAM_K = 0x00100000,
            SECAM_K1 = 0x00200000,
            SECAM_L = 0x00400000,
            SECAM_LC = 0x00800000,
            ATSC_8_VSB = 0x01000000,
            ATSC_16_VSB = 0x02000000,
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
        public struct v4l2_input
        {
            public int index;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string name;
            public v4l2_inputtype type;
            public int audioset;
            public int tuner;
            public v4l2_std_id std;
            public v4l2_inputstatus status;
            public v4l2_inputcapabilities capabilities;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=3)] public int[] reserved;
        }

        [DllImport("libc", EntryPoint="ioctl", CallingConvention=CallingConvention.Cdecl)]
        private static extern int ioctl(
            int fd, uint request, out v4l2_input input);
        private const uint VIDIOC_ENUMINPUT = 0xc04c561a;
        public static int ioctl(
            int fd, out v4l2_input input) =>
            ioctl(fd, VIDIOC_ENUMINPUT, out input);
                  
        ///////////////////////////////////////////////////////////

        public enum v4l2_buf_type
        {
            VIDEO_CAPTURE = 1,
            VIDEO_OUTPUT = 2,
            VIDEO_OVERLAY = 3,
            VBI_CAPTURE = 4,
            VBI_OUTPUT = 5,
            SLICED_VBI_CAPTURE = 6,
            SLICED_VBI_OUTPUT = 7,
            VIDEO_OUTPUT_OVERLAY = 8,
            VIDEO_CAPTURE_MPLANE = 9,
            VIDEO_OUTPUT_MPLANE = 10,
            SDR_CAPTURE = 11,
            SDR_OUTPUT = 12,
            META_CAPTURE = 13,
            META_OUTPUT = 14,
        }
        
        [Flags]
        public enum v4l2_fmtflag
        {
            COMPRESSED = 0x0001,
            EMULATED = 0x0002,
        }
        
        public enum v4l2_pix_fmt
        {
            BI_RGB = 0x00000000,    // Compat Windows (maybe 24bit, invalid but useful)
            BI_JPEG = 0x00000004,   // Compat Windows (invalid but useful)
            BI_PNG = 0x00000005,    // Compat Windows (invalid but useful)
            RGB332 = 0x31424752,    // RGB1
            RGB555 = 0x30424752,    // RGB0
            RGB565 = 0x50424752,    // RGBP
            RGB24 = 0x33424752,     // RGB3
            XRGB32 = 0x34325842,    // BX24
            ARGB32 = 0x34324142,    // BA24
            ARGB = 0x42475241,      // FOURCC, Compat Windows (invalid but useful)
            RGB2 = 0x32424752,      // FOURCC, Compat Windows (invalid but useful)
            YUY2 = 0x32595559,
            YUYV = 0x56595559,
            UYUY = 0x59565955,
            MJPG = 0x47504A4D,
            JPEG = 0x4745504A,
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
        public struct v4l2_fmtdesc
        {
            public int index;
            public v4l2_buf_type type;
            public v4l2_fmtflag flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string description;
            public v4l2_pix_fmt pixelformat;   // v4l2_pix_format_id
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)] public int[] reserved;
        }

        [DllImport("libc", EntryPoint="ioctl", CallingConvention=CallingConvention.Cdecl)]
        private static extern int ioctl(
            int fd, uint request, ref v4l2_fmtdesc fmtdesc);
        private const uint VIDIOC_ENUM_FMT = 0xc0405602;
        public static int ioctl(
            int fd, ref v4l2_fmtdesc fmtdesc) =>
            ioctl(fd, VIDIOC_ENUM_FMT, ref fmtdesc);
                  
        ///////////////////////////////////////////////////////////

        [StructLayout(LayoutKind.Sequential)]
        public struct v4l2_frmsize_discrete
        {
            public int width;
            public int height;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct v4l2_frmsize_stepwise
        {
            public int min_width;
            public int max_width;
            public int step_width;
            public int min_height;
            public int max_height;
            public int step_height;
        }

        public enum v4l2_frmsizetypes
        {
            DISCRETE = 1,
            CONTINUOUS = 2,
            STEPWISE = 3,
        }
        
        [StructLayout(LayoutKind.Explicit)]
        public struct v4l2_frmsizeenum
        {
            [FieldOffset(0)] public int index;
            [FieldOffset(4)] public v4l2_pix_fmt pixel_format;
            [FieldOffset(8)] public v4l2_frmsizetypes type;
            [FieldOffset(12)] public v4l2_frmsize_discrete discrete;
            [FieldOffset(12)] public v4l2_frmsize_stepwise stepwise;
            [FieldOffset(36)] public int reserved0;
            [FieldOffset(40)] public int reserved1;
        }

        [DllImport("libc", EntryPoint="ioctl", CallingConvention=CallingConvention.Cdecl)]
        private static extern int ioctl(
            int fd, uint request, ref v4l2_frmsizeenum frmsizeenum);
        private const uint VIDIOC_ENUM_FRAMESIZES = 0xc02c564a;
        public static int ioctl(
            int fd, ref v4l2_frmsizeenum frmsizeenum) =>
            ioctl(fd, VIDIOC_ENUM_FRAMESIZES, ref frmsizeenum);

        ///////////////////////////////////////////////////////////

        [StructLayout(LayoutKind.Sequential)]
        public struct v4l2_fract
        {
            public int numerator;
            public int denominator;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct v4l2_frmival_stepwise
        {
            public v4l2_fract min;
            public v4l2_fract max;
            public v4l2_fract step;
        }
       
        public enum v4l2_frmivaltypes
        {
            DISCRETE = 1,
            CONTINUOUS = 2,
            STEPWISE = 3,
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct v4l2_frmivalenum
        {
            [FieldOffset(0)] public int index;
            [FieldOffset(4)] public v4l2_pix_fmt pixel_format;
            [FieldOffset(8)] public int width;
            [FieldOffset(12)] public int height;
            [FieldOffset(16)] public v4l2_frmivaltypes type;
            [FieldOffset(20)] public v4l2_fract discrete;
            [FieldOffset(20)] public v4l2_frmival_stepwise stepwise;
            [FieldOffset(44)] public int reserved0;
            [FieldOffset(48)] public int reserved1;
        }
        
        [DllImport("libc", EntryPoint="ioctl", CallingConvention=CallingConvention.Cdecl)]
        private static extern int ioctl(
            int fd, uint request, ref v4l2_frmivalenum frmivalenum);
        private const uint VIDIOC_ENUM_FRAMEINTERVALS = 0xc034564b;
        public static int ioctl(
            int fd, ref v4l2_frmivalenum frmivalenum) =>
            ioctl(fd, VIDIOC_ENUM_FRAMEINTERVALS, ref frmivalenum);
                       
        ///////////////////////////////////////////////////////////
        
        public static VideoCharacteristics? CreateVideoCharacteristics(
            v4l2_pix_fmt pix_fmt,
            int width, int height,
            Fraction framesPerSecond,
            string description)
        {
            if (pix_fmt switch
            {
                v4l2_pix_fmt.BI_RGB => (PixelFormats?)PixelFormats.RGB24,
                v4l2_pix_fmt.BI_JPEG => PixelFormats.JPEG,
                v4l2_pix_fmt.BI_PNG => PixelFormats.PNG,
                v4l2_pix_fmt.RGB332 => PixelFormats.RGB8,
                v4l2_pix_fmt.RGB555 => PixelFormats.RGB15,
                v4l2_pix_fmt.RGB565 => PixelFormats.RGB16,
                v4l2_pix_fmt.RGB24 => PixelFormats.RGB24,
                v4l2_pix_fmt.XRGB32 => PixelFormats.RGB32,
                v4l2_pix_fmt.ARGB32 => PixelFormats.ARGB32,
                v4l2_pix_fmt.ARGB => PixelFormats.ARGB32,
                v4l2_pix_fmt.RGB2 => PixelFormats.RGB24,
                v4l2_pix_fmt.MJPG => PixelFormats.JPEG,
                v4l2_pix_fmt.JPEG => PixelFormats.JPEG,
                v4l2_pix_fmt.UYUY => PixelFormats.UYVY,
                v4l2_pix_fmt.YUYV => PixelFormats.YUYV,
                v4l2_pix_fmt.YUY2 => PixelFormats.YUYV,
                _ => null,
            } is { } pixelFormat)
            {
                return new VideoCharacteristics(
                    pixelFormat, width, height,
                    framesPerSecond.Reduce(),
                    description,
                    NativeMethods.GetFourCCString((int)pix_fmt));
            }
            else
            {
                return null;
            }
        }
    }
}
