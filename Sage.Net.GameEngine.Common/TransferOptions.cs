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

namespace Sage.Net.GameEngine.Common;

/// <summary>A record that represents flags for transfer options.</summary>
/// <param name="Value">The value of the options bits.</param>
public record TransferOptions(uint Value)
{
    /// <summary>Gets a new <see cref="TransferOptions"/> that represents no options selected.</summary>
    public static TransferOptions None => new(0);

    /// <summary>Gets a new <see cref="TransferOptions"/> that represents the <c>No post-processing</c> option.</summary>
    public static TransferOptions NoPostProcessing => new(1);

    /// <summary>Gets a new <see cref="TransferOptions"/> that represents all possible options being enabled.</summary>
    public static TransferOptions All => new(0xFFFF_FFFF);

    /// <summary>Performs a bitwise OR operation.</summary>
    /// <param name="other">The other <see cref="TransferOptions"/> to perform the operation on.</param>
    /// <returns>A new <see cref="TransferOptions"/> resulting of the OR operation between both <see cref="Value"/>s.</returns>
    public TransferOptions BitwiseOr(TransferOptions other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new TransferOptions(Value | other.Value);
    }

    /// <summary>Performs a bitwise AND operation.</summary>
    /// <param name="other">The other <see cref="TransferOptions"/> to perform the operation on.</param>
    /// <returns>A new <see cref="TransferOptions"/> resulting of the AND operation between both <see cref="Value"/>s.</returns>
    public TransferOptions BitwiseAnd(TransferOptions other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new TransferOptions(Value & other.Value);
    }

    /// <summary>Performs an XOR operation.</summary>
    /// <param name="other">The other <see cref="TransferOptions"/> to perform the operation on.</param>
    /// <returns>A new <see cref="TransferOptions"/> resulting of the XOR operation between both <see cref="Value"/>s.</returns>
    public TransferOptions Xor(TransferOptions other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new TransferOptions(Value ^ other.Value);
    }

    /// <summary>Performs a one's complement operation.</summary>
    /// <returns>A new <see cref="TransferOptions"/> resulting of the one's complement operation on the <see cref="Value"/>.</returns>
    public TransferOptions OnesComplement() => new(~Value);

    /// <summary>Returns the underlying <see cref="uint"/> value.</summary>
    /// <returns>A new <see cref="uint"/> with the <see cref="TransferOptions"/> <see cref="Value"/>.</returns>
    public uint ToUInt32() => Value;

    /// <summary>An operator that allows performing an OR operation between two <see cref="TransferOptions"/>.</summary>
    /// <param name="left">The left-handed <see cref="TransferOptions"/>.</param>
    /// <param name="right">The right-handed <see cref="TransferOptions"/>.</param>
    /// <returns>A new <see cref="TransferOptions"/> resulting from the OR operation.</returns>
    public static TransferOptions operator |(TransferOptions left, TransferOptions right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.BitwiseOr(right);
    }

    /// <summary>An operator that allows performing an AND operation between two <see cref="TransferOptions"/>.</summary>
    /// <param name="left">The left-handed <see cref="TransferOptions"/>.</param>
    /// <param name="right">The right-handed <see cref="TransferOptions"/>.</param>
    /// <returns>A new <see cref="TransferOptions"/> resulting from the AND operation.</returns>
    public static TransferOptions operator &(TransferOptions left, TransferOptions right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.BitwiseAnd(right);
    }

    /// <summary>An operator that allows performing an XOR operation between two <see cref="TransferOptions"/>.</summary>
    /// <param name="left">The left-handed <see cref="TransferOptions"/>.</param>
    /// <param name="right">The right-handed <see cref="TransferOptions"/>.</param>
    /// <returns>A new <see cref="TransferOptions"/> resulting from the XOR operation.</returns>
    public static TransferOptions operator ^(TransferOptions left, TransferOptions right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.Xor(right);
    }

    /// <summary>An operator that allows performing a one's complete operation on a <see cref="TransferOptions"/>.</summary>
    /// <param name="options">The <see cref="TransferOptions"/> to perform the operation on.</param>
    /// <returns>A new <see cref="TransferOptions"/> resulting from the one's complement operation.</returns>
    public static TransferOptions operator ~(TransferOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.OnesComplement();
    }

    /// <summary>An operator to explicitly convert <see cref="TransferOptions"/> to a <see cref="uint"/>.</summary>
    /// <param name="options">The <see cref="TransferOptions"/> to convert.</param>
    /// <returns>A <see cref="uint"/> with the underlying <see cref="Value"/>.</returns>
    public static explicit operator uint(TransferOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.ToUInt32();
    }
}
