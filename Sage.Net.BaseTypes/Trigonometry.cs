// -----------------------------------------------------------------------
// <copyright file="Trigonometry.cs" company="Sage.Net">
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

namespace Sage.Net.BaseTypes;

/// <summary>Trigonometry functions and constants.</summary>
public static class Trigonometry
{
    /// <summary>An approximation of the number pi.</summary>
    public const float TwoPi = 6.28318530718F;

    /// <summary>An approximation of the number pi divided by 180.</summary>
    public const float Deg2Rad = 0.0174532925199F;

    /// <summary>The resolution of trigonometry functions.</summary>
    public const float TrigRes = 4096;

    /// <summary>Same as <see cref="TrigRes"/>, but as an integer.</summary>
    public const int IntOne = 4096;

    /// <summary>2 * <see cref="TrigRes"/>, but as an integer.</summary>
    public const int IntTwoPi = 25736;

    /// <summary>3 * pi / 2, but as an integer.</summary>
    public const int ThreePiOverTwo = 19302;

    /// <summary>Pi, but as an integer.</summary>
    public const int IntPi = 12868;

    /// <summary>Pi / 2, but as an integer.</summary>
    public const int HalfPi = 6434;

    /// <summary>Calculate the sine of a value.</summary>
    /// <param name="x">A <see cref="float"/> with the value to calculate the sine of.</param>
    /// <returns>A <see cref="float"/> with the sine of <paramref name="x"/>.</returns>
    public static float Sin(float x) => float.Sin(x);

    /// <summary>Calculate the cosine of a value.</summary>
    /// <param name="x">A <see cref="float"/> with the value to calculate the cosine of.</param>
    /// <returns>A <see cref="float"/> with the cosine of <paramref name="x"/>.</returns>
    public static float Cos(float x) => float.Cos(x);

    /// <summary>Calculate the tangent of a value.</summary>
    /// <param name="x">A <see cref="float"/> with the value to calculate the tangent of.</param>
    /// <returns>A <see cref="float"/> with the tangent of <paramref name="x"/>.</returns>
    public static float Tan(float x) => float.Tan(x);

    /// <summary>Calculate the arc sine of a value.</summary>
    /// <param name="x">A <see cref="float"/> with the value to calculate the arc sine of.</param>
    /// <returns>A <see cref="float"/> with the arc sine of <paramref name="x"/>.</returns>
    public static float ACos(float x) => float.Acos(x);

    /// <summary>Calculate the arc cosine of a value.</summary>
    /// <param name="x">A <see cref="float"/> with the value to calculate the arc cosine of.</param>
    /// <returns>A <see cref="float"/> with the arc cosine of <paramref name="x"/>.</returns>
    public static float ASin(float x) => float.Asin(x);
}
