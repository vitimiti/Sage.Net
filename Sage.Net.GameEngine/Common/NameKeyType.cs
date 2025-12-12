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

namespace Sage.Net.GameEngine.Common;

/// <summary>A record that represents the value of a name key type.</summary>
/// <param name="Value">An <see cref="int"/> with the value.</param>
public record NameKeyType(int Value)
{
    /// <summary>Gets the invalid <see cref="NameKeyType"/>.</summary>
    public static NameKeyType Invalid => new(0);

    /// <summary>Gets the maximum <see cref="NameKeyType"/> values allowed.</summary>
    public static NameKeyType Max => new(1 << 23);

    /// <summary>Converts the <see cref="NameKeyType"/> to a <see cref="uint"/>.</summary>
    /// <returns>The <see cref="Value"/> as a <see cref="uint"/>.</returns>
    public uint ToUInt32() => (uint)Value;

    /// <summary>Explicitly converts a <see cref="NameKeyType"/> to a <see cref="uint"/>.</summary>
    /// <param name="type">The <see cref="NameKeyType"/> to convert.</param>
    /// <returns>A new <see cref="uint"/> with the <paramref name="type"/>'s <see cref="Value"/>.</returns>
    public static explicit operator uint(NameKeyType type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.ToUInt32();
    }
}
