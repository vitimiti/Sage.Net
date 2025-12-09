// -----------------------------------------------------------------------
// <copyright file="BaseTypesMath.cs" company="Sage.Net">
// A transliteration and update of the CnC Generals (Zero Hour) engine and games with mod-first support.
// Copyright (C) 2025 Sage.Net Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see https://www.gnu.org/licenses/.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sage.Net.BaseTypes;

/// <summary>A class that provides math operations.</summary>
public static class BaseTypesMath
{
    /// <summary>An approximation of the number pi.</summary>
    public const float Pi = 3.14159265359F;

    /// <summary>An approximation of the number tau.</summary>
    public const float TwoPi = 6.28318530718F;

    /// <summary>Perform a square operation on any number.</summary>
    /// <param name="x">The number to perform the square on.</param>
    /// <typeparam name="TNumber">One of the <see cref="INumber{TSelf}"/> numbers.</typeparam>
    /// <returns>The value of <paramref name="x"/> multiplied by itself once.</returns>
    public static TNumber Sqr<TNumber>(TNumber x)
        where TNumber : INumber<TNumber> => x * x;

    /// <summary>Performs a clamp operation on any number.</summary>
    /// <param name="lo">The low value of the number.</param>
    /// <param name="value">The current value of the number.</param>
    /// <param name="hi">The high value of the number.</param>
    /// <typeparam name="TNumber">One of the <see cref="INumber{TSelf}"/> numbers.</typeparam>
    /// <returns>The value of <paramref name="value"/> clamped between <paramref name="lo"/> and <paramref name="hi"/>.</returns>
    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Improve readability.")]
    public static TNumber Clamp<TNumber>(TNumber lo, TNumber value, TNumber hi)
        where TNumber : INumber<TNumber>
    {
        if (value < lo)
        {
            return lo;
        }

        if (value > hi)
        {
            return hi;
        }

        return value;
    }

    /// <summary>Gets the sign of any number.</summary>
    /// <param name="x">The number to get the sign from.</param>
    /// <typeparam name="TNumber">One of the <see cref="INumber{TSelf}"/> numbers.</typeparam>
    /// <returns><c>1</c> if the value <paramref name="x"/> is positive, <c>-1</c> if the value of <paramref name="x"/> is negative and <c>0</c> if the value of <paramref name="x"/> is <see cref="INumberBase{TNumber}.Zero"/>.</returns>
    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Improve readability.")]
    public static int Sign<TNumber>(TNumber x)
        where TNumber : INumber<TNumber>
    {
        if (x > TNumber.Zero)
        {
            return 1;
        }

        if (x < TNumber.Zero)
        {
            return -1;
        }

        return 0;
    }

    /// <summary>Converts radians to degrees.</summary>
    /// <param name="rad">A <see cref="float"/> with the radians to convert.</param>
    /// <returns>A new <see cref="float"/> with the <paramref name="rad"/> converted to degrees.</returns>
    public static float Rad2Deg(float rad) => rad * (180F / Pi);

    /// <summary>Converts degrees to radians.</summary>
    /// <param name="deg">A <see cref="float"/> with the degrees to convert.</param>
    /// <returns>A new <see cref="float"/> with the <paramref name="deg"/> converted to radians.</returns>
    public static float Deg2Rad(float deg) => deg * (Pi / 180F);

    /// <summary>Performs a fast, rounded conversion of a <see cref="float"/> to a <see cref="long"/>.</summary>
    /// <param name="f">The <see cref="float"/> to round and convert.</param>
    /// <returns>A new <see cref="long"/> from the rounded <paramref name="f"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long FastFloat2LongRound(float f) => (long)(f + (f >= 0 ? 0.5F : -0.5F));

#if RTS_ZERO_HOUR
    /// <summary>Performs the fast truncation of a <see cref="float"/> value.</summary>
    /// <param name="f">The <see cref="float"/> to truncate.</param>
    /// <returns>A new <see cref="float"/> resulting from the truncation of the <paramref name="f"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastFloatTrunc(float f)
    {
        var bits = BitConverter.SingleToUInt32Bits(f);
        var exp = (byte)((bits >> 23) & 0x00FF);
        unchecked
        {
            var mask = (int)(exp < 127 ? 0 : 0xFF80_0000);
            exp -= 127;
            mask >>= exp & 31;
            bits &= (uint)mask;
            return BitConverter.UInt32BitsToSingle(bits);
        }
    }

    /// <summary>Performs the fast flooring of a <see cref="float"/> value.</summary>
    /// <param name="f">The <see cref="float"/> to floor.</param>
    /// <returns>A new <see cref="float"/> resulting from the flooring of the <paramref name="f"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastFloatFloor(float f)
    {
        const uint almost1 = (126 << 23) | 0x007F_FFFF;
        if ((BitConverter.SingleToUInt32Bits(f) & 0x8000_0000) != 0)
        {
            f -= BitConverter.UInt32BitsToSingle(almost1);
        }

        return FastFloatTrunc(f);
    }

    /// <summary>Performs the fast ceiling of a <see cref="float"/> value.</summary>
    /// <param name="f">The <see cref="float"/> to ceiling.</param>
    /// <returns>A new <see cref="float"/> resulting from the ceiling of the <paramref name="f"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastFloatCeil(float f)
    {
        const uint almost1 = (126 << 23) | 0x007F_FFFF;
        if ((BitConverter.SingleToUInt32Bits(f) & 0x8000_0000) != 0)
        {
            f += BitConverter.UInt32BitsToSingle(almost1);
        }

        return FastFloatTrunc(f);
    }
#endif

    /// <summary>Converts a <see cref="float"/> to a 32-bit signed integer using an unchecked cast (truncates toward zero).</summary>
    /// <param name="f">The <see cref="float"/> value to convert.</param>
    /// <returns>The 32-bit signed integer produced by unchecked casting of <paramref name="f"/>.</returns>
    public static int SingleToInt32(float f) => unchecked((int)f);

    /// <summary>Converts a <see cref="float"/> to a 32-bit unsigned integer using an unchecked cast (truncates toward zero).</summary>
    /// <param name="f">The <see cref="float"/> value to convert.</param>
    /// <returns>The 32-bit unsigned integer produced by unchecked casting of <paramref name="f"/>.</returns>
    public static uint SingleToUInt32(float f) => unchecked((uint)f);

    /// <summary>Converts a <see cref="float"/> to a 16-bit signed integer using an unchecked cast (truncates toward zero).</summary>
    /// <param name="f">The <see cref="float"/> value to convert.</param>
    /// <returns>The 16-bit signed integer produced by unchecked casting of <paramref name="f"/>.</returns>
    public static short SingleToInt16(float f) => unchecked((short)f);

    /// <summary>Converts a <see cref="float"/> to an 8-bit signed integer using an unchecked cast (truncates toward zero).</summary>
    /// <param name="f">The <see cref="float"/> value to convert.</param>
    /// <returns>The 8-bit signed integer produced by unchecked casting of <paramref name="f"/>.</returns>
    public static sbyte SingleToSByte(float f) => unchecked((sbyte)f);

    /// <summary>Converts a <see cref="float"/> to an 8-bit unsigned integer using an unchecked cast (truncates toward zero).</summary>
    /// <param name="f">The <see cref="float"/> value to convert.</param>
    /// <returns>The 8-bit unsigned integer produced by unchecked casting of <paramref name="f"/>.</returns>
    public static byte SingleToByte(float f) => unchecked((byte)f);

    /// <summary>Converts a <see cref="float"/> to a <see cref="char"/> using an unchecked cast.</summary>
    /// <param name="f">The <see cref="float"/> value to convert.</param>
    /// <returns>The <see cref="char"/> produced by unchecked casting of <paramref name="f"/>.</returns>
    public static char SingleToChar(float f) => unchecked((char)f);

    /// <summary>Converts a <see cref="double"/> to a <see cref="float"/> using a narrowing cast.</summary>
    /// <param name="d">The <see cref="double"/> value to convert.</param>
    /// <returns>The <see cref="float"/> result of casting <paramref name="d"/>.</returns>
    public static float DoubleToSingle(double d) => (float)d;

    /// <summary>Converts a 32-bit signed integer to a <see cref="float"/>.</summary>
    /// <param name="i">The 32-bit signed integer to convert.</param>
    /// <returns>The <see cref="float"/> representation of <paramref name="i"/>.</returns>
    public static float Int32ToSingle(int i) => i;

#if RTS_ZERO_HOUR
    /// <summary>Converts a <see cref="float"/> to a 64-bit signed integer by rounding toward positive infinity.</summary>
    /// <param name="f">The <see cref="float"/> value to convert.</param>
    /// <returns>The ceiling of <paramref name="f"/> as a 64-bit signed integer.</returns>
    public static long SingleToInt64Ceil(float f) => FastFloat2LongRound(FastFloatFloor(f));
#else
    /// <summary>Converts a <see cref="float"/> to a 64-bit signed integer by rounding toward positive infinity.</summary>
    /// <param name="f">The <see cref="float"/> value to convert.</param>
    /// <returns>The ceiling of <paramref name="f"/> as a 64-bit signed integer.</returns>
    public static long SingleToInt64Ceil(float f) => FastFloat2LongRound(float.Ceiling(f));
#endif

#if RTS_ZERO_HOUR
    /// <summary>Converts a <see cref="float"/> to a 64-bit signed integer by rounding toward negative infinity.</summary>
    /// <param name="f">The <see cref="float"/> value to convert.</param>
    /// <returns>The floor of <paramref name="f"/> as a 64-bit signed integer.</returns>
    public static long SingleToInt64Floor(float f) => FastFloat2LongRound(FastFloatCeil(f));
#else
    /// <summary>Converts a <see cref="float"/> to a 64-bit signed integer by rounding toward negative infinity.</summary>
    /// <param name="f">The <see cref="float"/> value to convert.</param>
    /// <returns>The floor of <paramref name="f"/> as a 64-bit signed integer.</returns>
    public static long SingleToInt64Floor(float f) => FastFloat2LongRound(float.Floor(f));
#endif
#if RTS_ZERO_HOUR
    /// <summary>Performs a fast truncation on a <see cref="float"/> value.</summary>
    /// <param name="f">The <see cref="float"/> to truncate.</param>
    /// <returns>The truncated value of <paramref name="f"/>.</returns>
    public static FastSingleTrunc(float f) => FastFloatTrunc(f);

    /// <summary>Performs a fast ceiling operation on a <see cref="float"/> value.</summary>
    /// <param name="f">The <see cref="float"/> to ceil.</param>
    /// <returns>The result of applying a fast ceiling to <paramref name="f"/>.</returns>
    public static FastSingleCeil(float f) => FastFloatCeil(f);

    /// <summary>Performs a fast floor operation on a <see cref="float"/> value.</summary>
    /// <param name="f">The <see cref="float"/> to floor.</param>
    /// <returns>The result of applying a fast floor to <paramref name="f"/>.</returns>
    public static FastSingleFloor(float f) => FastFloatFloor(f);
#endif
}
