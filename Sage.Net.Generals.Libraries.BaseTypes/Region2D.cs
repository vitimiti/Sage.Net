// -----------------------------------------------------------------------
// <copyright file="Region2D.cs" company="Sage.Net">
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

/// <summary>Represents a rectangular region of space.</summary>
/// <param name="Lo">The lower-left corner of the region.</param>
/// <param name="Hi">The upper-right corner of the region.</param>
public record Region2D(Coord2D Lo, Coord2D Hi)
{
    /// <summary>Gets the width of the region.</summary>
    public int Width => Hi.X - Lo.X;

    /// <summary>Gets the height of the region.</summary>
    public int Height => Hi.Y - Lo.Y;
}
