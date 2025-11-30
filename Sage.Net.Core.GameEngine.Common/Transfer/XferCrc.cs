// -----------------------------------------------------------------------
// <copyright file="XferCrc.cs" company="Sage.Net">
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

using System.Buffers.Binary;

namespace Sage.Net.Core.GameEngine.Common.Transfer;

/// <summary>
/// The CRC transfer operations.
/// </summary>
public class XferCrc : Xfer
{
    private uint _crc;

    /// <summary>
    /// Gets or sets the CRC value.
    /// </summary>
    public uint Crc
    {
        get => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(_crc) : _crc;
        protected set => _crc = value;
    }

    /// <inheritdoc/>
    public override void Open(string identifier)
    {
        base.Open(identifier);
        Crc = 0;
    }

    /// <inheritdoc/>
    /// <note>This is a no-op.</note>
    public override void Close() { }

    /// <summary>
    /// The transfer implementation.
    /// </summary>
    /// <param name="data">The data to transfer.</param>
    protected override void TransferCore(Span<byte> data)
    {
        var dataBytes = data.Length / 4;
        int i;
        for (i = 0; i < dataBytes; i++)
        {
            AddCrc(BinaryPrimitives.ReadUInt32LittleEndian(data[(i * 4)..]));
        }

        var value = 0U;
        var offset = i * 4;
        var remainder = data.Length & 3;

        if (remainder >= 3)
        {
            value += (uint)(data[offset + 2] << 16);
        }

        if (remainder >= 2)
        {
            value += (uint)(data[offset + 1] << 8);
        }

        if (remainder >= 1)
        {
            value += data[offset];
            _crc = (_crc << 1) + value + ((_crc >> 31) & 0x01);
        }
    }

    /// <summary>
    /// Adds a CRC value.
    /// </summary>
    /// <param name="value">The value to add to <see cref="Crc"/>.</param>
    protected void AddCrc(uint value) =>
        Crc =
            (_crc << 1)
            + (BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value)
            + ((_crc >> 31) & 0x01);
}
