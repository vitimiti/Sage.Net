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

namespace Sage.Net.Core.GameEngine.Common;

/// <summary>Represents a CRC (Cyclic Redundancy Check) computation class.</summary>
/// <remarks>This class is used to compute and manage CRC values based on a given byte buffer.</remarks>
public class Crc
{
    /// <summary>Gets the computed CRC value.</summary>
    public uint Value { get; private set; }

    /// <summary>Computes the CRC value based on the provided byte buffer and updates the current CRC value.</summary>
    /// <param name="buffer">A read-only span of bytes containing the data to compute the CRC from.</param>
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Compute(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 1)
        {
            return;
        }

        foreach (var b in buffer)
        {
            var hiBit = (Value & 0x8000_0000) != 0 ? 1U : 0U;
            Value <<= 1;
            Value += b;
            Value += hiBit;
        }
    }

    /// <summary>Clears the current CRC value by resetting it to zero.</summary>
    public void Clear() => Value = 0;
}
