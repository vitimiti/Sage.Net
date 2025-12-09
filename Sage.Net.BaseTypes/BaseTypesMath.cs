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
}
