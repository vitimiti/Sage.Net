// -----------------------------------------------------------------------
// <copyright file="FRegion2D.cs" company="Sage.Net">
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

/// <summary>
/// A 2D region with <see cref="float"/> precision.
/// </summary>
/// <param name="Low">The <see cref="FCoord2D"/> representing the lower part of the region.</param>
/// <param name="High">The <see cref="FCoord2D"/> representing the higher part of the region.</param>
/// <seealso cref="Region2D"/>
public record FRegion2D(FCoord2D Low, FCoord2D High)
{
    /// <summary>
    /// Gets the width of the region.
    /// </summary>
    public float Width => High.X - Low.X;

    /// <summary>
    /// Gets the height of the region.
    /// </summary>
    public float Height => High.Y - Low.Y;
}
