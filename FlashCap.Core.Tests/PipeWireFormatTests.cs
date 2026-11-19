////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////


using FlashCap.Internal.PipeWire;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed class PipeWireFormatTests
{
    // Native SPA_TYPE_OBJECT_Format with a discrete 4x2, 30fps video format.
    // Values use the published SPA ABI, independently of FlashCap's POD writer.
    private static uint[] NativeFormat(uint format) => new uint[]
    {
        128, 15, 0x40003, 3,
        1, 0, 4, 3, 2, 0,
        2, 0, 4, 3, 1, 0,
        0x20001, 0, 4, 3, format, 0,
        0x20003, 0, 8, 10, 4, 2,
        0x20004, 0, 8, 11, 30, 1,
    };

    [TestCase(4u, PixelFormats.YUYV)]
    [TestCase(5u, PixelFormats.UYVY)]
    [TestCase(8u, PixelFormats.RGB32)]
    [TestCase(12u, PixelFormats.ARGB32)]
    [TestCase(16u, PixelFormats.RGB24)]
    [TestCase(23u, PixelFormats.NV12)]
    public void NativeFormatsHaveTheExpectedFlashCapMeaning(uint native, PixelFormats expected)
    {
        var pod = NativeFormat(native).SelectMany(BitConverter.GetBytes).ToArray();
        var format = PipeWireFormats.Parse(pod).Single().Characteristics;
        Assert.That(format.PixelFormat, Is.EqualTo(expected));
        Assert.That(format.Width, Is.EqualTo(4));
        Assert.That(format.Height, Is.EqualTo(2));
        Assert.That(format.FramesPerSecond.Numerator, Is.EqualTo(30));
    }

    [Test]
    public void Nv21IsNotAdvertisedAsNv12()
    {
        var pod = NativeFormat(24).SelectMany(BitConverter.GetBytes).ToArray();
        Assert.That(PipeWireFormats.Parse(pod), Is.Empty);
    }

    [TestCase(0u)]
    [TestCase(1u)]
    public void DmaBufFormatsAreNotAdvertisedAsCpuReadableLinearFrames(uint modifier)
    {
        var words = NativeFormat(23).Concat(new uint[] { 0x20002, 8, 8, 5, modifier, 0 }).ToArray();
        words[0] = (uint)words.Length * 4 - 8;
        Assert.That(PipeWireFormats.Parse(words.SelectMany(BitConverter.GetBytes).ToArray()), Is.Empty);
    }

    [Test]
    public void FrameRateRangesIncludeOnlySupportedValues()
    {
        var words = NativeFormat(23).Take(28).Concat(new uint[]
        {
            0x20004, 0, 40, 19, 1, 0, 8, 11, 30, 1, 1, 1, 60, 1,
        }).ToArray();
        words[0] = (uint)words.Length * 4 - 8;
        var formats = PipeWireFormats.Parse(words.SelectMany(BitConverter.GetBytes).ToArray());
        Assert.That(formats.Any(format => format.Characteristics.FramesPerSecond.Numerator == 30), Is.True);
        Assert.That(formats.All(format =>
        {
            var rate = format.Characteristics.FramesPerSecond;
            return rate.Numerator >= rate.Denominator && rate.Numerator <= rate.Denominator * 60;
        }), Is.True);
        Assert.That(formats.Select(format => format.Characteristics).Distinct().Count(), Is.EqualTo(formats.Length));
    }

    [Test]
    public void TruncatedFormatParametersAreRejected()
    {
        var pod = NativeFormat(23).SelectMany(BitConverter.GetBytes).Take(100).ToArray();
        Assert.Throws<InvalidDataException>(() => PipeWireFormats.Parse(pod));
    }
}
