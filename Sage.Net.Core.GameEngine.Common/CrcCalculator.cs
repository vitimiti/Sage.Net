// -----------------------------------------------------------------------
// <copyright file="CrcCalculator.cs" company="Sage.Net">
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

using System.Diagnostics;

namespace Sage.Net.Core.GameEngine.Common;

/// <summary>
/// CRC calculator.
/// </summary>
public class CrcCalculator
{
    /// <summary>
    /// Gets or sets the CRC value.
    /// </summary>
    public uint Crc { get; set; }

    /// <summary>
    /// Computes the CRC value.
    /// </summary>
    /// <param name="data">The data to compute the CRC value for.</param>
    public void Compute(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return;
        }

        foreach (var b in data)
        {
            Add(b);
        }
    }

    /// <summary>
    /// Clears the CRC value.
    /// </summary>
    public void Clear() => Crc = 0;

    private void Add(byte value)
    {
        var hiBit = (Crc & 0x8000_0000) != 0 ? 1U : 0U;
        Crc <<= 1;
        Crc += value;
        Crc += hiBit;

        Debug.WriteLine($"CRC: 0x{Crc:X8}.");
    }
}
