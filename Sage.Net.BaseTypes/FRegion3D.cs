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

using System.Diagnostics.CodeAnalysis;

namespace Sage.Net.BaseTypes;

/// <summary>
/// A 3D region with <see cref="float"/> precision.
/// </summary>
/// <param name="Low">The <see cref="FCoord3D"/> that represents the lower part of the range.</param>
/// <param name="High">The <see cref="FCoord3D"/> that represents the higher part of the range.</param>
public record FRegion3D(FCoord3D Low, FCoord3D High)
{
    /// <summary>
    /// Gets the zero region.
    /// </summary>
    public static FRegion3D Zero => new(FCoord3D.Zero, FCoord3D.Zero);

    /// <summary>
    /// Gets the width of the region.
    /// </summary>
    public float Width => High.X - Low.X;

    /// <summary>
    /// Gets the height of the region.
    /// </summary>
    public float Height => High.Y - Low.Y;

    /// <summary>
    /// Gets the depth of the region.
    /// </summary>
    public float Depth => High.Z - Low.Z;

    /// <summary>
    /// Checks if a point is inside the region.
    /// </summary>
    /// <param name="point">The <see cref="FCoord3D"/> to check.</param>
    /// <param name="useZ"><see langword="true"/> to check with Z components; <see langword="false"/> otherse. <see langword="true"/> by default.</param>
    /// <returns><see langword="true"/> if the <paramref name="point"/> is within the region's boundaries; <see langword="false"/> otherwise.</returns>
    public bool IsInRegion([NotNull] FCoord3D point, bool useZ = true) =>
        useZ
            ? Low.X < point.X
                && point.X < High.X
                && Low.Y < point.Y
                && point.Y < High.Y
                && Low.Z < point.Z
                && point.Z < High.Z
            : Low.X < point.X && point.X < High.X && Low.Y < point.Y && point.Y < High.Y;
}
