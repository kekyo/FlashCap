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

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed class PipeWireFrameTests
{
    [TestCase(PixelFormats.NV12, 4, 2, 4, 12)]
    [TestCase(PixelFormats.YUYV, 4, 2, 8, 16)]
    [TestCase(PixelFormats.UYVY, 4, 2, 8, 16)]
    [TestCase(PixelFormats.RGB24, 3, 2, 12, 24)]
    [TestCase(PixelFormats.RGB32, 2, 2, 8, 16)]
    [TestCase(PixelFormats.ARGB32, 2, 2, 8, 16)]
    [TestCase(PixelFormats.JPEG, 640, 480, 0, 11)]
    public unsafe void MatchingLayoutPreservesTheOriginalPayload(
        PixelFormats format, int width, int height, int stride, int length)
    {
        var source = new byte[length];
        byte[]? scratch = null;
        fixed (byte* data = source)
        {
            var frame = PipeWireFrames.Normalize(format, width, height,
                new[] { new PipeWirePlane((IntPtr)data, length, stride) }, true, ref scratch);
            Assert.That(frame.Pointer, Is.EqualTo((IntPtr)data));
            Assert.That(frame.Length, Is.EqualTo(length));
            Assert.That(scratch, Is.Null);
        }
    }

    [Test]
    public unsafe void PaddedNv12PreservesPixelsAndRemovesRowPadding()
    {
        var source = new byte[] { 1, 2, 3, 4, 99, 99, 5, 6, 7, 8, 99, 99, 9, 10, 11, 12, 99, 99 };
        byte[]? scratch = null;
        fixed (byte* data = source)
        {
            var frame = PipeWireFrames.Normalize(PixelFormats.NV12, 4, 2,
                new[] { new PipeWirePlane((IntPtr)data, source.Length, 6) }, false, ref scratch);
            Assert.That(frame.Pointer, Is.EqualTo(IntPtr.Zero));
            Assert.That(frame.Length, Is.EqualTo(12));
            Assert.That(scratch, Is.EqualTo(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }));
        }
    }

    [Test]
    public unsafe void SeparateNv12PlanesCanHaveDifferentStrides()
    {
        var y = new byte[] { 1, 2, 3, 4, 99, 5, 6, 7, 8 };
        var uv = new byte[] { 9, 10, 11, 12, 99, 99, 99, 99 };
        byte[]? scratch = null;
        fixed (byte* yp = y)
        fixed (byte* uvp = uv)
        {
            var frame = PipeWireFrames.Normalize(PixelFormats.NV12, 4, 2,
                new[] { new PipeWirePlane((IntPtr)yp, y.Length, 5), new PipeWirePlane((IntPtr)uvp, uv.Length, 8) },
                false, ref scratch);
            Assert.That(frame.Length, Is.EqualTo(12));
            Assert.That(scratch, Is.EqualTo(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }));
        }
    }

    [Test]
    public unsafe void TopDownRgbIsRepackedWithDibPadding()
    {
        var source = new byte[] { 1, 2, 3, 99, 99, 4, 5, 6 };
        byte[]? scratch = null;
        fixed (byte* data = source)
        {
            var frame = PipeWireFrames.Normalize(PixelFormats.RGB24, 1, 2,
                new[] { new PipeWirePlane((IntPtr)data, source.Length, 5) }, false, ref scratch);
            Assert.That(frame.Length, Is.EqualTo(8));
            Assert.That(scratch, Is.EqualTo(new byte[] { 4, 5, 6, 0, 1, 2, 3, 0 }));
        }
    }

    [Test]
    public unsafe void LayoutIsReevaluatedForEveryFrame()
    {
        var source = new byte[] { 1, 2, 3, 4, 99, 99, 5, 6, 7, 8, 99, 99 };
        byte[]? scratch = null;
        fixed (byte* data = source)
        {
            _ = PipeWireFrames.Normalize(PixelFormats.YUYV, 2, 2,
                new[] { new PipeWirePlane((IntPtr)data, source.Length, 6) }, false, ref scratch);
            var previousScratch = scratch;
            var frame = PipeWireFrames.Normalize(PixelFormats.YUYV, 2, 2,
                new[] { new PipeWirePlane((IntPtr)data, 8, 4) }, false, ref scratch);
            Assert.That(frame.Pointer, Is.EqualTo((IntPtr)data));
            Assert.That(scratch, Is.SameAs(previousScratch));
        }
    }

    [TestCase(0, 4)]
    [TestCase(7, 4)]
    [TestCase(16, 3)]
    [TestCase(16, -4)]
    public unsafe void InvalidRowsAreRejectedBeforeReadingMemory(int length, int stride)
    {
        var source = new byte[16];
        byte[]? scratch = null;
        fixed (byte* data = source)
        {
            var plane = new PipeWirePlane((IntPtr)data, length, stride);
            Assert.Throws<ArgumentException>(() => PipeWireFrames.Normalize(
                PixelFormats.YUYV, 2, 2, new[] { plane }, false, ref scratch));
        }
    }
}
