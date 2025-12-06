// -----------------------------------------------------------------------
// <copyright file="VegasMath.cs" company="Sage.Net">
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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Sage.Net.GameEngine.Libraries.Vegas.Math;

/// <summary>
/// A collection of math helpers and fast approximations used by the Vegas engine.
/// </summary>
public static partial class VegasMath
{
    /// <summary>
    /// Small epsilon value used for float comparisons (1e-4).
    /// </summary>
    public const float Epsilon = .0001F;

    /// <summary>
    /// Square of <see cref="Epsilon"/>.
    /// </summary>
    public const float Epsilon2 = Epsilon * Epsilon;

    /// <summary>
    /// Ratio of a circle's circumference to its diameter (π).
    /// </summary>
    public const float Pi = 3.141592654F;

    /// <summary>
    /// Two times <see cref="Pi"/> (τ).
    /// </summary>
    public const float TwoPi = 6.283185308F;

    /// <summary>
    /// Maximum finite <see cref="float"/> value.
    /// </summary>
    public const float FloatMax = float.MaxValue;

    /// <summary>
    /// Minimum (most negative) finite <see cref="float"/> value.
    /// </summary>
    public const float FloatMin = float.MinValue;

    /// <summary>
    /// Square root of 2.
    /// </summary>
    public const float Sqrt2 = 1.414213562F;

    /// <summary>
    /// Square root of 3.
    /// </summary>
    public const float Sqrt3 = 1.732050808F;

    /// <summary>
    /// 1 / sqrt(2).
    /// </summary>
    public const float OneOverSqrt2 = .707106781F;

    /// <summary>
    /// 1 / sqrt(3).
    /// </summary>
    public const float OneOverSqrt3 = .577350269F;

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    /// <param name="rad">Angle in radians.</param>
    /// <returns>Angle in degrees.</returns>
    public static double RadToDeg(double rad) => rad * 180.0 / Pi;

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    /// <param name="rad">Angle in radians.</param>
    /// <returns>Angle in degrees.</returns>
    public static float RadToDeg(float rad) => rad * 180.0F / Pi;

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="deg">Angle in degrees.</param>
    /// <returns>Angle in radians.</returns>
    public static double DegToRad(double deg) => deg * Pi / 180.0;

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="deg">Angle in degrees.</param>
    /// <returns>Angle in radians.</returns>
    public static float DegToRad(float deg) => deg * Pi / 180.0F;

    private const int TableSize = 1024;

    private static readonly float[] FastAcosTable = new float[TableSize];
    private static readonly float[] FastAsinTable = new float[TableSize];
    private static readonly float[] FastSinTable = new float[TableSize];
    private static readonly float[] FastInvSinTable = new float[TableSize];

    static VegasMath()
    {
        for (var i = 0; i < TableSize; i++)
        {
            var cv = (i - (TableSize / 2F)) * (1F / (TableSize / 2F));
            FastAcosTable[i] = float.Acos(cv);
            FastAsinTable[i] = float.Asin(cv);
        }

        for (var i = 0; i < TableSize; i++)
        {
            var cv = i * 2F * Pi / TableSize;
            FastSinTable[i] = float.Sin(cv);

            FastInvSinTable[i] = i > 0 ? 1F / FastSinTable[i] : FloatMax;
        }
    }

    /// <summary>
    /// Returns the absolute value of a single-precision floating-point number using bit operations.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <returns>The absolute value of <paramref name="value"/>.</returns>
    public static float FAbs(float value)
    {
        var bits = BitConverter.SingleToInt32Bits(value);
        bits &= 0x7FFF_FFFF;
        return BitConverter.Int32BitsToSingle(bits);
    }

    /// <summary>
    /// Converts a <see cref="float"/> to a 32-bit integer by truncation toward zero.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The truncated integer.</returns>
    public static int FloatToIntChop(float value)
    {
        unchecked
        {
            var bits = BitConverter.SingleToInt32Bits(value);
            var sign = bits >> 31;
            var mantissa = (bits & ((1L << 32) - 1)) | (1 << 23);
            var exponent = ((bits & 0x7FFF_FFFF) >> 23) - 127;
            var r = ((uint)mantissa << 8) >> (31 - exponent);
            return (int)(((r ^ sign) - sign) & ~(exponent >> 31));
        }
    }

    /// <summary>
    /// Converts a <see cref="float"/> to a 32-bit integer by flooring (rounding toward negative infinity).
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The floored integer.</returns>
    public static int FloatToIntFloor(float value)
    {
        unchecked
        {
            var bits = BitConverter.SingleToInt32Bits(value);
            var sign = bits >> 31;
            bits &= 0x7FFF_FFFF;

            var exponent = (bits >> 23) - 127;
            var exponentSign = ~(exponent >> 31);
            var iMask = (1 << (31 - exponent)) - 1;
            var mantissa = bits & ((1 << 23) - 1);
            var r = ((uint)(mantissa | (1 << 23)) << 8) >> (31 - exponent);
            r = (uint)(
                ((r & exponentSign) ^ sign) + (~((mantissa << 8) & iMask) & (exponentSign ^ ((bits - 1) >> 31)) & sign)
            );

            return (int)r;
        }
    }

    /// <summary>
    /// Returns the cosine of the specified angle.
    /// </summary>
    /// <param name="value">An angle, in radians.</param>
    /// <returns>The cosine of <paramref name="value"/>.</returns>
    public static float Cos(float value) => float.Cos(value);

    /// <summary>
    /// Returns the sine of the specified angle.
    /// </summary>
    /// <param name="value">An angle, in radians.</param>
    /// <returns>The sine of <paramref name="value"/>.</returns>
    public static float Sin(float value) => float.Sin(value);

    /// <summary>
    /// Returns the square root of a specified number.
    /// </summary>
    /// <param name="value">A number.</param>
    /// <returns>The square root of <paramref name="value"/>.</returns>
    public static float Sqrt(float value) => float.Sqrt(value);

    /// <summary>
    /// Returns the reciprocal of the square root of a specified number.
    /// </summary>
    /// <param name="value">A number.</param>
    /// <returns>1 / sqrt(<paramref name="value"/>).</returns>
    public static float InvSqrt(float value) => 1F / float.Sqrt(value);

    /// <summary>
    /// Converts a <see cref="float"/> to a 64-bit integer by truncation toward zero.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The truncated integer as a <see cref="float"/>.</returns>
    public static float FloatToLong(float value) => (long)value;

    /// <summary>
    /// Fast sine approximation using a lookup table and linear interpolation.
    /// </summary>
    /// <param name="value">Angle in radians.</param>
    /// <returns>Approximate sine of <paramref name="value"/>.</returns>
    public static float FastSin(float value)
    {
        value *= FastInvSinTable.Length / (2F * Pi);
        var idx0 = FloatToIntFloor(value);
        var idx1 = idx0 + 1;
        var fraction = value - idx0;

        idx0 &= FastSinTable.Length - 1;
        idx1 &= FastSinTable.Length - 1;

        return ((1F - fraction) * FastSinTable[idx0]) + (fraction * FastSinTable[idx1]);
    }

    /// <summary>
    /// Fast reciprocal sine using <see cref="FastSin(float)"/>.
    /// </summary>
    /// <param name="value">Angle in radians.</param>
    /// <returns>Approximate 1 / sin(<paramref name="value"/>).</returns>
    public static float FastInvSin(float value) => 1F / FastSin(value);

    /// <summary>
    /// Fast cosine approximation using a lookup table and linear interpolation.
    /// </summary>
    /// <param name="value">Angle in radians.</param>
    /// <returns>Approximate cosine of <paramref name="value"/>.</returns>
    public static float FastCos(float value)
    {
        value += Pi * .5F;
        value *= FastSinTable.Length / (2F * Pi);

        var idx0 = FloatToIntFloor(value);
        var idx1 = idx0 + 1;
        var fraction = value - idx0;

        idx0 &= FastSinTable.Length - 1;
        idx1 &= FastSinTable.Length - 1;

        return ((1F - fraction) * FastSinTable[idx0]) + (fraction * FastSinTable[idx1]);
    }

    /// <summary>
    /// Fast reciprocal cosine using <see cref="FastCos(float)"/>.
    /// </summary>
    /// <param name="value">Angle in radians.</param>
    /// <returns>Approximate 1 / cos(<paramref name="value"/>).</returns>
    public static float FastInvCos(float value) => 1F / FastCos(value);

    /// <summary>
    /// Fast arccos approximation using a lookup table and linear interpolation.
    /// </summary>
    /// <param name="value">A value typically in [-1, 1].</param>
    /// <returns>Approximate arccos of <paramref name="value"/> in radians.</returns>
    public static float FastAcos(float value)
    {
        if (FAbs(value) > .975F)
        {
            return Acos(value);
        }

        value *= FastAcosTable.Length / 2F;

        var idx0 = FloatToIntFloor(value);
        var idx1 = idx0 + 1;
        var fraction = value - idx0;

        idx0 += FastAcosTable.Length / 2;
        idx1 += FastAcosTable.Length / 2;

        Debug.Assert(
            idx0 >= 0 && idx0 < FastAcosTable.Length,
            $"The {nameof(idx0)} must be between 0 and {FastAcosTable.Length - 1}, but was {idx0}."
        );

        Debug.Assert(
            idx1 >= 0 && idx1 < FastAcosTable.Length,
            $"The {nameof(idx1)} must be between 0 and {FastAcosTable.Length - 1}, but was {idx1}."
        );

        return ((1F - fraction) * FastAcosTable[idx0]) + (fraction * FastAcosTable[idx1]);
    }

    /// <summary>
    /// Returns the angle whose cosine is the specified number.
    /// </summary>
    /// <param name="value">A value typically in [-1, 1].</param>
    /// <returns>Arccos of <paramref name="value"/> in radians.</returns>
    public static float Acos(float value) => float.Acos(value);

    /// <summary>
    /// Fast arcsin approximation using a lookup table and linear interpolation.
    /// </summary>
    /// <param name="value">A value typically in [-1, 1].</param>
    /// <returns>Approximate arcsin of <paramref name="value"/> in radians.</returns>
    public static float FastAsin(float value)
    {
        if (FAbs(value) > .975F)
        {
            return Asin(value);
        }

        value *= FastAsinTable.Length / 2F;

        var idx0 = FloatToIntFloor(value);
        var idx1 = idx0 + 1;
        var fraction = value - idx0;

        idx0 += FastAsinTable.Length / 2;
        idx1 += FastAsinTable.Length / 2;

        Debug.Assert(
            idx0 >= 0 && idx0 < FastAsinTable.Length,
            $"The {nameof(idx0)} must be between 0 and {FastAsinTable.Length - 1}, but was {idx0}."
        );

        Debug.Assert(
            idx1 >= 0 && idx1 < FastAsinTable.Length,
            $"The {nameof(idx1)} must be between 0 and {FastAsinTable.Length - 1}, but was {idx1}."
        );

        return ((1F - fraction) * FastAsinTable[idx0]) + (fraction * FastAsinTable[idx1]);
    }

    /// <summary>
    /// Returns the angle whose sine is the specified number.
    /// </summary>
    /// <param name="value">A value typically in [-1, 1].</param>
    /// <returns>Arcsin of <paramref name="value"/> in radians.</returns>
    public static float Asin(float value) => float.Asin(value);

    /// <summary>
    /// Returns the angle whose tangent is the specified number.
    /// </summary>
    /// <param name="value">A number.</param>
    /// <returns>Arctangent of <paramref name="value"/> in radians.</returns>
    public static float Atan(float value) => float.Atan(value);

    /// <summary>
    /// Returns the angle whose tangent is the quotient of two specified numbers.
    /// </summary>
    /// <param name="y">The y coordinate of a point.</param>
    /// <param name="x">The x coordinate of a point.</param>
    /// <returns>The angle in radians between the positive x-axis and the point (x, y).</returns>
    public static float Atan2(float y, float x) => float.Atan2(y, x);

    /// <summary>
    /// Returns -1, 0, or 1 depending on the sign of the specified value.
    /// </summary>
    /// <param name="value">A number.</param>
    /// <returns>-1 if negative, 1 if positive, 0 if zero.</returns>
    public static float Sign(float value) =>
        value > 0F ? 1F
        : value < 0F ? -1F
        : 0F;

    /// <summary>
    /// Returns the smallest integral value greater than or equal to the specified number.
    /// </summary>
    /// <param name="value">A number.</param>
    /// <returns>The ceiling of <paramref name="value"/>.</returns>
    public static float Ceil(float value) => float.Ceiling(value);

    /// <summary>
    /// Returns the largest integral value less than or equal to the specified number.
    /// </summary>
    /// <param name="value">A number.</param>
    /// <returns>The floor of <paramref name="value"/>.</returns>
    public static float Floor(float value) => float.Floor(value);

    /// <summary>
    /// Rounds the specified number to the nearest integral value, with halves rounding up.
    /// </summary>
    /// <param name="value">A number.</param>
    /// <returns>The rounded value.</returns>
    public static float Round(float value) => float.Floor(value + .5F);

    /// <summary>
    /// Determines whether the sign bit of the specified float is not set (i.e., value is non-negative).
    /// </summary>
    /// <param name="value">A number.</param>
    /// <returns><see langword="true"/> if non-negative; otherwise, <see langword="false"/>.</returns>
    public static bool FastIsFloatPositive(float value) => (BitConverter.SingleToInt32Bits(value) & 0x8000_0000) == 0;

    /// <summary>
    /// Determines whether <paramref name="value"/> is a power of two.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns><see langword="true"/> if power of two; otherwise, <see langword="false"/>.</returns>
    public static bool IsPowerOf2(uint value) => (value & (value - 1)) == 0;

    /// <summary>
    /// Returns a pseudo-random float in [0, 1].
    /// </summary>
    /// <returns>A number in the range [0, 1].</returns>
    public static float RandomFloat() => (NativeImports.Rand() & 0x0FFF) / (float)0x0FFF;

    /// <summary>
    /// Returns a pseudo-random float in [min, max].
    /// </summary>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound.</param>
    /// <returns>A number in the range [<paramref name="min"/>, <paramref name="max"/>].</returns>
    public static float RandomFloat(float min, float max) => (RandomFloat() * (max - min)) + min;

    /// <summary>
    /// Clamps a value to the inclusive range [min, max].
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The inclusive minimum.</param>
    /// <param name="max">The inclusive maximum.</param>
    /// <returns>The clamped value.</returns>
    public static float Clamp(float value, float min = 0F, float max = 1F) =>
        value < min ? min
        : value > max ? max
        : value;

    /// <summary>
    /// Clamps a value to the inclusive range [min, max].
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The inclusive minimum.</param>
    /// <param name="max">The inclusive maximum.</param>
    /// <returns>The clamped value.</returns>
    public static double Clamp(double value, double min = 0D, double max = 1D) =>
        value < min ? min
        : value > max ? max
        : value;

    /// <summary>
    /// Clamps a value to the inclusive range [min, max].
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The inclusive minimum.</param>
    /// <param name="max">The inclusive maximum.</param>
    /// <returns>The clamped value.</returns>
    public static int Clamp(int value, int min, int max) =>
        value < min ? min
        : value > max ? max
        : value;

    /// <summary>
    /// Wraps a value into the half-open interval [min, max). Values outside are wrapped by the range size.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <param name="min">The inclusive minimum.</param>
    /// <param name="max">The exclusive maximum.</param>
    /// <returns>The wrapped value constrained to [min, max].</returns>
    public static float Wrap(float value, float min = 0F, float max = 1F)
    {
        if (value >= max)
        {
            value -= max - min;
        }

        if (value < min)
        {
            value += max - min;
        }

        if (value < min)
        {
            value = min;
        }

        if (value > max)
        {
            value = max;
        }

        return value;
    }

    /// <summary>
    /// Wraps a value into the half-open interval [min, max). Values outside are wrapped by the range size.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <param name="min">The inclusive minimum.</param>
    /// <param name="max">The exclusive maximum.</param>
    /// <returns>The wrapped value constrained to [min, max].</returns>
    public static double Wrap(double value, double min = 0D, double max = 1D)
    {
        if (value >= max)
        {
            value -= max - min;
        }

        if (value < min)
        {
            value += max - min;
        }

        if (value < min)
        {
            value = min;
        }

        if (value > max)
        {
            value = max;
        }

        return value;
    }

    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <returns><paramref name="a"/> if a &lt; b; otherwise <paramref name="b"/>.</returns>
    public static float Min(float a, float b) => a < b ? a : b;

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <returns><paramref name="a"/> if a &gt; b; otherwise <paramref name="b"/>.</returns>
    public static float Max(float a, float b) => a > b ? a : b;

    /// <summary>
    /// Reinterprets the bits of a <see cref="float"/> as a signed 32-bit integer.
    /// </summary>
    /// <param name="value">The value whose bit pattern to reinterpret.</param>
    /// <returns>The integer representing the bit pattern of <paramref name="value"/>.</returns>
    public static int FloatAsInt(float value) => BitConverter.SingleToInt32Bits(value);

    /// <summary>
    /// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/> by <paramref name="lerp"/>.
    /// </summary>
    /// <param name="a">Start value.</param>
    /// <param name="b">End value.</param>
    /// <param name="lerp">Interpolation factor in [0, 1].</param>
    /// <returns>The interpolated value.</returns>
    public static float Lerp(float a, float b, float lerp) => a + ((b - a) * lerp);

    /// <summary>
    /// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/> by <paramref name="lerp"/>.
    /// </summary>
    /// <param name="a">Start value.</param>
    /// <param name="b">End value.</param>
    /// <param name="lerp">Interpolation factor in [0, 1].</param>
    /// <returns>The interpolated value.</returns>
    public static double Lerp(double a, double b, double lerp) => a + ((b - a) * lerp);

    /// <summary>
    /// Converts a unit float in [0, 1] to a byte in [0, 255].
    /// </summary>
    /// <param name="value">Unit float value.</param>
    /// <returns>An 8-bit value representing <paramref name="value"/>.</returns>
    public static byte UnitFloatToByte(float value) => (byte)(value * 255F);

    /// <summary>
    /// Converts an 8-bit value in [0, 255] to a unit float in [0, 1].
    /// </summary>
    /// <param name="value">An 8-bit value.</param>
    /// <returns>A unit float in [0, 1].</returns>
    public static float ByteToUnitFloat(byte value) => value / 255F;

    /// <summary>
    /// Determines whether the specified float is not NaN or infinity by inspecting exponent bits.
    /// </summary>
    /// <param name="value">A number.</param>
    /// <returns><see langword="true"/> if the value is a normal/subnormal or zero; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidFloat(float value)
    {
        var bits = (ulong)BitConverter.SingleToInt32Bits(value);
        var exponent = (bits & 0x7F80_0000) >> (32 - 9);
        return exponent != 0xFF;
    }

    /// <summary>
    /// Determines whether the specified double is not NaN or infinity by inspecting exponent bits.
    /// </summary>
    /// <param name="value">A number.</param>
    /// <returns><see langword="true"/> if the value is a normal/subnormal or zero; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidDouble(double value)
    {
        var bits = (ulong)BitConverter.DoubleToInt64Bits(value) + 1;
        var exponent = (bits & 0x8FF0_0000) >> (32 - 12);
        return exponent != 0x07FF;
    }

    /// <summary>
    /// Normalizes an angle in radians to the range [-π, π).
    /// </summary>
    /// <param name="angle">Angle in radians.</param>
    /// <returns>The normalized angle in [-π, π).</returns>
    public static float NormalizeAngle(float angle) => angle - (TwoPi * Floor((angle + Pi) / TwoPi));

    private static partial class NativeImports
    {
        /// <summary>
        /// Calls the platform C runtime <c>rand()</c> and returns its value.
        /// </summary>
        /// <returns>A pseudo-random integer from the platform's C library.</returns>
        [SuppressMessage(
            "Interoperability",
            "CA1416:Validate platform compatibility",
            Justification = "This is a false positive."
        )]
        public static int Rand() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? RandMsVCrt()
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? RandLibSystem()
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)
                ? RandLibC()
            : throw new PlatformNotSupportedException();

        [LibraryImport("msvcrt", EntryPoint = "rand")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        [SupportedOSPlatform("windows")]
        private static partial int RandMsVCrt();

        [LibraryImport("libSystem", EntryPoint = "rand")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        [SupportedOSPlatform("osx")]
        private static partial int RandLibSystem();

        [LibraryImport("libc", EntryPoint = "rand")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freebsd")]
        private static partial int RandLibC();
    }
}
