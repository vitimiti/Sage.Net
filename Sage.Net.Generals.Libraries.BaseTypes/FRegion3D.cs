// -----------------------------------------------------------------------
// <copyright file="FRegion3D.cs" company="Sage.Net">
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

namespace Sage.Net.Generals.Libraries.BaseTypes;

/// <summary>Represents a cubic region of space.</summary>
/// <param name="Lo">The lower-left corner of the region.</param>
/// <param name="Hi">The upper-right corner of the region.</param>
public record FRegion3D(FCoord3D Lo, FCoord3D Hi)
{
    /// <summary>Gets the zero region.</summary>
    public static FRegion3D Zero => new(FCoord3D.Zero, FCoord3D.Zero);

    /// <summary>Gets the width of the region.</summary>
    public float Width => Hi.X - Lo.X;

    /// <summary>Gets the height of the region.</summary>
    public float Height => Hi.Y - Lo.Y;

    /// <summary>Gets the depth of the region.</summary>
    public float Depth => Hi.Z - Lo.Z;

    /// <summary>Determines whether a given coordinate is within the horizontal bounds of the region, ignoring the z-dimension.</summary>
    /// <param name="query">The three-dimensional coordinate to test.</param>
    /// <returns><see langword="true"/> if the coordinate is within the horizontal bounds of the region; otherwise, <see langword="false"/>.</returns>
    public bool IsInRegionNoZ(FCoord3D query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return Lo.X < query.X && query.X < Hi.X && Lo.Y < query.Y && query.Y < Hi.Y;
    }

    /// <summary>Determines whether a given coordinate is within the region.</summary>
    /// <param name="query">The three-dimensional coordinate to test.</param>
    /// <returns><see langword="true"/> if the coordinate is within the region; otherwise, <see langword="false"/>.</returns>
    public bool IsInRegion(FCoord3D query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return IsInRegionNoZ(query) && Lo.Z < query.Z && query.Z < Hi.Z;
    }
}
