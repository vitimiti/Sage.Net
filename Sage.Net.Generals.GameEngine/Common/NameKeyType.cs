// -----------------------------------------------------------------------
// <copyright file="NameKeyType.cs" company="Sage.Net">
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

/// <summary>Represents a record structure for handling specific integer values with additional semantic meaning.</summary>
/// <remarks>This type is designed to encapsulate integer values with specific significance, such as predefined constants and implicit conversions, providing a strongly-typed way to represent and manipulate these values within the system.</remarks>
/// <param name="Value">The integer value associated with the instance.</param>
public record NameKeyType(int Value)
{
    /// <summary>Gets an invalid or uninitialized instance of the <see cref="NameKeyType"/> structure.</summary>
    /// <remarks>The <c>Invalid</c> property is a predefined constant representing a default value of 0. It is used to signify an invalid or undefined state for the <see cref="NameKeyType"/>.</remarks>
    public static NameKeyType Invalid => new(0);

    /// <summary>Gets the maximum allowable instance of the <see cref="NameKeyType"/> structure.</summary>
    /// <remarks>The <c>Max</c> property represents the upper limit for values encapsulated by the <see cref="NameKeyType"/> structure. It is defined as the result of a bitwise left shift operation to allow sufficient range for future extensibility.</remarks>
    public static NameKeyType Max => new(1 << 23);

    /// <summary>Creates a new instance of the <see cref="NameKeyType"/> record from a given 32-bit integer value.</summary>
    /// <param name="value">The 32-bit integer value to encapsulate within the <see cref="NameKeyType"/>.</param>
    /// <returns>A new instance of the <see cref="NameKeyType"/> record with the specified value.</returns>
    public static NameKeyType FromInt32(int value) => new(value);

    /// <summary>Retrieves the encapsulated 32-bit integer value from the current <see cref="NameKeyType"/> instance.</summary>
    /// <returns>The 32-bit integer value associated with this <see cref="NameKeyType"/> instance.</returns>
    public int ToInt32() => Value;

    /// <summary>Defines an implicit conversion from a <see cref="NameKeyType"/> instance to a 32-bit integer.</summary>
    /// <param name="type">The <see cref="NameKeyType"/> instance to convert.</param>
    /// <returns>The 32-bit integer value encapsulated within the provided <see cref="NameKeyType"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided <see cref="NameKeyType"/> instance is null.</exception>
    public static implicit operator int(NameKeyType type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.ToInt32();
    }

    /// <summary>Defines an implicit conversion from a 32-bit integer to a <see cref="NameKeyType"/> instance.</summary>
    /// <param name="value">The 32-bit integer value to convert into a <see cref="NameKeyType"/> instance.</param>
    /// <returns>A new <see cref="NameKeyType"/> instance encapsulating the specified integer value.</returns>
    public static implicit operator NameKeyType(int value) => FromInt32(value);
}
