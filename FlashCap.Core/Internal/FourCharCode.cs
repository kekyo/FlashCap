////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Yoh Deadfall (@YohDeadfall)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;

namespace FlashCap.Internal;

internal readonly struct FourCharCode : IEquatable<FourCharCode>
{
    private readonly uint value;

    public FourCharCode(char c1, char c2, char c3, char c4)
    {
        value = GetFourCharCode(c1, c2, c3, c4);
    }

    private uint GetFourCharCode(char c1, char c2, char c3, char c4)
    {
        return (uint)((c1 << 24) | (c2 << 16) | (c3 << 8) | c4);
    }
    
    public FourCharCode(string fourcc)
    {
        if (fourcc.Length != 4)
            throw new ArgumentException("FourCC must be exactly 4 characters long.");

        value = GetFourCharCode(fourcc[0], fourcc[1], fourcc[2], fourcc[3]);
    }
    
    public FourCharCode(uint value)
    {
        this.value = value;
    }

    public static implicit operator uint(FourCharCode fourCC) => fourCC.value;
    
    public static implicit operator int(FourCharCode fourCC) => (int)fourCC.value;

    public override string ToString()
    {
        return new string(new[]
        {
            (char)((value >> 24) & 0xFF),
            (char)((value >> 16) & 0xFF),
            (char)((value >> 8) & 0xFF),
            (char)(value & 0xFF)
        });
    }

    public bool Equals(FourCharCode other) => value == other.value;

    public override bool Equals(object? obj) => 
        obj is FourCharCode other && Equals(other);

    public override int GetHashCode() => value.GetHashCode();

    public static bool operator ==(FourCharCode left, FourCharCode right) => left.Equals(right);

    public static bool operator !=(FourCharCode left, FourCharCode right) => !left.Equals(right);
    
    public int GetIntVal() => (int)value;
    

}