// -----------------------------------------------------------------------
// <copyright file="FRange.cs" company="Sage.Net">
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

/// <summary>Represents a range of floating-point values.</summary>
/// <param name="Lo">The lower bound of the range.</param>
/// <param name="Hi">The upper bound of the range.</param>
public record FRange(float Lo, float Hi)
{
    /// <summary>Combines two ranges into a single range.</summary>
    /// <param name="other">The other range to combine with this one.</param>
    /// <returns>A new range that represents the combined range of both input ranges.</returns>
    public FRange Combine(FRange other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new FRange(float.Min(Lo, other.Lo), float.Max(Hi, other.Hi));
    }
}
