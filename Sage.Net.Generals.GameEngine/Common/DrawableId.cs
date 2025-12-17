// -----------------------------------------------------------------------
// <copyright file="DrawableId.cs" company="Sage.Net">
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

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Represents a unique identifier for a drawable object.</summary>
public record DrawableId(int Value)
{
    /// <summary>Gets an invalid drawable identifier.</summary>
    public static DrawableId Invalid => new(0);

    /// <summary>Creates a new instance of the <see cref="DrawableId"/> record from a given 32-bit integer value.</summary>
    /// <param name="value">The 32-bit integer value to encapsulate within the <see cref="DrawableId"/>.</param>
    /// <returns>A new instance of the <see cref="DrawableId"/> record with the specified value.</returns>
    public static DrawableId FromInt32(int value) => new(value);

    /// <summary>Converts the current <see cref="DrawableId"/> instance to a 32-bit integer value.</summary>
    /// <returns>The 32-bit integer value encapsulated within the current instance.</returns>
    public int ToInt32() => Value;

    /// <summary>Implicitly converts the specified <see cref="DrawableId"/> instance to a 32-bit integer value.</summary>
    /// <param name="id">The <see cref="DrawableId"/> instance to convert.</param>
    /// <returns>The 32-bit integer value encapsulated within the specified instance.</returns>
    public static implicit operator int(DrawableId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return id.ToInt32();
    }

    /// <summary>Implicitly converts the specified 32-bit integer value to a <see cref="DrawableId"/> instance.</summary>
    /// <param name="value">The 32-bit integer value to convert.</param>
    /// <returns>A <see cref="DrawableId"/> instance representing the specified value.</returns>
    public static implicit operator DrawableId(int value) => FromInt32(value);
}
