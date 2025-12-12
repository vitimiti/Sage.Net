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

namespace Sage.Net.GameEngine.Common;

/// <summary>The interface for objects that will be considered during game saves, loads, and CRC checks.</summary>
public interface ISnapshot
{
    /// <summary>Run the "light" CRC check on the snapshot's data.</summary>
    /// <param name="operation">The <see cref="TransferOperation"/> to run the CRC check with.</param>
    void Crc(TransferOperation operation);

    /// <summary>Run, save, load, or deep CRC check on this snapshot's data.</summary>
    /// <param name="operation">The <see cref="TransferOperation"/> to operate with.</param>
    /// <remarks>The type depends on the setup of the <paramref name="operation"/> object.</remarks>
    void Transfer(TransferOperation operation);

    /// <summary>Post-process phase for loading save games.</summary>
    /// <remarks>All save systems have their <see cref="TransferOperation"/> run using the <see cref="TransferMode.Load"/> mode, and then all systems each have their post-process run.</remarks>
    void LoadPostProcess();
}
