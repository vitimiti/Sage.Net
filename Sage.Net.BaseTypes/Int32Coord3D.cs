// -----------------------------------------------------------------------
// <copyright file="Int32Coord3D.cs" company="Sage.Net">
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

/// <summary>A 3D coordinate represented as integers.</summary>
/// <param name="X">An <see cref="int"/> representing the X coordinate.</param>
/// <param name="Y">An <see cref="int"/> representing the Y coordinate.</param>
/// <param name="Z">An <see cref="int"/> representing the Z coordinate.</param>
public record Int32Coord3D(int X, int Y, int Z)
{
    /// <summary>Gets the zero <see cref="Int32Coord3D"/>.</summary>
    public static Int32Coord3D Zero => new(0, 0, 0);

    /// <summary>Gets the length of the coordinate.</summary>
    public int Length => (int)float.Sqrt((X * X) + (Y * Y) + (Z * Z));
}
