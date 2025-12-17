// -----------------------------------------------------------------------
// <copyright file="TransferOptions.cs" company="Sage.Net">
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

/// <summary>Represents a set of transfer options.</summary>
public record TransferOptions(uint Value)
{
    /// <summary>Gets an empty set of transfer options.</summary>
    public static TransferOptions None => new(0U);

    /// <summary>Gets a set of transfer options that disables post-processing.</summary>
    public static TransferOptions NoPostProcessing => new(1U);

    /// <summary>Gets all the possible transfer options.</summary>
    public static TransferOptions All => new(0xFFFF_FFFFU);

    /// <summary>Creates a <see cref="TransferOptions"/> instance from the specified unsigned 32-bit integer value.</summary>
    /// <param name="value">The unsigned 32-bit integer representing the transfer options.</param>
    /// <returns>A <see cref="TransferOptions"/> instance corresponding to the given value.</returns>
    public static TransferOptions FromUInt32(uint value) => new(value);

    /// <summary>Converts the current instance to an unsigned 32-bit integer value.</summary>
    /// <returns>An unsigned 32-bit integer representing the current transfer options.</returns>
    public uint ToUInt32() => Value;

    /// <summary>Implicitly converts the specified <see cref="TransferOptions"/> instance to an unsigned 32-bit integer value.</summary>
    /// <param name="options">The <see cref="TransferOptions"/> instance to convert.</param>
    /// <returns>An unsigned 32-bit integer representing the specified transfer options.</returns>
    public static implicit operator uint(TransferOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.ToUInt32();
    }

    /// <summary>Implicitly converts the specified unsigned 32-bit integer value to a <see cref="TransferOptions"/> instance.</summary>
    /// <param name="value">The unsigned 32-bit integer value to convert.</param>
    /// <returns>A <see cref="TransferOptions"/> instance representing the specified value.</returns>
    public static implicit operator TransferOptions(uint value) => FromUInt32(value);
}
