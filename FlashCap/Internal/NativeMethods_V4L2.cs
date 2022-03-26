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
using FlashCap.Internal.V4L2;
using FlashCap.Utilities;

namespace FlashCap.Internal
{
    internal static class NativeMethods_V4L2
    {
        private static readonly RequestCode RequestCode =
            IntPtr.Size == 4 ? new RequestCode32() : new RequestCode64();

        public const int EINTR = 4;
        public const int EINVAL = 22;

        [StructLayout(LayoutKind.Sequential)]
        public struct timeval
        {
            public IntPtr tv_sec;
            public IntPtr tv_usec;
        }

        [Flags]
        public enum OPENBITS
        {
            O_RDONLY = 0,
            O_WRONLY = 1,
            O_RDWR = 2,
        }

        [DllImport("libc", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int open(
            [MarshalAs(UnmanagedType.LPStr)] string pathname, OPENBITS flag);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int read(
            int fd, byte[] buffer, int length);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int write(
            int fd, byte[] buffer, int count);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int close(int fd);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int pipe(int[] filedes);

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

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int poll(
            pollfd[] fds, int nfds, int timeout);

        [Flags]
        public enum PROT
        {
            NONE = 0,
            READ = 1,
            WRITE = 2,
            EXEC = 4,
        }

        [Flags]
        public enum MAP
        {
            SHARED = 1,
            PRIVATE = 2,
        }

        public static readonly IntPtr MAP_FAILED = (IntPtr)(-1);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern IntPtr mmap(
            IntPtr addr, IntPtr length, PROT prot, MAP flags, int fd, long offset);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int munmap(
            IntPtr addr, IntPtr length);

        private delegate int IoctlIn<T>(int fd, UIntPtr req, in T arg)
            where T : struct;

        private delegate int IoctlOut<T>(int fd, UIntPtr req, out T arg)
            where T : struct;

        private delegate int IoctlRef<T>(int fd, UIntPtr req, ref T arg)
            where T : struct;

        private static int do_ioctl<T>(
            int fd, UIntPtr req, in T arg, IoctlIn<T> ioctl)
            where T : struct
        {
            while (true)
            {
                var result = ioctl(fd, req, in arg);
                if (result < 0 && Marshal.GetLastWin32Error() == EINTR)
                {
                    continue;
                }

                return result;
            }
        }

        private static int do_ioctl<T>(
            int fd, UIntPtr req, out T arg, IoctlOut<T> ioctl)
            where T : struct
        {
            while (true)
            {
                var result = ioctl(fd, req, out arg);
                if (result < 0 && Marshal.GetLastWin32Error() == EINTR)
                {
                    continue;
                }

                return result;
            }
        }

        private static int do_ioctl<T>(
            int fd, UIntPtr req, ref T arg, IoctlRef<T> ioctl)
            where T : struct
        {
            while (true)
            {
                var result = ioctl(fd, req, ref arg);
                if (result < 0 && Marshal.GetLastWin32Error() == EINTR)
                {
                    continue;
                }

                return result;
            }
        }

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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct v4l2_capability
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string driver;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string card;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string bus_info;

            public int version;
            public v4l2_caps capabilities;
            public v4l2_caps device_caps;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public int[] reserved;
        }

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern int ioctl(
            int fd, UIntPtr request, out v4l2_capability caps);

        public static int ioctl(
            int fd, out v4l2_capability caps) =>
            do_ioctl(fd, RequestCode.VIDIOC_QUERYCAP, out caps, ioctl);

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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct v4l2_input
        {
            public int index;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string name;

            public v4l2_inputtype type;
            public int audioset;
            public int tuner;
            public v4l2_std_id std;
            public v4l2_inputstatus status;
            public v4l2_inputcapabilities capabilities;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public int[] reserved;
        }

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern int ioctl(
            int fd, UIntPtr request, out v4l2_input input);

        public static int ioctl(
            int fd, out v4l2_input input) =>
            do_ioctl(fd, RequestCode.VIDIOC_ENUMINPUT, out input, ioctl);

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
            BI_RGB = 0x00000000, // Compat Windows (maybe 24bit, invalid but useful)
            BI_JPEG = 0x00000004, // Compat Windows (invalid but useful)
            BI_PNG = 0x00000005, // Compat Windows (invalid but useful)
            RGB332 = 0x31424752, // RGB1
            RGB555 = 0x30424752, // RGB0
            RGB565 = 0x50424752, // RGBP
            RGB24 = 0x33424752, // RGB3
            XRGB32 = 0x34325842, // BX24
            ARGB32 = 0x34324142, // BA24
            ARGB = 0x42475241, // FOURCC, Compat Windows (invalid but useful)
            UYVY = 0x59565955,
            YUYV = 0x56595559,
            YUY2 = 0x32595559,
            MJPG = 0x47504A4D,
            JPEG = 0x4745504A,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct v4l2_fmtdesc
        {
            public int index;
            public v4l2_buf_type type;
            public v4l2_fmtflag flags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string description;

            public v4l2_pix_fmt pixelformat; // v4l2_pix_format_id

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] reserved;
        }

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern int ioctl(
            int fd, UIntPtr request, ref v4l2_fmtdesc fmtdesc);

        public static int ioctl(
            int fd, ref v4l2_fmtdesc fmtdesc) =>
            do_ioctl(fd, RequestCode.VIDIOC_ENUM_FMT, ref fmtdesc, ioctl);

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

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern int ioctl(
            int fd, UIntPtr request, ref v4l2_frmsizeenum frmsizeenum);

        public static int ioctl(
            int fd, ref v4l2_frmsizeenum frmsizeenum) =>
            do_ioctl(fd, RequestCode.VIDIOC_ENUM_FRAMESIZES, ref frmsizeenum, ioctl);

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

        [DllImport("libc", EntryPoint = "ioctl", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern int ioctl(
            int fd, UIntPtr request, ref v4l2_frmivalenum frmivalenum);

        public static int ioctl(
            int fd, ref v4l2_frmivalenum frmivalenum) =>
            do_ioctl(fd, RequestCode.VIDIOC_ENUM_FRAMEINTERVALS, ref frmivalenum, ioctl);

        ///////////////////////////////////////////////////////////

        public enum v4l2_field
        {
            ANY,
            NONE,
            TOP,
            BOTTOM,
            INTERLACED,
            SEQ_TB,
            SEQ_BT,
            ALTERNATE,
            INTERLACED_TB,
            INTERLACED_BT,
        }

        public enum v4l2_colorspace
        {
            DEFAULT,
            SMPTE170M,
            REC709,
            SRGB,
            OPRGB,
            BT2020,
            DCI_P3,
            SMPTE240M,
            SYSTEM_M,
            SYSTEM_BG,
            JPEG,
            RAW,
        }

        [Flags]
        public enum v4l2_pix_format_flag
        {
            PREMUL_ALPHA = 0x00000001,
        }

        public enum v4l2_quantization
        {
            DEFAULT,
            FULL_RANGE,
            LIM_RANGE,
        }

        public enum v4l2_xfer_func
        {
            DEFAULT,
            REC709,
            SRGB,
            OPRGB,
            SMPTE240M,
            NONE,
            DCI_P3,
            SMPTE2084,
        }

        public enum v4l2_ycbcr_encoding
        {
            DEFAULT,
            BT601,
            BT709,
            XV601,
            XV709,
            SYCC,
            BT2020,
            BT2020_CONST_LUM,
            SMPTE240M,
        }

        public enum v4l2_hsv_encoding
        {
            VALUE_180,
            VALUE_256,
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct v4l2_pix_format_encoding
        {
            [FieldOffset(0)]
            public v4l2_ycbcr_encoding ycbcr_enc;
            [FieldOffset(0)]
            public v4l2_hsv_encoding hsv_enc;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct v4l2_pix_format
        {
            public int width;
            public int height;
            public v4l2_pix_fmt pixelformat;
            public v4l2_field field;
            public int bytesperline;
            public int sizeimage;
            public v4l2_colorspace colorspace;
            public int priv;
            public v4l2_pix_format_flag flags;
            public v4l2_pix_format_encoding encoding;   // unnamed union { ... }
            public v4l2_quantization quantization;
            public v4l2_xfer_func xfer_func;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        private struct v4l2_format_fmt_raw_data200
        {
            private Guid raw_data0;
            private Guid raw_data16;
            private Guid raw_data32;
            private Guid raw_data48;
            private Guid raw_data64;
            private Guid raw_data80;
            private Guid raw_data96;
            private Guid raw_data112;
            private Guid raw_data128;
            private Guid raw_data144;
            private Guid raw_data160;
            private Guid raw_data176;
            private long raw_data192;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct v4l2_format_fmt
        {
            [FieldOffset(0)] public v4l2_pix_format pix;
            [FieldOffset(0)] private v4l2_format_fmt_raw_data200 raw_data;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct v4l2_format_type
        {
            [FieldOffset(0)] public v4l2_buf_type type;
            [FieldOffset(0)] private IntPtr __hack;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public struct v4l2_format
        {
            public v4l2_format_type type;   // HACK: Unknown padding insertion at after this field on 32bit environment...
            public v4l2_format_fmt fmt;
        }

        [DllImport("libc", EntryPoint = "ioctl", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern int ioctls(
            int fd, UIntPtr request, in v4l2_format format);

        public static int ioctls(
            int fd, in v4l2_format format) =>
            do_ioctl(fd, RequestCode.VIDIOC_S_FMT, in format, ioctls);

        [DllImport("libc", EntryPoint = "ioctl", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern int ioctlg(
            int fd, UIntPtr request, ref v4l2_format format);

        public static int ioctlg(
            int fd, ref v4l2_format format) =>
            do_ioctl(fd, RequestCode.VIDIOC_G_FMT, ref format, ioctlg);

        ///////////////////////////////////////////////////////////

        [Flags]
        public enum v4l2_buf_capabilities_supports
        {
            MMAP = 0x00000001,
            USERPTR = 0x00000002,
            DMABUF = 0x00000004,
            REQUESTS = 0x00000008,
            ORPHANED_BUFS = 0x00000010,
        }

        public enum v4l2_memory
        {
            MMAP = 1,
            USERPTR = 2,
            OVERLAY = 3,
            DMABUF = 4,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct v4l2_requestbuffers
        {
            public int count;
            public v4l2_buf_type type;
            public v4l2_memory memory;
            public v4l2_buf_capabilities_supports capabilities;
            public int reserved0;
        }

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern int ioctl(
            int fd, UIntPtr request, ref v4l2_requestbuffers requestbuffers);

        public static int ioctl(
            int fd, ref v4l2_requestbuffers requestbuffers) =>
            do_ioctl(fd, RequestCode.VIDIOC_REQBUFS, ref requestbuffers, ioctl);

        ///////////////////////////////////////////////////////////

        [StructLayout(LayoutKind.Explicit)]
        public struct v4l2_buffer_m
        {
            [FieldOffset(0)]
            public int offset;
            [FieldOffset(0)]
            public IntPtr userptr;
            [FieldOffset(0)]
            public IntPtr planes; // v4l2_plane*
            [FieldOffset(0)]
            public int fd;
        }

        public enum v4l2_timecode_types
        {
            FPS24 = 1,
            FPS25 = 2,
            FPS30 = 3,
            FPS50 = 4,
            FPS60 = 5,
        }
    
        [Flags]
        public enum v4l2_timecode_flags
        {
            DROPFRAME = 0x0001,
            COLORFRAME = 0x0002,
            field = 0x000c,    // lower case
            USERDEFINED = 0x0000,
            BIT8CHARS = 0x0008,
        }
    
        [StructLayout(LayoutKind.Sequential)]
        public struct v4l2_timecode
        {
            public v4l2_timecode_types type;
            public v4l2_timecode_flags flags;
            public byte frames;
            public byte seconds;
            public byte minutes;
            public byte hours;
            public byte userbits0;
            public byte userbits1;
            public byte userbits2;
            public byte userbits3;
        }

        [Flags]
        public enum v4l2_buffer_flags
        {
            MAPPED = 0x00000001,
            QUEUED = 0x00000002,
            DONE = 0x00000004,
            ERROR = 0x00000040,
            IN_REQUEST = 0x00000080,
            KEYFRAME = 0x00000008,
            PFRAME = 0x00000010,
            BFRAME = 0x00000020,
            TIMECODE = 0x00000100,
            PREPARED = 0x00000400,
            NO_CACHE_INVALIDATE = 0x00000800,
            NO_CACHE_CLEAN = 0x00001000,
            LAST = 0x00100000,
            REQUEST_FD = 0x00800000,
            TIMESTAMP_MASK = 0x0000e000,
            //TIMESTAMP_UNKNOWN = 0x00000000,
            TIMESTAMP_MONOTONIC = 0x00002000,
            TIMESTAMP_COPY = 0x00004000,
            TSTAMP_SRC_MASK = 0x00070000,
            //TSTAMP_SRC_EOF = 0x00000000,
            TSTAMP_SRC_SOE = 0x00010000,
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct v4l2_buffer
        {
            public int index;
            public v4l2_buf_type type;
            public int bytesused;
            public v4l2_buffer_flags flags;
            public v4l2_field field;
            public timeval timestamp;
            public v4l2_timecode timecode;
            public int sequence;
            public v4l2_memory memory;
            public v4l2_buffer_m m;
            public int length;
            public int reserved2;
            public int request_fd;
        }

        [DllImport("libc", EntryPoint="ioctl", CallingConvention=CallingConvention.Cdecl, SetLastError = true)]
        private static extern int ioctlr(
            int fd, UIntPtr request, ref v4l2_buffer buffer);
        public static int ioctl_querybuf(
            int fd, ref v4l2_buffer buffer) =>
            do_ioctl(fd, RequestCode.VIDIOC_QUERYBUF, ref buffer, ioctlr);
                  
        ///////////////////////////////////////////////////////////
                  
        [DllImport("libc", EntryPoint="ioctl", CallingConvention=CallingConvention.Cdecl, SetLastError = true)]
        private static extern int ioctli(
            int fd, UIntPtr request, in v4l2_buffer buffer);

        public static int ioctl_qbuf(
            int fd, in v4l2_buffer buffer) =>
            do_ioctl(fd, RequestCode.VIDIOC_QBUF, in buffer, ioctli);
                    
        public static int ioctl_dqbuf(
            int fd, in v4l2_buffer buffer) =>
            do_ioctl(fd, RequestCode.VIDIOC_DQBUF, in buffer, ioctli);
                
        ///////////////////////////////////////////////////////////
                
        [DllImport("libc", CallingConvention=CallingConvention.Cdecl, SetLastError = true)]
        private static extern int ioctl(
            int fd, UIntPtr request, in v4l2_buf_type type);

        public static int ioctl_streamon(
            int fd, v4l2_buf_type type) =>
            do_ioctl(fd, RequestCode.VIDIOC_STREAMON, in type, ioctl);

        public static int ioctl_streamoff(
            int fd, v4l2_buf_type type) =>
            do_ioctl(fd, RequestCode.VIDIOC_STREAMOFF, in type, ioctl);
        
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
                v4l2_pix_fmt.MJPG => PixelFormats.JPEG,
                v4l2_pix_fmt.JPEG => PixelFormats.JPEG,
                v4l2_pix_fmt.UYVY => PixelFormats.UYVY,
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

        public static v4l2_pix_fmt[] GetPixelFormats(
            PixelFormats pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormats.RGB8:
                    return new[] { v4l2_pix_fmt.RGB332 };
                case PixelFormats.RGB15:
                    return new[] { v4l2_pix_fmt.RGB555 };
                case PixelFormats.RGB16:
                    return new[] { v4l2_pix_fmt.RGB565 };
                case PixelFormats.RGB24:
                    return new[] { v4l2_pix_fmt.RGB24, v4l2_pix_fmt.BI_RGB };
                case PixelFormats.RGB32:
                    return new[] { v4l2_pix_fmt.XRGB32 };
                case PixelFormats.ARGB32:
                    return new[] { v4l2_pix_fmt.ARGB32, v4l2_pix_fmt.ARGB };
                case PixelFormats.UYVY:
                    return new[] { v4l2_pix_fmt.UYVY };
                case PixelFormats.YUYV:
                    return new[] { v4l2_pix_fmt.YUYV, v4l2_pix_fmt.YUY2 };
                case PixelFormats.JPEG:
                    return new[] { v4l2_pix_fmt.MJPG, v4l2_pix_fmt.JPEG, v4l2_pix_fmt.BI_JPEG };
                case PixelFormats.PNG:
                    return new[] { v4l2_pix_fmt.BI_PNG };
                default:
                    return ArrayEx.Empty<v4l2_pix_fmt>();
            }
        }
    }
}
