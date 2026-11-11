////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

#if !(NET35 || NET40 || NET45 || NETSTANDARD1_3)
using FlashCap.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FlashCap.Internal.PipeWire;

internal sealed class PipeWireFormat
{
    internal readonly uint MediaSubtype;
    internal readonly uint SpaFormat;
    internal readonly VideoCharacteristics Characteristics;

    internal PipeWireFormat(uint subtype, uint spaFormat, VideoCharacteristics characteristics)
    {
        this.MediaSubtype = subtype;
        this.SpaFormat = spaFormat;
        this.Characteristics = characteristics;
    }
}

// SPA PODs are public, native-endian, eight-byte-aligned value objects.
// See spa/pod/pod.h, spa/param/format.h and https://docs.pipewire.org/page_spa_pod.html.
internal static class PipeWireFormats
{
    internal const uint EnumFormat = 3;
    internal const uint Format = 4;
    private const uint MediaType = 1, MediaSubtype = 2;
    private const uint VideoFormat = 0x20001, VideoSize = 0x20003, VideoRate = 0x20004;

    internal static byte[] ReadPod(IntPtr pointer)
    {
        var length = Marshal.ReadInt32(pointer);
        if (length < 0 || length > 16 * 1024 * 1024)
            throw new InvalidDataException("Invalid PipeWire format parameter size.");
        var bytes = new byte[checked(length + 8)];
        Marshal.Copy(pointer, bytes, 0, bytes.Length);
        return bytes;
    }

    internal static PipeWireFormat[] Parse(byte[] pod)
    {
        var properties = ReadProperties(pod, out var hasDmaBufModifier);
        // SPA_FORMAT_VIDEO_modifier is only valid for DMA-BUF. Even modifier zero
        // belongs to that memory contract, which this CPU-buffer backend does not offer.
        if (hasDmaBufModifier || !properties.TryGetValue(MediaType, out var media) || media.First != 2 ||
            !properties.TryGetValue(MediaSubtype, out var subtype) ||
            !properties.TryGetValue(VideoSize, out var sizes) || sizes.ElementType != 10 ||
            !properties.TryGetValue(VideoRate, out var rates) || rates.ElementType != 11)
            return ArrayEx.Empty<PipeWireFormat>();
        uint[] formats;
        if (subtype.First == 0x20002) formats = new[] { 0u }; // SPA_MEDIA_SUBTYPE_mjpg
        else if (subtype.First == 1 && properties.TryGetValue(VideoFormat, out var raw) && raw.ElementType == 3)
            formats = raw.Values.Select(value => value[0]).Distinct().ToArray();
        else return ArrayEx.Empty<PipeWireFormat>();

        var result = new List<PipeWireFormat>();
        foreach (var native in formats)
        {
            var pixelFormat = subtype.First == 0x20002 ? PixelFormats.JPEG : native switch
            {
                4 => PixelFormats.YUYV,
                5 => PixelFormats.UYVY,
                8 => PixelFormats.RGB32,  // BGRx in memory
                12 => PixelFormats.ARGB32, // BGRA in memory
                16 => PixelFormats.RGB24, // BGR in memory
                23 => PixelFormats.NV12,
                _ => PixelFormats.Unknown,
            };
            if (pixelFormat == PixelFormats.Unknown) continue;
            foreach (var size in Expand(sizes, NativeMethods.DefactoStandardResolutions
                .Select(value => new[] { (uint)value.Width, (uint)value.Height }), false))
            foreach (var rate in Expand(rates, NativeMethods.DefactoStandardFramesPerSecond
                .Select(value => new[] { (uint)value.Numerator, (uint)value.Denominator }), true))
            {
                if (size[0] == 0 || size[1] == 0 || size[0] > int.MaxValue || size[1] > int.MaxValue ||
                    rate[0] == 0 || rate[1] == 0 || rate[0] > int.MaxValue || rate[1] > int.MaxValue ||
                    (pixelFormat == PixelFormats.NV12 && (size[0] & 1) != 0))
                    continue;
                var name = pixelFormat == PixelFormats.JPEG ? "MJPEG" : "SPA:" + native;
                result.Add(new PipeWireFormat(subtype.First, native, new VideoCharacteristics(pixelFormat,
                    (int)size[0], (int)size[1], new Fraction((int)rate[0], (int)rate[1]), name,
                    sizes.Kind is 0 or 3 && rates.Kind is 0 or 3, name)));
            }
        }
        return result.GroupBy(value => value.Characteristics).Select(group => group.First()).ToArray();
    }

    private static IEnumerable<uint[]> Expand(Choice choice, IEnumerable<uint[]> standards, bool fraction)
    {
        if (choice.Kind is 0 or 3) return choice.Values;
        if (choice.Kind is not (1 or 2) || choice.Values.Length < 3)
            return ArrayEx.Empty<uint[]>();
        var min = choice.Values[1];
        var max = choice.Values[2];
        var step = choice.Kind == 2 && choice.Values.Length >= 4 ? choice.Values[3] : null;
        return choice.Values.Take(3).Concat(standards).Where(value =>
        {
            if (fraction)
            {
                if (value[1] == 0 || min[1] == 0 || max[1] == 0) return false;
                return (ulong)value[0] * min[1] >= (ulong)min[0] * value[1] &&
                    (ulong)value[0] * max[1] <= (ulong)max[0] * value[1] &&
                    (step is null || FractionStepMatches(value, min, step));
            }
            return value[0] >= min[0] && value[0] <= max[0] &&
                value[1] >= min[1] && value[1] <= max[1] &&
                (step is null || (step[0] > 0 && step[1] > 0 &&
                    (value[0] - min[0]) % step[0] == 0 && (value[1] - min[1]) % step[1] == 0));
        });
    }

    private static bool FractionStepMatches(uint[] value, uint[] min, uint[] step)
    {
        if (step[0] == 0 || step[1] == 0) return false;
        var difference = (decimal)value[0] * min[1] - (decimal)min[0] * value[1];
        var numerator = difference * step[1];
        var denominator = (decimal)value[1] * min[1] * step[0];
        return numerator % denominator == 0;
    }

    private sealed class Choice
    {
        internal readonly uint Kind;
        internal readonly uint ElementType;
        internal readonly uint[][] Values;
        internal uint First => this.Values[0][0];
        internal Choice(uint kind, uint elementType, uint[][] values)
        { this.Kind = kind; this.ElementType = elementType; this.Values = values; }
    }

    private static Dictionary<uint, Choice> ReadProperties(byte[] pod, out bool hasDmaBufModifier)
    {
        hasDmaBufModifier = false;
        uint Read(int offset) => BitConverter.ToUInt32(pod, offset);
        if (pod.Length < 16 || Read(0) != pod.Length - 8 || Read(4) != 15 || Read(8) != 0x40003)
            throw new InvalidDataException("Invalid PipeWire format object.");
        var result = new Dictionary<uint, Choice>();
        for (var offset = 16; offset < pod.Length;)
        {
            if (pod.Length - offset < 16) throw new InvalidDataException("Truncated PipeWire property.");
            var key = Read(offset);
            hasDmaBufModifier |= key == 0x20002;
            var size = Read(offset + 8);
            var type = Read(offset + 12);
            if (size > pod.Length - offset - 16) throw new InvalidDataException("Truncated PipeWire value.");
            var start = offset + 16;
            var end = checked(start + (int)size);
            var kind = 0u;
            var elementSize = size;
            if (type == 19) // SPA_TYPE_Choice
            {
                if (size < 16) throw new InvalidDataException("Truncated PipeWire choice.");
                kind = Read(start);
                elementSize = Read(start + 8);
                type = Read(start + 12);
                start += 16;
            }
            if ((type is 3 or 4 && elementSize == 4) || (type is 10 or 11 && elementSize == 8))
            {
                if (start == end || (end - start) % elementSize != 0)
                    throw new InvalidDataException("Invalid PipeWire choice values.");
                var values = new List<uint[]>();
                for (var valueOffset = start; valueOffset < end; valueOffset += (int)elementSize)
                    values.Add(elementSize == 4 ? new[] { Read(valueOffset) } :
                        new[] { Read(valueOffset), Read(valueOffset + 4) });
                result[key] = new Choice(kind, type, values.ToArray());
            }
            offset = checked((end + 7) & ~7);
            if (offset > pod.Length) throw new InvalidDataException("Invalid PipeWire property alignment.");
        }
        return result;
    }

    private readonly struct Property
    {
        internal readonly uint Key, Type;
        internal readonly uint[] Values;
        internal Property(uint key, uint type, params uint[] values)
        { this.Key = key; this.Type = type; this.Values = values; }
    }

    private static byte[] BuildObject(uint objectType, uint id, params Property[] properties)
    {
        var data = new List<byte>();
        void Add(uint value) => data.AddRange(BitConverter.GetBytes(value));
        Add(0); Add(15); Add(objectType); Add(id);
        foreach (var property in properties)
        {
            Add(property.Key); Add(0);
            Add((uint)property.Values.Length * 4); Add(property.Type);
            foreach (var value in property.Values) Add(value);
            while ((data.Count & 7) != 0) data.Add(0);
        }
        var bytes = data.ToArray();
        Array.Copy(BitConverter.GetBytes((uint)bytes.Length - 8), bytes, 4);
        return bytes;
    }

    internal static byte[] CreateFormat(PipeWireFormat format)
    {
        var characteristics = format.Characteristics;
        var properties = new List<Property>
        {
            new(MediaType, 3, 2),
            new(MediaSubtype, 3, format.MediaSubtype),
            new(VideoSize, 10, (uint)characteristics.Width, (uint)characteristics.Height),
            new(VideoRate, 11, (uint)characteristics.FramesPerSecond.Numerator, (uint)characteristics.FramesPerSecond.Denominator),
        };
        if (format.MediaSubtype == 1) properties.Add(new Property(VideoFormat, 3, format.SpaFormat));
        return BuildObject(0x40003, EnumFormat, properties.ToArray());
    }

    internal static byte[] CreateBufferParameters() =>
        // Request CPU-readable buffers; no DMA-BUF modifier or GPU import is advertised.
        BuildObject(0x40004, 5, new Property(6, 4, (1u << 1) | (1u << 2)));
}
#endif
