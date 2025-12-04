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

#if !DEBUG
using System.Runtime.CompilerServices;
#endif

namespace Sage.Net.GameEngine.Common;

/// <summary>
/// Cyclic Redundancy Check (CRC) accumulator used to produce a simple rolling checksum.
/// </summary>
public class Crc
{
#if RETAIL_COMPATIBLE_CRC
    /// <summary>
    /// Gets or sets the current CRC value.
    /// </summary>
    public uint Value { get; set; }
#else
    /// <summary>
    /// Gets the current CRC value.
    /// </summary>
    public uint Value { get; private set; }
#endif

    /// <summary>
    /// Updates the CRC with the provided data.
    /// </summary>
    /// <param name="data">The read-only span of bytes to incorporate into the checksum.</param>
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void ComputeCrc(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return;
        }

        foreach (var b in data)
        {
            AddCrc(b);
        }
    }

    /// <summary>
    /// Resets the CRC value to zero.
    /// </summary>
    public void Clear() => Value = 0;

    private void AddCrc(uint value)
    {
        var hiBit = (Value & 0x8000_0000) != 0 ? 1 : 0;
        Value <<= 1;
        Value += value;
        Value += (uint)hiBit;
    }
}
