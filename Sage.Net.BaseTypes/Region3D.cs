// -----------------------------------------------------------------------
// <copyright file="Region3D.cs" company="Sage.Net">
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

/// <summary>A record representing a 3D region.</summary>
/// <param name="Lo">A <see cref="Coord3D"/> representing the low end of the region.</param>
/// <param name="Hi">A <see cref="Coord3D"/> representing the high end of the region.</param>
public record Region3D(Coord3D Lo, Coord3D Hi)
{
    /// <summary>Gets the zero <see cref="Region3D"/>.</summary>
    public static Region3D Zero => new(Coord3D.Zero, Coord3D.Zero);

    /// <summary>Gets the width of the region.</summary>
    public float Width => Hi.X - Lo.X;

    /// <summary>Gets the height of the region.</summary>
    public float Height => Hi.Y - Lo.Y;

    /// <summary>Gets the depth of the region.</summary>
    public float Depth => Hi.Z - Lo.Z;

    /// <summary>Checks if a <see cref="Coord3D"/> is in the region.</summary>
    /// <param name="query">The <see cref="Coord3D"/> to query.</param>
    /// <returns><see langword="true"/> if the <paramref name="query"/> is in the region; <see langword="false"/> otherwise. The Z coordinates are ignored.</returns>
    public bool IsInRegionNoZ(Coord3D query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return Lo.X < query.X && query.X < Hi.X && Lo.Y < query.Y && query.Y < Hi.Y;
    }

    /// <summary>Checks if a <see cref="Coord3D"/> is in the region.</summary>
    /// <param name="query">The <see cref="Coord3D"/> to query.</param>
    /// <returns><see langword="true"/> if the <paramref name="query"/> is in the region; <see langword="false"/> otherwise.</returns>
    public bool IsInRegionWithZ(Coord3D query) => IsInRegionNoZ(query) && Lo.Z <= query.Z;
}
