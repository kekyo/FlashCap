////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using FlashCap.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlashCap.Devices
{
    public sealed class V4L2Devices : CaptureDevices
    {
        private static IEnumerable<NativeMethods_V4L2.v4l2_fmtdesc> EnumerateFormatDesc(
            int fd) =>
            Enumerable.Range(0, 1000).
            CollectWhile(index =>
            {
                var fmtdesc = new NativeMethods_V4L2.v4l2_fmtdesc
                {
                    index = index,
                    type = NativeMethods_V4L2.v4l2_buf_type.VIDEO_CAPTURE,
                };
                return NativeMethods_V4L2.ioctl(fd, ref fmtdesc) == 0 &&
                    Enum.IsDefined(typeof(NativeMethods_V4L2.v4l2_pix_fmt), fmtdesc.pixelformat) ?
                    (NativeMethods_V4L2.v4l2_fmtdesc?)fmtdesc : null;
            }).
            Select(fmtdesc => fmtdesc!.Value).
            ToArray();   // Important: Iteration process must be continuous, avoid ioctl calls with other requests.

        private struct FrameSize
        {
            public int Width;
            public int Height;
            public bool IsDiscrete;
        }

        private static IEnumerable<FrameSize> EnumerateFrameSize(
            int fd, NativeMethods_V4L2.v4l2_pix_fmt pixelFormat) =>
            Enumerable.Range(0, 1000).
            CollectWhile(index =>
            {
                var frmsizeenum = new NativeMethods_V4L2.v4l2_frmsizeenum
                {
                    index = index,
                    pixel_format = pixelFormat,
                };
                return NativeMethods_V4L2.ioctl(fd, ref frmsizeenum) == 0 ?
                    (NativeMethods_V4L2.v4l2_frmsizeenum?)frmsizeenum : null;
            }).
            // Expand when both stepwise and continuous:
            SelectMany(frmsizeenum =>
            {
                static IEnumerable<FrameSize> EnumerateStepWise(
                    NativeMethods_V4L2.v4l2_frmsize_stepwise stepwise) =>
                    NativeMethods.DefactoStandardResolutions.
                        Where(r =>
                            r.Width >= stepwise.min_width &&
                            r.Width <= stepwise.max_height &&
                            (r.Width - stepwise.min_width % stepwise.step_width) == 0 &&
                            r.Height >= stepwise.min_height &&
                            r.Height <= stepwise.max_height &&
                            (r.Height - stepwise.min_height % stepwise.step_height) == 0).
                        OrderByDescending(r => r).
                        Select(r => new FrameSize
                            { Width = r.Width, Height = r.Height, IsDiscrete = false, });

                static IEnumerable<FrameSize> EnumerateContinuous(
                    NativeMethods_V4L2.v4l2_frmsize_stepwise stepwise) =>
                    NativeMethods.DefactoStandardResolutions.
                        Where(r =>
                            r.Width >= stepwise.min_width &&
                            r.Width <= stepwise.max_height &&
                            r.Height >= stepwise.min_height &&
                            r.Height <= stepwise.max_height).
                        OrderByDescending(r => r).
                        Select(r => new FrameSize
                            { Width = r.Width, Height = r.Height, IsDiscrete = false, });

                var fse = frmsizeenum!.Value;
                return fse.type switch
                {
                    NativeMethods_V4L2.v4l2_frmsizetypes.DISCRETE =>
                        new[] { new FrameSize
                            { Width = fse.discrete.width, Height = fse.discrete.height, IsDiscrete = true, }, },
                    NativeMethods_V4L2.v4l2_frmsizetypes.STEPWISE =>
                        EnumerateStepWise(fse.stepwise),
                    _ =>
                        EnumerateContinuous(fse.stepwise),
                };
            }).
            ToArray();   // Important: Iteration process must be continuous, avoid ioctl calls with other requests.

        private struct FramesPerSecond
        {
            public Fraction Value;
            public bool IsDiscrete;
        }

        private static IEnumerable<FramesPerSecond> EnumerateFramesPerSecond(
            int fd, NativeMethods_V4L2.v4l2_pix_fmt pixelFormat, int width, int height) =>
            Enumerable.Range(0, 1000).
            CollectWhile(index =>
            {
                var frmivalenum = new NativeMethods_V4L2.v4l2_frmivalenum
                {
                    index = index,
                    pixel_format = pixelFormat,
                    width = width,
                    height = height,
                };
                return NativeMethods_V4L2.ioctl(fd, ref frmivalenum) == 0 ?
                    (NativeMethods_V4L2.v4l2_frmivalenum?)frmivalenum : null;
            }).
            SelectMany(frmivalenum =>
            {
                // v4l2_fract is "interval", so makes fps to do reciprocal.
                // (numerator <--> denominator)
                static IEnumerable<FramesPerSecond> EnumerateStepWise(
                    NativeMethods_V4L2.v4l2_frmival_stepwise stepwise)
                {
                    var min = new Fraction(stepwise.min.denominator, stepwise.min.numerator);
                    var max = new Fraction(stepwise.max.denominator, stepwise.max.numerator);
                    var step = new Fraction(stepwise.step.denominator, stepwise.step.numerator);
                    return NativeMethods.DefactoStandardFramesPerSecond.
                        Where(fps =>
                            fps >= min && fps <= max &&
                            ((fps - min) % step) == 0).
                        OrderByDescending(fps => fps).
                        Select(fps => new FramesPerSecond { Value = fps, IsDiscrete = false, });
                }

                static IEnumerable<FramesPerSecond> EnumerateContinuous(
                    NativeMethods_V4L2.v4l2_frmival_stepwise stepwise)
                {
                    var min = new Fraction(stepwise.min.denominator, stepwise.min.numerator);
                    var max = new Fraction(stepwise.max.denominator, stepwise.max.numerator);
                    return NativeMethods.DefactoStandardFramesPerSecond.
                        Where(fps => fps >= min && fps <= max).
                        OrderByDescending(fps => fps).
                        Select(fps => new FramesPerSecond { Value = fps, IsDiscrete = false, });
                }

                var fie = frmivalenum!.Value;
                return fie.type switch
                {
                    NativeMethods_V4L2.v4l2_frmivaltypes.DISCRETE =>
                        new [] { new FramesPerSecond
                            { Value = new Fraction(fie.discrete.denominator, fie.discrete.numerator), IsDiscrete = true, }, },
                    NativeMethods_V4L2.v4l2_frmivaltypes.STEPWISE =>
                        EnumerateStepWise(fie.stepwise),
                    _ =>
                        EnumerateContinuous(fie.stepwise),
                };
            }).
            ToArray();   // Important: Iteration process must be continuous, avoid ioctl calls with other requests.

        public override IEnumerable<CaptureDeviceDescriptor> EnumerateDescriptors() =>
            Directory.GetFiles("/dev", "video*").
            Collect(devicePath =>
            {
                if (NativeMethods_V4L2.open(
                    devicePath, NativeMethods_V4L2.OPENBITS.O_RDWR) is { } fd && fd >= 0)
                {
                    try
                    {
                        if (NativeMethods_V4L2.ioctl(fd, out NativeMethods_V4L2.v4l2_capability caps) >= 0 &&
                            (caps.capabilities & NativeMethods_V4L2.v4l2_caps.VIDEO_CAPTURE) == NativeMethods_V4L2.v4l2_caps.VIDEO_CAPTURE)
                        {
                            return (CaptureDeviceDescriptor)new V4L2DeviceDescriptor(
                                devicePath, caps.card, $"{caps.bus_info}: {caps.driver}",
                                EnumerateFormatDesc(fd).
                                SelectMany(fmtdesc =>
                                    EnumerateFrameSize(fd, fmtdesc.pixelformat).
                                    SelectMany(frmsize =>
                                        EnumerateFramesPerSecond(fd, fmtdesc.pixelformat, frmsize.Width, frmsize.Height).
                                        Collect(framesPerSecond =>
                                            NativeMethods_V4L2.CreateVideoCharacteristics(
                                                fmtdesc.pixelformat, frmsize.Width, frmsize.Height,
                                                framesPerSecond.Value, fmtdesc.description,
                                                frmsize.IsDiscrete && framesPerSecond.IsDiscrete)))).
                                Distinct().
                                OrderByDescending(vc => vc).
                                ToArray());
                        }
                        else
                        {
                            return null;
                        }
                    }
                    finally
                    {
                        NativeMethods_V4L2.close(fd);
                    }
                }
                else
                {
                    return null;
                }
            });
    }
}
