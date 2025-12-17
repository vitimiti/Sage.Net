// -----------------------------------------------------------------------
// <copyright file="TransferOperation.cs" company="Sage.Net">
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

/// <summary>Represents a transfer operation.</summary>
public abstract class TransferOperation
{
    /// <summary>Transfers data in a user-defined way in the context of a transfer operation.</summary>
    /// <param name="data">The pointer to the data to be transferred.</param>
    /// <param name="dataSize">The size of the data to be transferred, in bytes.</param>
    public virtual void TransferUser(nint data, int dataSize) => TransferCore(data, dataSize);

    /// <inheritdoc cref="TransferUser(nint, int)"/>
    public virtual unsafe void TransferUser(void* data, int dataSize) => TransferCore(data, dataSize);

    /// <summary>The actual transfer operation.</summary>
    /// <param name="data">The pointer to the data to be transferred.</param>
    /// <param name="dataSize">The size of the data to be transferred, in bytes.</param>
    protected unsafe void TransferCore(nint data, int dataSize) => TransferCore(data.ToPointer(), dataSize);

    /// <inheritdoc cref="TransferCore(nint, int)"/>
    protected abstract unsafe void TransferCore(void* data, int dataSize);
}
