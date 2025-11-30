// -----------------------------------------------------------------------
// <copyright file="ISnapshot.cs" company="Sage.Net">
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

using Sage.Net.Core.GameEngine.Common.Transfer;

namespace Sage.Net.Core.GameEngine.Common;

/// <summary>
/// Interface for snapshot objects.
/// </summary>
public interface ISnapshot
{
    /// <summary>
    /// The snapshot CRC value.
    /// </summary>
    /// <param name="xfer">The <see cref="Xfer"/> based transfer class.</param>
    protected void Crc(Xfer? xfer);

    /// <summary>
    /// The snapshot data transfer.
    /// </summary>
    /// <param name="xfer">The <see cref="Xfer"/> based transfer class.</param>
    protected void Xfer(Xfer? xfer);

    /// <summary>
    /// The post-load process.
    /// </summary>
    protected void LoadPostProcess();
}
