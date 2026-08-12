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
public sealed class MediaFoundationFrameTests
{
    [TestCase(PixelFormats.RGB24, 3, 2, 24, 9, 2, 12, 12, true)]
    [TestCase(PixelFormats.RGB32, 2, 2, 16, 8, 2, 8, 8, true)]
    [TestCase(PixelFormats.ARGB32, 2, 2, 16, 8, 2, 8, 8, true)]
    [TestCase(PixelFormats.RGB15, 3, 2, 12, 6, 2, 6, 8, true)]
    [TestCase(PixelFormats.RGB16, 3, 2, 12, 6, 2, 6, 8, true)]
    [TestCase(PixelFormats.UYVY, 3, 2, 12, 6, 2, 6, 6, false)]
    [TestCase(PixelFormats.YUYV, 3, 2, 12, 6, 2, 6, 6, false)]
    [TestCase(PixelFormats.NV12, 4, 3, 20, 4, 5, 4, 4, false)]
    public void GetFrameLayoutCalculatesFormatSpecificLayout(
        PixelFormats format,
        int width,
        int height,
        int bufferLength,
        int expectedRowLength,
        int expectedRows,
        int expectedSourceStride,
        int expectedTargetStride,
        bool expectedBottomUp)
    {
        var layout = MediaFoundationInterop.GetFrameLayout(
            format,
            width,
            height,
            null,
            bufferLength);

        Assert.That(layout.RowLength, Is.EqualTo(expectedRowLength));
        Assert.That(layout.Rows, Is.EqualTo(expectedRows));
        Assert.That(layout.SourceStride, Is.EqualTo(expectedSourceStride));
        Assert.That(layout.TargetStride, Is.EqualTo(expectedTargetStride));
        Assert.That(layout.BottomUp, Is.EqualTo(expectedBottomUp));
        Assert.That(layout.TargetLength, Is.EqualTo(checked(expectedTargetStride * expectedRows)));
    }

    [TestCase(12, false)]
    [TestCase(-12, true)]
    public void GetFrameLayoutUsesExplicitStrideDirection(int stride, bool expectedBottomUp)
    {
        var layout = MediaFoundationInterop.GetFrameLayout(
            PixelFormats.RGB24,
            3,
            2,
            stride,
            24);

        Assert.That(layout.SourceStride, Is.EqualTo(12));
        Assert.That(layout.BottomUp, Is.EqualTo(expectedBottomUp));
    }

    [Test]
    public void GetFrameLayoutRejectsUnsupportedFormat()
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
            MediaFoundationInterop.GetFrameLayout(PixelFormats.JPEG, 2, 2, null, 4));
    }

    [Test]
    public void GetFrameLayoutRejectsTruncatedBuffer()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            MediaFoundationInterop.GetFrameLayout(PixelFormats.RGB24, 2, 2, 8, 15));

        Assert.That(exception!.ParamName, Is.EqualTo("bufferLength"));
    }

    [Test]
    public void GetFrameLayoutChecksRowLengthOverflow()
    {
        _ = Assert.Throws<OverflowException>(() =>
            MediaFoundationInterop.GetFrameLayout(
                PixelFormats.RGB32,
                int.MaxValue,
                1,
                null,
                int.MaxValue));
    }

    [Test]
    public void RepackFrameCopiesRowsAndClearsPadding()
    {
        var source = new byte[] { 1, 2, 3, 99, 4, 5, 6, 99 };
        var target = new byte[] { 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc };
        var layout = new MediaFoundationInterop.FrameLayout(3, 2, 4, 4, false);

        MediaFoundationInterop.RepackFrame(source, target, layout, false);

        Assert.That(
            target,
            Is.EqualTo(new byte[] { 1, 2, 3, 0, 4, 5, 6, 0, 0xcc }));
    }

    [Test]
    public void RepackFrameReversesRowsWhenRequested()
    {
        var source = new byte[] { 1, 2, 3, 99, 4, 5, 6, 99 };
        var target = new byte[8];
        var layout = new MediaFoundationInterop.FrameLayout(3, 2, 4, 4, false);

        MediaFoundationInterop.RepackFrame(source, target, layout, true);

        Assert.That(
            target,
            Is.EqualTo(new byte[] { 4, 5, 6, 0, 1, 2, 3, 0 }));
    }

    [Test]
    public void RepackFrameRejectsTruncatedSource()
    {
        var layout = new MediaFoundationInterop.FrameLayout(3, 2, 4, 4, false);

        _ = Assert.Throws<ArgumentException>(() =>
            MediaFoundationInterop.RepackFrame(new byte[7], new byte[8], layout, false));
    }

    [Test]
    public void RepackFrameRejectsTruncatedTarget()
    {
        var layout = new MediaFoundationInterop.FrameLayout(3, 2, 4, 4, false);

        _ = Assert.Throws<ArgumentException>(() =>
            MediaFoundationInterop.RepackFrame(new byte[8], new byte[7], layout, false));
    }
}

#pragma warning restore CA1416
