// -----------------------------------------------------------------------
// <copyright file="TransferCrc.cs" company="Sage.Net">
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

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Represents a transfer operation that calculates and tracks a cyclic redundancy check (CRC) value for data being processed during the operation. This class specializes the <see cref="TransferOperation"/> to focus on CRC computation, providing methods to calculate and update the CRC value as data is transferred.</summary>
public class TransferCrc : TransferOperation
{
    private uint _crc;

    /// <summary>Initializes a new instance of the <see cref="TransferCrc"/> class.</summary>
    public TransferCrc() => Mode = TransferMode.Crc;

    /// <summary>Gets the CRC value for the data processed during the operation.</summary>
    public uint Crc => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(_crc) : _crc;

    /// <summary>Opens the transfer operation with the specified identifier and initializes the cyclic redundancy check (CRC) value for the operation.</summary>
    /// <param name="identifier">The unique identifier for the transfer operation.</param>
    public override void Open(string identifier)
    {
        base.Open(identifier);
        _crc = 0;
    }

    /// <inheritdoc/>
    /// <remarks>This method does nothing.</remarks>
    public override void Close() { }

    /// <inheritdoc/>
    /// <remarks>This method does nothing.</remarks>
    public override int BeginBlock() => 0;

    /// <inheritdoc/>
    /// <remarks>This method does nothing.</remarks>
    public override void EndBlock() { }

    /// <inheritdoc/>
    /// <remarks>This method does nothing.</remarks>
    public override void Skip(int dataSize) { }

    /// <summary>Transfers a snapshot of the current state using the provided <see cref="ISnapshot"/> instance.</summary>
    /// <param name="snapshot">The <see cref="ISnapshot"/> instance used to handle the transfer of the snapshot. Can be <see langword="null"/>.</param>
    public override void TransferSnapshot(ISnapshot? snapshot) => snapshot?.Crc(this);

    /// <summary>Adds the specified CRC value to the current CRC value.</summary>
    /// <param name="value">The CRC value to add.</param>
    protected void AddCrc(uint value)
    {
        var be = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        _crc = (_crc << 1) + be + ((_crc >> 31) & 0x0001);
    }

    /// <inheritdoc/>
    /// <summary>Performs the core operation of the CRC-based data transfer, processing the given data and updating the CRC value accordingly. This method is specialized to handle byte-aligned and uneven byte transfers efficiently.</summary>
    protected override unsafe void TransferCore(void* data, int dataSize)
    {
        var uintPtr = (uint*)data;
        dataSize *= data is not null ? 1 : 0;

        var dataBytes = dataSize / 4;
        for (var i = 0; i < dataBytes; i++)
        {
            AddCrc(*uintPtr++);
        }

        var value = 0U;
        var c = (byte*)uintPtr;
        var switchValue = dataSize & 3;
        if (switchValue == 0)
        {
            return;
        }

        if (switchValue >= 3)
        {
            value += (uint)(c[2] << 16);
        }

        if (switchValue >= 2)
        {
            value += (uint)(c[1] << 8);
        }

        // switchValue is 1, 2, or 3 here
        value += c[0];
        unchecked
        {
            _crc = (_crc << 1) + value + ((_crc >> 31) & 0x0001u);
        }
    }
}
