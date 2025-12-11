// -----------------------------------------------------------------------
// <copyright file="SingleRange.cs" company="Sage.Net">
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

/// <summary>A record representing a range of values.</summary>
/// <param name="Lo">A <see cref="float"/> representing the low end of the range.</param>
/// <param name="Hi">A <see cref="float"/> representing the high end of the range.</param>
public record SingleRange(float Lo, float Hi)
{
    /// <summary>Combine two ranges together.</summary>
    /// <param name="other">The other <see cref="SingleRange"/> to combine this instance with.</param>
    /// <returns>A new <see cref="SingleRange"/> resulting of the combination of this instance with the <paramref name="other"/> range.</returns>
    public SingleRange Combine(SingleRange other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new SingleRange(float.Min(Lo, other.Lo), float.Max(Hi, other.Hi));
    }
}
