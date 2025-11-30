// -----------------------------------------------------------------------
// <copyright file="Xfer.cs" company="Sage.Net">
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

namespace Sage.Net.Core.GameEngine.Common.Transfer;

/// <summary>
/// Base class for all transfer objects.
/// </summary>
public abstract class Xfer
{
    /// <summary>
    /// Gets or sets the transfer options.
    /// </summary>
    protected XferOptions Options { get; set; }

    /// <summary>
    /// Gets or sets the transfer mode.
    /// </summary>
    protected XferMode Mode { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    protected string? Identifier { get; set; }

    /// <summary>
    /// Opens the transfer object.
    /// </summary>
    /// <param name="identifier">The identifier for the transfer object.</param>
    public virtual void Open(string identifier) => Identifier = identifier;

    /// <summary>
    /// Closes the transfer object.
    /// </summary>
    public abstract void Close();

    /// <summary>
    /// User defined transfer.
    /// </summary>
    /// <param name="data">The data to transfer.</param>
    public virtual void User(Span<byte> data) => TransferCore(data);

    /// <summary>
    /// Implementation of the transfer object.
    /// </summary>
    /// <param name="data">The data to transfer.</param>
    protected abstract void TransferCore(Span<byte> data);
}
