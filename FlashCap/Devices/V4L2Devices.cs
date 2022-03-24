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
    public sealed class V4L2Devices : ICaptureDevices
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

        private static IEnumerable<NativeMethods_V4L2.v4l2_frmsize_discrete> EnumerateFrameSize(
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
                static IEnumerable<NativeMethods_V4L2.v4l2_frmsize_discrete> EnumerateStepWise(
                    NativeMethods_V4L2.v4l2_frmsize_stepwise stepwise)
                {
                    for (var height = stepwise.min_height;
                         height <= stepwise.max_height;
                         height += stepwise.step_height)
                    {
                        for (var width = stepwise.min_width;
                             width <= stepwise.max_width;
                             width += stepwise.step_width)
                        {
                            yield return new NativeMethods_V4L2.v4l2_frmsize_discrete
                            {
                                width = width,
                                height = height,
                            };
                        }
                    }
                }

                static IEnumerable<NativeMethods_V4L2.v4l2_frmsize_discrete> EnumerateContinuous(
                    NativeMethods_V4L2.v4l2_frmsize_stepwise stepwise) =>
                    NativeMethods.DefactoStandardResolutions.
                        Where(r =>
                            r.Width >= stepwise.min_width &&
                            r.Width <= stepwise.max_height &&
                            r.Height >= stepwise.min_height &&
                            r.Height <= stepwise.max_height).
                        Select(r => new NativeMethods_V4L2.v4l2_frmsize_discrete
                            { width = r.Width, height = r.Height });

                var fse = frmsizeenum!.Value;
                return fse.type switch
                {
                    NativeMethods_V4L2.v4l2_frmsizetypes.DISCRETE =>
                        new[] { fse.discrete },
                    NativeMethods_V4L2.v4l2_frmsizetypes.STEPWISE =>
                        EnumerateStepWise(fse.stepwise),
                    _ =>
                        EnumerateContinuous(fse.stepwise),
                };
            }).
            ToArray();   // Important: Iteration process must be continuous, avoid ioctl calls with other requests.

        private static IEnumerable<Fraction> EnumerateFramesPerSecond(
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
                static IEnumerable<Fraction> EnumerateStepWise(
                    NativeMethods_V4L2.v4l2_frmival_stepwise stepwise)
                {
                    var minfract = new Fraction(
                        stepwise.min.denominator, stepwise.min.numerator);
                    var maxfract = new Fraction(
                        stepwise.max.denominator, stepwise.max.numerator);
                    var stepfract = new Fraction(
                        stepwise.step.denominator, stepwise.step.numerator);
                    var index = 0;
                    while (true)
                    {
                        var fract = minfract + stepfract * index;
                        if (fract > maxfract)
                        {
                            break;
                        }
                        yield return fract;
                        index++;
                    }
                }

                static IEnumerable<Fraction> EnumerateContinuous(
                    NativeMethods_V4L2.v4l2_frmival_stepwise stepwise)
                {
                    var min = new Fraction(stepwise.min.denominator, stepwise.min.numerator);
                    var max = new Fraction(stepwise.max.denominator, stepwise.max.numerator);
                    return NativeMethods.DefactoStandardFramesPerSecond.
                        Where(fps => fps >= min && fps <= max);
                }

                var fie = frmivalenum!.Value;
                return fie.type switch
                {
                    NativeMethods_V4L2.v4l2_frmivaltypes.DISCRETE =>
                        new [] { new Fraction(fie.discrete.denominator, fie.discrete.numerator) },
                    NativeMethods_V4L2.v4l2_frmivaltypes.STEPWISE =>
                        EnumerateStepWise(fie.stepwise),
                    _ =>
                        EnumerateContinuous(fie.stepwise),
                };
            }).
            ToArray();   // Important: Iteration process must be continuous, avoid ioctl calls with other requests.

        public IEnumerable<ICaptureDeviceDescriptor> EnumerateDescriptors() =>
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
                            return (ICaptureDeviceDescriptor)new V4L2DeviceDescriptor(
                                devicePath, caps.card, $"{caps.bus_info}: {caps.driver}",
                                EnumerateFormatDesc(fd).
                                SelectMany(fmtdesc =>
                                    EnumerateFrameSize(fd, fmtdesc.pixelformat).
                                    SelectMany(frmsize =>
                                        EnumerateFramesPerSecond(fd, fmtdesc.pixelformat, frmsize.width, frmsize.height).
                                        Collect(framesPerSecond =>
                                            NativeMethods_V4L2.CreateVideoCharacteristics(
                                                fmtdesc.pixelformat, frmsize.width, frmsize.height,
                                                framesPerSecond, fmtdesc.description)))).
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
