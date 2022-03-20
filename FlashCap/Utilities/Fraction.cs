////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace FlashCap.Utilities
{
#if !NETSTANDARD1_3
    [Serializable]
#endif
    public readonly struct Fraction :
        IEquatable<Fraction>, IComparable<Fraction>
    {
        public readonly int Numerator;
        public readonly int Denominator;

        public Fraction(int numerator, int denominator)
        {
            if (denominator == 0 && numerator != 0)
            {
                throw new DivideByZeroException();
            }
            this.Numerator = numerator;
            this.Denominator = denominator;
        }

        public static Fraction Create(int numerator, int denominator) =>
            new(numerator, denominator);
        
        public static Fraction Create(int value) =>
            new(value, 1);
        
        private static long gcm(long a, long b) =>
            b == 0 ?
                0 :
                a % b is { } mod && mod == 0 ?
                    b : gcm(b, mod);

        private static long lcm(long a, long b) =>
            b == 0 ?
                0 :
                a * b / gcm(a, b);

        private static Fraction Reduce(long n, long d)
        {
            var g = gcm(n, d);
            return g switch
            {
                0 => Zero,
                1 => new Fraction((int)n, (int)d),
                _ => new Fraction((int)(n / g), (int)(d / g)),
            };
        }

        public Fraction Reduce() =>
            Reduce(this.Numerator, this.Denominator);

        public Fraction Reciprocal() =>
            new(this.Denominator, this.Numerator);

        public Fraction Add(Fraction rhs, bool reduce = true)
        {
            if (this.Denominator == 0)
            {
                Debug.Assert(this.Numerator == 0);
                return rhs;
            }
            if (rhs.Denominator == 0)
            {
                Debug.Assert(rhs.Numerator == 0);
                return rhs;
            }
            var pd = lcm(this.Denominator, rhs.Denominator);
            var tm = pd / this.Denominator;
            var rm = pd / rhs.Denominator;
            var tn = this.Numerator * tm;
            var rn = rhs.Numerator * rm;
            var pn = tn + rn;
            return reduce ? Reduce(pn, pd) : new Fraction((int)pn, (int)pd);
        }

        public Fraction Mult(Fraction rhs, bool reduce = true)
        {
            var pn = (long)this.Numerator * rhs.Numerator;
            var pd = (long)this.Denominator * rhs.Denominator;
            return reduce ? Reduce(pn, pd) : new Fraction((int)pn, (int)pd);
        }

        public override int GetHashCode() =>
            this.Numerator ^ this.Denominator;

        private static bool ExactEquals(Fraction lhs, Fraction rhs) =>
            lhs.Numerator == rhs.Numerator &&
            lhs.Denominator == rhs.Denominator;

        public bool Equals(Fraction rhs) =>
            ExactEquals(this, rhs) ||
            ExactEquals(this.Reduce(), rhs.Reduce());

        public override bool Equals(object? obj) =>
            obj is Fraction rhs && this.Equals(rhs);

        bool IEquatable<Fraction>.Equals(Fraction rhs) =>
            this.Equals(rhs);

        public int CompareTo(Fraction rhs)
        {
            if (this.Denominator == 0)
            {
                Debug.Assert(this.Numerator == 0);
                return this.Numerator.CompareTo(rhs.Numerator);
            }
            if (rhs.Denominator == 0)
            {
                Debug.Assert(rhs.Numerator == 0);
                return this.Numerator.CompareTo(rhs.Numerator);
            }
            var pd = lcm(this.Denominator, rhs.Denominator);
            var tm = pd / this.Denominator;
            var rm = pd / rhs.Denominator;
            var tn = this.Numerator * tm;
            var rn = rhs.Numerator * rm;
            return tn.CompareTo(rn);
        }

        int IComparable<Fraction>.CompareTo(Fraction rhs) =>
            this.CompareTo(rhs);

        public override string ToString() =>
            this.Denominator == 0 ?
                "0 [0.0]" :
                $"{this.Numerator}/{this.Denominator} [{(double)this.Numerator / this.Denominator:F4}]";

        public static readonly Fraction Zero =
            new(0, 0);

        public static Fraction operator +(Fraction lhs, Fraction rhs) =>
            lhs.Add(rhs);
        public static Fraction operator -(Fraction lhs, Fraction rhs) =>
            lhs.Add(new Fraction(-rhs.Numerator, rhs.Denominator));
        public static Fraction operator *(Fraction lhs, Fraction rhs) =>
            lhs.Mult(rhs);
        public static Fraction operator /(Fraction lhs, Fraction rhs) =>
            lhs.Mult(rhs.Reciprocal());

        public static bool operator ==(Fraction lhs, Fraction rhs) =>
            lhs.Equals(rhs);
        public static bool operator !=(Fraction lhs, Fraction rhs) =>
            !lhs.Equals(rhs);
        public static bool operator <(Fraction lhs, Fraction rhs) =>
            lhs.CompareTo(rhs) < 0;
        public static bool operator <=(Fraction lhs, Fraction rhs) =>
            lhs.CompareTo(rhs) <= 0;
        public static bool operator >(Fraction lhs, Fraction rhs) =>
            lhs.CompareTo(rhs) > 0;
        public static bool operator >=(Fraction lhs, Fraction rhs) =>
            lhs.CompareTo(rhs) >= 0;

        public static implicit operator Fraction(int numerator) =>
            new(numerator, 1);
        public static implicit operator double(Fraction lhs) =>
            lhs.Denominator == 0 ?
                0.0 :
                (double)lhs.Numerator / lhs.Denominator;
    }
}
