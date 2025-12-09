// -----------------------------------------------------------------------
// <copyright file="BitUtility.cs" company="Sage.Net">
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

/// <summary>A utility class to manage bits.</summary>
public static class BitUtility
{
    /// <summary>Checks whether a set of bits is set or not.</summary>
    /// <param name="x">A <see cref="uint"/> with the bits to operate on.</param>
    /// <param name="i">A <see cref="uint"/> with the bit to check whether it has been set or not.</param>
    /// <returns><see langword="true"/> if <paramref name="x"/> has <paramref name="i"/> as part of its bits; otherwise <see langword="false"/>.</returns>
    public static bool BitIsSet(uint x, uint i) => (x & i) != 0;

    /// <summary>Sets a bit by combining it with existing bits.</summary>
    /// <param name="x">A <see cref="uint"/> with the existing bits to operate on.</param>
    /// <param name="i">A <see cref="uint"/> with the bits to set.</param>
    /// <returns>A new <see cref="uint"/> with the values of <paramref name="x"/> and <paramref name="i"/> OR'd together.</returns>
    public static uint BitSet(uint x, uint i) => x | i;

    /// <summary>Clears bits from a number.</summary>
    /// <param name="x">A <see cref="uint"/> with the bits to operate on.</param>
    /// <param name="i">A <see cref="uint"/> with the bits to clear.</param>
    /// <returns>A new <see cref="uint"/> with the value of <paramref name="x"/> with the bits in <paramref name="i"/> cleared by performing an AND NOT operation.</returns>
    public static uint BitClear(uint x, uint i) => x & ~i;

    /// <summary>Toggles bits from a number.</summary>
    /// <param name="x">A <see cref="uint"/> with the existing bits to operate on.</param>
    /// <param name="i">A <see cref="uint"/> with the bits to toggle.</param>
    /// <returns>A new <see cref="uint"/> with the value of <paramref name="x"/> with the bits in <paramref name="i"/> toggled by performing an XOR operation.</returns>
    public static uint BitToggle(uint x, uint i) => x ^ i;
}
