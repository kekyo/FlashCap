////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using FlashCap.Internal;
using FlashCap.Utilities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;

using static FlashCap.Internal.NativeMethods_V4L2;
using static FlashCap.Internal.V4L2.NativeMethods_V4L2_Interop;

namespace FlashCap.Devices;

[SupportedOSPlatform("linux")]
public sealed class V4L2Devices : CaptureDevices
{
    public V4L2Devices() :
        this(new DefaultBufferPool())
    {
    }

    public V4L2Devices(BufferPool defaultBufferPool) :
        base(defaultBufferPool)
    {
    }

    private static IEnumerable<v4l2_fmtdesc> EnumerateFormatDesc(
        int fd) =>
        Enumerable.Range(0, 1000).
        CollectWhile(index =>
        {
            var fmtdesc = Interop.Create_v4l2_fmtdesc();
            fmtdesc.index = (uint)index;
            fmtdesc.type = (uint)v4l2_buf_type.VIDEO_CAPTURE;
            
            return
                ioctl(fd, Interop.VIDIOC_ENUM_FMT, fmtdesc) == 0 ? (v4l2_fmtdesc?)fmtdesc : null;
        }).
        ToArray();    // Important: Iteration process must be continuous, avoid ioctl calls with other requests.

    private struct FrameSize
    {
        public int Width;
        public int Height;
        public bool IsDiscrete;
    }

    private static IEnumerable<FrameSize> EnumerateFrameSize(
        int fd, uint pixelFormat) =>
        EnumerateFrameSizes(fd, pixelFormat) is { Length: >= 1 } frameSizes ?
            frameSizes :
            // Some drivers, such as the ones of analog video grabbers, don't enumerate frame sizes:
            GetFrameSize(fd, pixelFormat);

    private static FrameSize[] EnumerateFrameSizes(
        int fd, uint pixelFormat) =>
        Enumerable.Range(0, 1000).
        CollectWhile(index =>
        {
            var frmsizeenum = Interop.Create_v4l2_frmsizeenum();
            frmsizeenum.index = (uint)index;
            frmsizeenum.pixel_format = pixelFormat;

            return ioctl(fd, Interop.VIDIOC_ENUM_FRAMESIZES, frmsizeenum) == 0 ?
                (v4l2_frmsizeenum?)frmsizeenum : null;
        }).
        // Expand when both stepwise and continuous:
        SelectMany(frmsizeenum =>
        {
            static IEnumerable<FrameSize> EnumerateStepWise(
                v4l2_frmsize_stepwise stepwise) =>
                NativeMethods.DefactoStandardResolutions.
                    Where(r =>
                        r.Width >= stepwise.min_width &&
                        r.Width <= stepwise.max_width &&
                        ((r.Width - stepwise.min_width) % stepwise.step_width) == 0 &&
                        r.Height >= stepwise.min_height &&
                        r.Height <= stepwise.max_height &&
                        ((r.Height - stepwise.min_height) % stepwise.step_height) == 0).
                    OrderByDescending(r => r).
                    Select(r => new FrameSize
                        { Width = r.Width, Height = r.Height, IsDiscrete = false, });

            static IEnumerable<FrameSize> EnumerateContinuous(
                v4l2_frmsize_stepwise stepwise) =>
                NativeMethods.DefactoStandardResolutions.
                    Where(r =>
                        r.Width >= stepwise.min_width &&
                        r.Width <= stepwise.max_width &&
                        r.Height >= stepwise.min_height &&
                        r.Height <= stepwise.max_height).
                    OrderByDescending(r => r).
                    Select(r => new FrameSize
                        { Width = r.Width, Height = r.Height, IsDiscrete = false, });

            return (v4l2_frmsizetypes)frmsizeenum.type switch
            {
                v4l2_frmsizetypes.DISCRETE =>
                    new[] { new FrameSize
                        { Width = (int)frmsizeenum.discrete.width, Height = (int)frmsizeenum.discrete.height, IsDiscrete = true, }, },
                v4l2_frmsizetypes.STEPWISE =>
                    EnumerateStepWise(frmsizeenum.stepwise),
                _ =>
                    EnumerateContinuous(frmsizeenum.stepwise),
            };
        }).
        ToArray();   // Important: Iteration process must be continuous, avoid ioctl calls with other requests.

    private struct FramesPerSecond
    {
        public Fraction Value;
        public bool IsDiscrete;
    }

    private static IEnumerable<FramesPerSecond> EnumerateFramesPerSecond(
        int fd, uint pixelFormat, int width, int height) =>
        EnumerateFrameIntervals(fd, pixelFormat, width, height) is { Length: >= 1 } framesPerSecond ?
            framesPerSecond :
            // Some drivers, such as the ones of analog video grabbers, don't enumerate frame intervals:
            GetFramesPerSecond(fd);

    private static FramesPerSecond[] EnumerateFrameIntervals(
        int fd, uint pixelFormat, int width, int height) =>
        Enumerable.Range(0, 1000).
        CollectWhile(index =>
        {
            var frmivalenum = Interop.Create_v4l2_frmivalenum();
            frmivalenum.index = (uint)index;
            frmivalenum.pixel_format = pixelFormat;
            frmivalenum.width = (uint)width;
            frmivalenum.height = (uint)height;

            return ioctl(fd, Interop.VIDIOC_ENUM_FRAMEINTERVALS, frmivalenum) == 0 ?
                (v4l2_frmivalenum?)frmivalenum : null;
        }).
        SelectMany(frmivalenum =>
        {
            
            static IEnumerable<FramesPerSecond> EnumerateStepWise(
                v4l2_frmival_stepwise stepwise)
            {
                // v4l2_fract is "interval", so do reciprocal to make fps.
                // (numerator <--> denominator)

                // Since we're inverting the interval to get the FPS, we also need to swap the meaning of min/max
                var min = new Fraction((int)stepwise.max.denominator, (int)stepwise.max.numerator);
                var max = new Fraction((int)stepwise.min.denominator, (int)stepwise.min.numerator);

                var step = new Fraction((int)stepwise.step.denominator, (int)stepwise.step.numerator);
                
                return NativeMethods.DefactoStandardFramesPerSecond.
                    Where(fps =>
                        fps >= min && fps <= max &&
                        ((fps - min) % step) == 0).
                    OrderByDescending(fps => fps).
                    Select(fps => new FramesPerSecond { Value = fps, IsDiscrete = false, });
            }

            static IEnumerable<FramesPerSecond> EnumerateContinuous(
                v4l2_frmival_stepwise stepwise)
            {
                // v4l2_fract is "interval", so do reciprocal to make fps.
                // (numerator <--> denominator)

                // Since we're inverting the interval to get the FPS, we also need to swap the meaning of min/max
                var min = new Fraction((int)stepwise.max.denominator, (int)stepwise.max.numerator);
                var max = new Fraction((int)stepwise.min.denominator, (int)stepwise.min.numerator);

                return NativeMethods.DefactoStandardFramesPerSecond.
                    Where(fps => fps >= min && fps <= max).
                    OrderByDescending(fps => fps).
                    Select(fps => new FramesPerSecond { Value = fps, IsDiscrete = false, });
            }

            return (v4l2_frmivaltypes)frmivalenum.type switch
            {
                v4l2_frmivaltypes.DISCRETE =>
                    new [] { new FramesPerSecond
                        { Value = new Fraction((int)frmivalenum.discrete.denominator, (int)frmivalenum.discrete.numerator), IsDiscrete = true, }, },
                v4l2_frmivaltypes.STEPWISE =>
                    EnumerateStepWise(frmivalenum.stepwise),
                _ =>
                    EnumerateContinuous(frmivalenum.stepwise),
            };
        }).
        ToArray();   // Important: Iteration process must be continuous, avoid ioctl calls with other requests.

    private static FrameSize[] GetFrameSize(
        int fd, uint pixelFormat)
    {
        var fmt_pix = Interop.Create_v4l2_pix_format();
        fmt_pix.width = 1920;
        fmt_pix.height = 1080;
        fmt_pix.pixelformat = pixelFormat;
        fmt_pix.field = (uint)v4l2_field.ANY;

        var format = Interop.Create_v4l2_format();
        format.type = (uint)v4l2_buf_type.VIDEO_CAPTURE;
        format.fmt_pix = fmt_pix;

        // The driver adjusts a tried format to the nearest size it supports.
        return ioctl(fd, Interop.VIDIOC_TRY_FMT, format) == 0 &&
            format.fmt_pix.pixelformat == pixelFormat &&
            format.fmt_pix.width >= 1 && format.fmt_pix.height >= 1 ?
            new[] { new FrameSize
                { Width = (int)format.fmt_pix.width, Height = (int)format.fmt_pix.height, IsDiscrete = true, }, } :
            ArrayEx.Empty<FrameSize>();
    }

    private static FramesPerSecond[] GetFramesPerSecond(
        int fd)
    {
        // Current frame rate:
        var streamparm = new byte[sizeof_v4l2_streamparm];
        Array.Copy(
            BitConverter.GetBytes((uint)v4l2_buf_type.VIDEO_CAPTURE), 0,
            streamparm, offsetof_v4l2_streamparm_type, sizeof(uint));
        if (ioctl(fd, Interop.VIDIOC_G_PARM, streamparm) == 0)
        {
            var numerator = BitConverter.ToUInt32(streamparm, offsetof_v4l2_streamparm_timeperframe_numerator);
            var denominator = BitConverter.ToUInt32(streamparm, offsetof_v4l2_streamparm_timeperframe_denominator);
            if (numerator >= 1 && denominator >= 1)
            {
                // timeperframe is "interval", so do reciprocal to make fps.
                return new[] { new FramesPerSecond
                    { Value = new Fraction((int)denominator, (int)numerator), IsDiscrete = true, }, };
            }
        }

        // Or the frame rate of the video standard:
        var std = new byte[sizeof(ulong)];
        if (ioctl(fd, Interop.VIDIOC_G_STD, std) == 0)
        {
            var id = BitConverter.ToUInt64(std, 0);
            if ((id & V4L2_STD_625_50) != 0)
            {
                return new[] { new FramesPerSecond { Value = new Fraction(25, 1), IsDiscrete = true, }, };
            }
            if ((id & V4L2_STD_525_60) != 0)
            {
                return new[] { new FramesPerSecond { Value = new Fraction(30000, 1001), IsDiscrete = true, }, };
            }
        }

        return ArrayEx.Empty<FramesPerSecond>();
    }

    private static string ToString(byte[] data)
    {
        var count = Array.IndexOf(data, (byte)0);
        var s = Encoding.UTF8.GetString(data);
        var str = Encoding.UTF8.GetString(data, 0, count);
        return str;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2046", Justification = "This backend does not require unreferenced code")]
    protected override IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors()
    {
        if (!NativeMethods.IsLinux())
        {
            throw new PlatformNotSupportedException("V4L2 capture requires Linux.");
        }

        return Directory.GetFiles("/dev", "video*").
        Collect(devicePath =>
        {
            if (open(devicePath, OPENBITS.O_RDWR) is { } fd && fd >= 0)
            {
                try
                {
                    var caps = Interop.Create_v4l2_capability();
                    if (ioctl(fd, Interop.VIDIOC_QUERYCAP, caps) >= 0 &&
                        (caps.capabilities & Interop.V4L2_CAP_VIDEO_CAPTURE) == Interop.V4L2_CAP_VIDEO_CAPTURE)
                    {
                        return (CaptureDeviceDescriptor)new V4L2DeviceDescriptor(
                            devicePath, ToString(caps.card), $"{ToString(caps.bus_info)}: {ToString(caps.driver)}",
                            EnumerateFormatDesc(fd).
                            SelectMany(fmtdesc =>
                                EnumerateFrameSize(fd, fmtdesc.pixelformat).
                                SelectMany(frmsize =>
                                    EnumerateFramesPerSecond(fd, fmtdesc.pixelformat, frmsize.Width, frmsize.Height).
                                    Collect(framesPerSecond =>
                                        NativeMethods_V4L2.CreateVideoCharacteristics(
                                            fmtdesc.pixelformat, frmsize.Width, frmsize.Height,
                                            framesPerSecond.Value, ToString(fmtdesc.description),
                                            frmsize.IsDiscrete && framesPerSecond.IsDiscrete)))).
                            Distinct().
                            OrderByDescending(vc => vc).
                            ToArray(),
                            this.DefaultBufferPool);
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    close(fd);
                }
            }
            else
            {
                return null;
            }
        });
    }
}
