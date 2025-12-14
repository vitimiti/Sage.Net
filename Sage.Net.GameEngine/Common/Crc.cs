// -----------------------------------------------------------------------
// <copyright file="Crc.cs" company="Sage.Net">
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

#if !RTS_DEBUG
using System.Runtime.CompilerServices;
#endif

namespace Sage.Net.GameEngine.Common;

/// <summary>Computes a simple running CRC-like value used by the original game engine for quick integrity checks.</summary>
/// <remarks>This is not a standard CRC polynomial implementation; it shifts the current value, adds the input byte and propagates the previous high bit.</remarks>
public class Crc
{
#if RETAIL_COMPATIBLE_CRC
    /// <summary>Gets or sets the current checksum value.</summary>
    public uint Value { get; set; }
#else
    /// <summary>Gets the current checksum value.</summary>
    public uint Value { get; private set; }
#endif

    /// <summary>Updates the <see cref="Value"/> by consuming all bytes from the provided <paramref name="buffer"/> using the engine-compatible algorithm.</summary>
    /// <param name="buffer">The sequence of bytes to incorporate into the checksum.</param>
#if !RTS_DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Compute(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 1)
        {
            return;
        }

        foreach (var value in buffer)
        {
            var hiBit = (Value & 0x8000_0000) != 0 ? 1U : 0U;

            Value <<= 1;
            Value += value;
            Value += hiBit;
        }
    }

    /// <summary>Resets the <see cref="Value"/> to zero.</summary>
    public void Clear() => Value = 0;
}
