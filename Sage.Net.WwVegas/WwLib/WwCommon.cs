// -----------------------------------------------------------------------
// <copyright file="WwCommon.cs" company="Sage.Net">
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

namespace Sage.Net.WwVegas.WwLib;

/// <summary>Common Westwood/Vegas timing constants used by the simulation.</summary>
public static class WwCommon
{
    /// <summary>The number of synchronization ticks processed each second by the game loop.</summary>
    /// <remarks>Typical RTS simulation step rate derived from the original engine.</remarks>
    public const int SyncPerSecond = 30;

    /// <summary>Duration of a single synchronization tick in milliseconds.</summary>
    /// <remarks><c>1000 / <see cref="SyncPerSecond"/></c>.</remarks>
    public const int SyncMilliseconds = 1000 / SyncPerSecond;
}
