////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal.MediaFoundation;
using NUnit.Framework;
using System;

#pragma warning disable CA1416 // These pure helpers do not invoke Windows APIs.

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed class MediaFoundationPixelFormatTests
{
    [TestCase("00000014-0000-0010-8000-00aa00389b71", PixelFormats.RGB24, "RGB24")]
    [TestCase("00000016-0000-0010-8000-00aa00389b71", PixelFormats.RGB32, "RGB32")]
    [TestCase("00000015-0000-0010-8000-00aa00389b71", PixelFormats.ARGB32, "ARGB32")]
    [TestCase("00000018-0000-0010-8000-00aa00389b71", PixelFormats.RGB15, "RGB555")]
    [TestCase("00000017-0000-0010-8000-00aa00389b71", PixelFormats.RGB16, "RGB565")]
    [TestCase("47504a4d-0000-0010-8000-00aa00389b71", PixelFormats.JPEG, "MJPG")]
    [TestCase("59565955-0000-0010-8000-00aa00389b71", PixelFormats.UYVY, "UYVY")]
    [TestCase("32595559-0000-0010-8000-00aa00389b71", PixelFormats.YUYV, "YUY2")]
    [TestCase("3231564e-0000-0010-8000-00aa00389b71", PixelFormats.NV12, "NV12")]
    public void TryMapPixelFormatMapsKnownMediaFoundationSubtype(
        string subtype,
        PixelFormats expectedFormat,
        string expectedName)
    {
        var mapped = MediaFoundationInterop.TryMapPixelFormat(
            Guid.Parse(subtype),
            out var format,
            out var name);

        Assert.That(mapped, Is.True);
        Assert.That(format, Is.EqualTo(expectedFormat));
        Assert.That(name, Is.EqualTo(expectedName));
    }

    [Test]
    public void TryMapPixelFormatRejectsUnknownSubtype()
    {
        var subtype = new Guid("11223344-5566-7788-99aa-bbccddeeff00");

        var mapped = MediaFoundationInterop.TryMapPixelFormat(
            subtype,
            out var format,
            out var name);

        Assert.That(mapped, Is.False);
        Assert.That(format, Is.EqualTo(PixelFormats.Unknown));
        Assert.That(name, Is.EqualTo(subtype.ToString("D")));
    }
}

#pragma warning restore CA1416
