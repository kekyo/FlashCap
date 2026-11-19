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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static FlashCap.Internal.NativeMethods_V4L2;

namespace FlashCap.Internal.PipeWire;

// SPA PropInfo/Props use the public POD ABI: https://docs.pipewire.org/page_spa_pod.html.
// The V4L2 provider publishes otherwise unmapped controls as SPA_PROP_START_CUSTOM + CID.
// Reuse the V4L2 control semantics (including coupled exposure/iris modes) without any ioctl.
internal sealed class PipeWireControls
{
    internal const uint PropInfo = 1, Props = 2;
    private const uint Custom = 0x1000000;
    private readonly bool v4l2;
    private readonly Dictionary<uint, Property> properties = new();
    private readonly Dictionary<uint, Value> values = new();

    private sealed class Value
    {
        internal uint Type, Kind, Flags;
        internal double[] Numbers = ArrayEx.Empty<double>();
        internal int Scale => this.Type == 6 ? 1000 : 1;
    }

    private sealed class Property
    {
        internal uint Id;
        internal Value Value = new();
        internal v4l2_queryctrl Range = new();
        internal int[]? Menu;
        internal bool Accepts(int value) => value >= this.Range.minimum && value <= this.Range.maximum &&
            ((long)value - this.Range.minimum) % this.Range.step == 0 &&
            (this.Menu is null || Array.IndexOf(this.Menu, value) >= 0);
    }

    internal PipeWireControls(bool v4l2) => this.v4l2 = v4l2;

    private static Dictionary<uint, Value> ReadObject(byte[] pod, uint objectType, out int labelsOffset)
    {
        uint Read(int offset) => BitConverter.ToUInt32(pod, offset);
        if (pod.Length < 16 || Read(0) != pod.Length - 8 || Read(4) != 15 || Read(8) != objectType)
            throw new InvalidDataException("Invalid PipeWire control object.");
        var result = new Dictionary<uint, Value>();
        labelsOffset = 0;
        for (var offset = 16; offset < pod.Length;)
        {
            if (pod.Length - offset < 16) throw new InvalidDataException("Truncated PipeWire control property.");
            var key = Read(offset);
            var size = Read(offset + 8);
            var type = Read(offset + 12);
            if (size > pod.Length - offset - 16) throw new InvalidDataException("Truncated PipeWire control value.");
            var start = offset + 16;
            var end = start + (int)size;
            if (objectType == 0x40001 && key == 4 && type == 14) labelsOffset = offset + 8;
            var value = new Value { Type = type, Flags = Read(offset + 4) };
            var elementSize = size;
            if (type == 19)
            {
                if (size < 16) throw new InvalidDataException("Truncated PipeWire control choice.");
                value.Kind = Read(start);
                elementSize = Read(start + 8);
                value.Type = Read(start + 12);
                start += 16;
            }
            if (value.Type is 2 or 3 or 4 or 6 && elementSize == 4)
            {
                if (start == end || (end - start) % 4 != 0)
                    throw new InvalidDataException("Invalid PipeWire control choice values.");
                var numbers = new List<double>();
                for (var position = start; position < end; position += 4)
                    numbers.Add(value.Type == 6 ? (double)BitConverter.ToSingle(pod, position) : BitConverter.ToInt32(pod, position));
                value.Numbers = numbers.ToArray();
                result[key] = value;
            }
            offset = checked((end + 7) & ~7);
            if (offset > pod.Length) throw new InvalidDataException("Invalid PipeWire control alignment.");
        }
        return result;
    }

    internal void AddInfo(byte[] pod)
    {
        var info = ReadObject(pod, 0x40001, out var labelsOffset);
        if (!info.TryGetValue(1, out var id) || id.Type != 3 ||
            !info.TryGetValue(3, out var value) || value.Type is not (2 or 4 or 6) || value.Kind > 3)
            return;
        var numbers = value.Numbers.Select(number => number * value.Scale).ToArray();
        if (numbers.Any(number => double.IsNaN(number) || double.IsInfinity(number) || number < int.MinValue || number > int.MaxValue))
            return;
        var property = new Property { Id = (uint)id.Numbers[0], Value = value };
        if (value.Kind is 1 or 2)
        {
            if (numbers.Length != (value.Kind == 2 ? 4 : 3)) return;
            property.Range.minimum = (int)Math.Ceiling(numbers[1]);
            property.Range.maximum = (int)Math.Floor(numbers[2]);
            property.Range.step = value.Kind == 2 ? (int)Math.Round(numbers[3]) : 1;
        }
        else
        {
            var menu = numbers.Select(number => (int)Math.Round(number)).Distinct().ToList();
            // Older V4L2 providers put menu alternatives in labels, leaving only
            // the default in the enum choice. Labels are alternating value/string PODs.
            if (labelsOffset != 0)
            {
                var end = labelsOffset + 8 + (int)BitConverter.ToUInt32(pod, labelsOffset);
                for (var offset = labelsOffset + 8; offset < end;)
                {
                    if (end - offset < 8) throw new InvalidDataException("Truncated PipeWire control label.");
                    var size = BitConverter.ToUInt32(pod, offset);
                    if (size > end - offset - 8) throw new InvalidDataException("Truncated PipeWire control label value.");
                    if (BitConverter.ToUInt32(pod, offset + 4) == 4 && size == 4)
                        menu.Add(BitConverter.ToInt32(pod, offset + 8));
                    offset = checked((offset + 8 + (int)size + 7) & ~7);
                    if (offset > end) throw new InvalidDataException("Invalid PipeWire control label alignment.");
                }
            }
            property.Menu = menu.Distinct().ToArray();
            property.Range.minimum = menu.Min();
            property.Range.maximum = menu.Max();
            property.Range.step = 1;
        }
        if (property.Range.minimum > property.Range.maximum || property.Range.step <= 0) return;
        var defaultValue = (int)Math.Round(numbers[0]);
        property.Range.default_value = property.Accepts(defaultValue) ? defaultValue : property.Range.minimum;
        this.properties[property.Id] = property;
    }

    internal void SetValues(IEnumerable<byte[]> pods)
    {
        this.values.Clear();
        foreach (var pod in pods)
            foreach (var entry in ReadObject(pod, 0x40002, out _)) this.values[entry.Key] = entry.Value;
    }

    private Property? Find(uint id)
    {
        // Never interpret another provider's private control namespace as V4L2 CIDs.
        if (this.v4l2 && this.properties.TryGetValue(Custom + id, out var custom)) return custom;
        var standard = id switch
        {
            V4L2_CID_BRIGHTNESS => 0x20001u,
            V4L2_CID_CONTRAST => 0x20002u,
            V4L2_CID_SATURATION => 0x20003u,
            V4L2_CID_HUE => 0x20004u,
            V4L2_CID_GAMMA => 0x20005u,
            V4L2_CID_EXPOSURE_ABSOLUTE => 0x20006u,
            V4L2_CID_GAIN => 0x20007u,
            V4L2_CID_SHARPNESS => 0x20008u,
            _ => 0u,
        };
        return this.properties.TryGetValue(standard, out var property) ? property : null;
    }

    private int? GetValue(Property property)
    {
        if (!this.values.TryGetValue(property.Id, out var value) || value.Type != property.Value.Type) return null;
        var number = Math.Round(value.Numbers[0] * value.Scale);
        return double.IsNaN(number) || double.IsInfinity(number) || number < int.MinValue || number > int.MaxValue ?
            null : (int)number;
    }

    private V4L2CameraControls CreateAdapter(Func<uint, int, bool> set) => new(
        id => this.Find(id)?.Range,
        id => this.Find(id) is { } property ? this.GetValue(property) : null,
        set,
        (id, value) => this.Find(id)?.Accepts(value) == true);

    internal CaptureDeviceControl[] GetControls() => this.CreateAdapter((_, _) => false).GetControls();

    internal CaptureDeviceControlValue? GetValue(CaptureDeviceControlTypes type) =>
        this.CreateAdapter((_, _) => false).GetValue(type);

    // Construct ordered writes first. The caller awaits and verifies each write,
    // so a rejected auto/manual switch never proceeds to the manual value.
    internal byte[][]? CreateWrites(CaptureDeviceControlTypes type, int value, bool isAuto)
    {
        var writes = new List<byte[]>();
        var adapter = this.CreateAdapter((id, requested) =>
        {
            if (this.Find(id) is not { } property || !property.Accepts(requested) ||
                !this.values.TryGetValue(property.Id, out var current) || (current.Flags & 1) != 0)
                return false;
            var bytes = property.Value.Type == 6 ? BitConverter.GetBytes(requested / 1000f) : BitConverter.GetBytes(requested);
            var words = new uint[] { 32, 15, 0x40002, Props, property.Id, 0, 4, property.Value.Type };
            writes.Add(words.SelectMany(BitConverter.GetBytes).Concat(bytes).Concat(new byte[4]).ToArray());
            return true;
        });
        return adapter.SetValue(type, value, isAuto) ? writes.ToArray() : null;
    }

    internal bool Matches(byte[] write)
    {
        var desired = ReadObject(write, 0x40002, out _).Single();
        if (!this.values.TryGetValue(desired.Key, out var actual) || actual.Type != desired.Value.Type) return false;
        return Math.Round(actual.Numbers[0] * actual.Scale) == Math.Round(desired.Value.Numbers[0] * desired.Value.Scale);
    }
}
#endif
