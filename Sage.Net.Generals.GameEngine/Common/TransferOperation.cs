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
    /// <summary>Gets or sets the transfer mode.</summary>
    public TransferMode Mode { get; protected set; } = TransferMode.Invalid;

    /// <summary>Gets or sets the transfer identifier.</summary>
    public string Identifier { get; protected set; } = string.Empty;

    /// <summary>Gets or sets the transfer options.</summary>
    public TransferOptions Options { get; protected set; } = TransferOptions.None;

    /// <summary>Opens a transfer operation using the specified identifier.</summary>
    /// <param name="identifier">The unique identifier for the transfer operation to be opened.</param>
    /// <remarks>This method is intended to initialize and prepare the transfer operation with the given <paramref name="identifier"/>.</remarks>
    public virtual void Open(string identifier) => Identifier = identifier;

    /// <summary>Closes the transfer operation, finalizing and releasing any associated resources.</summary>
    /// <remarks>This method is intended to complete the transfer process and clean up any resources allocated during the operation.</remarks>
    public abstract void Close();

    /// <summary>Begins a new block in the transfer operation.</summary>
    /// <returns>An integer representing the identifier of the newly started block.</returns>
    /// <remarks>This method is intended to initialize a new segment within the transfer operation for further processing.</remarks>
    public abstract int BeginBlock();

    /// <summary>Ends the current block in the transfer operation.</summary>
    /// <remarks>This method is intended to finalize the processing of the current segment within the transfer operation.</remarks>
    public abstract void EndBlock();

    /// <summary>Skips the specified amount of data in the transfer operation.</summary>
    /// <param name="dataSize">The size of the data to skip, in bytes.</param>
    /// <remarks>This method is intended to move the transfer operation's internal pointer forward by the given <paramref name="dataSize"/>, effectively ignoring the data in that range.</remarks>
    public abstract void Skip(int dataSize);

    /// <summary>Transfers a snapshot using the specified snapshot object.</summary>
    /// <param name="snapshot">The snapshot instance that implements <see cref="ISnapshot"/> and will be processed in the transfer operation.</param>
    /// <remarks>This method is intended to facilitate the transfer of state information encapsulated in the specified <paramref name="snapshot"/> during save, load, or CRC operations.</remarks>
    public abstract void TransferSnapshot(ISnapshot? snapshot);

    /// <summary>Sets the transfer options for the transfer operation.</summary>
    /// <param name="options">The transfer options to be applied.</param>
    /// <remarks>This will add the given <paramref name="options"/> to any existing ones.</remarks>
    public virtual void SetOptions(TransferOptions options) => Options |= options;

    /// <summary>Clears the transfer options for the transfer operation.</summary>
    /// <param name="options">The transfer options to be cleared.</param>
    /// <remarks>This will remove the given <paramref name="options"/> from any existing ones.</remarks>
    public virtual void ClearOptions(TransferOptions options) => Options &= ~options;

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
