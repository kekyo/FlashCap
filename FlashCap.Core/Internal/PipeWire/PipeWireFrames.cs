////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

#if !(NET35 || NET40 || NET45 || NETSTANDARD1_3)
using System;
using System.Runtime.InteropServices;

namespace FlashCap.Internal.PipeWire;

internal readonly struct PipeWirePlane
{
    internal readonly IntPtr Pointer;
    internal readonly int Length;
    internal readonly int Stride;

    internal PipeWirePlane(IntPtr pointer, int length, int stride)
    {
        this.Pointer = pointer;
        this.Length = length;
        this.Stride = stride;
    }
}

internal readonly struct PipeWireFrame
{
    // A null pointer means that the caller must pin its reusable repacking buffer.
    internal readonly IntPtr Pointer;
    internal readonly int Length;

    internal PipeWireFrame(IntPtr pointer, int length)
    {
        this.Pointer = pointer;
        this.Length = length;
    }
}

internal static class PipeWireFrames
{
    internal static PipeWireFrame Normalize(
        PixelFormats format, int width, int height, PipeWirePlane[] planes,
        bool rgbBottomUp, ref byte[]? scratch)
    {
        if (width <= 0 || height <= 0 || planes.Length == 0)
            throw new ArgumentException("Invalid PipeWire frame dimensions or planes.");
        var first = planes[0];
        if (format == PixelFormats.JPEG)
        {
            if (planes.Length != 1 || first.Pointer == IntPtr.Zero || first.Length <= 0)
                throw new ArgumentException("Invalid PipeWire JPEG frame.");
            return new PipeWireFrame(first.Pointer, first.Length);
        }

        var rgb = format is PixelFormats.RGB24 or PixelFormats.RGB32 or PixelFormats.ARGB32;
        var rowLength = format switch
        {
            PixelFormats.RGB24 => checked(width * 3),
            PixelFormats.RGB32 or PixelFormats.ARGB32 => checked(width * 4),
            PixelFormats.YUYV or PixelFormats.UYVY => checked(width * 2),
            PixelFormats.NV12 when (width & 1) == 0 => width,
            _ => throw new ArgumentException("Unsupported PipeWire frame format or dimensions."),
        };
        var targetStride = rgb ? checked((rowLength + 3) & ~3) : rowLength;
        var chromaRows = format == PixelFormats.NV12 ? checked((height + 1) / 2) : 0;
        var targetLength = checked(targetStride * checked(height + chromaRows));
        if (planes.Length != 1 && !(format == PixelFormats.NV12 && planes.Length == 2))
            throw new ArgumentException("Unexpected PipeWire plane count.");

        ValidatePlane(first, rowLength, height);
        var chroma = default(PipeWirePlane);
        if (chromaRows > 0)
        {
            if (planes.Length == 2)
                chroma = planes[1];
            else
            {
                var offset = checked(first.Stride * height);
                if (offset > first.Length)
                    throw new ArgumentException("Truncated PipeWire NV12 frame.");
                chroma = new PipeWirePlane(IntPtr.Add(first.Pointer, offset), first.Length - offset, first.Stride);
            }
            ValidatePlane(chroma, rowLength, chromaRows);
        }

        // Layout is a property of each received buffer, not only of its negotiated pixel format.
        // DoNotTranscode also uses this check: it preserves pixels but still requires a valid FlashCap layout.
        var contiguous = planes.Length == 1 ? first.Length >= targetLength :
            first.Length >= targetStride * height && chroma.Stride == targetStride &&
            chroma.Length >= targetStride * chromaRows &&
            chroma.Pointer == IntPtr.Add(first.Pointer, targetStride * height);
        if (contiguous && first.Stride == targetStride && (!rgb || rgbBottomUp || height == 1))
            return new PipeWireFrame(first.Pointer, targetLength);

        if (scratch is null || scratch.Length < targetLength)
            scratch = new byte[targetLength];
        CopyRows(first, height, rowLength, scratch, 0, targetStride, rgb && !rgbBottomUp);
        if (chromaRows > 0)
            CopyRows(chroma, chromaRows, rowLength, scratch, checked(targetStride * height), targetStride, false);
        return new PipeWireFrame(IntPtr.Zero, targetLength);
    }

    private static void ValidatePlane(PipeWirePlane plane, int rowLength, int rows)
    {
        if (plane.Pointer == IntPtr.Zero || plane.Stride < rowLength ||
            plane.Length < (long)(rows - 1) * plane.Stride + rowLength)
            throw new ArgumentException("Invalid or truncated PipeWire frame plane.");
    }

    private static void CopyRows(PipeWirePlane plane, int rows, int rowLength,
        byte[] target, int offset, int targetStride, bool reverse)
    {
        for (var row = 0; row < rows; row++)
        {
            var sourceRow = reverse ? rows - row - 1 : row;
            var targetOffset = checked(offset + row * targetStride);
            Marshal.Copy(IntPtr.Add(plane.Pointer, checked(sourceRow * plane.Stride)), target, targetOffset, rowLength);
            Array.Clear(target, targetOffset + rowLength, targetStride - rowLength);
        }
    }
}
#endif
