// -----------------------------------------------------------------------
// <copyright file="TimeOfDay.cs" company="Sage.Net">
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

/// <summary>Represents different times of the day.</summary>
public enum TimeOfDay
{
    /// <summary>Represents an invalid or uninitialized time of day.</summary>
    /// <remarks>This value typically indicates that a valid time of day has not been specified or recognized.</remarks>
    Invalid,

    /// <summary>Represents the morning time of day.</summary>
    Morning,

    /// <summary>Represents the afternoon time of day.</summary>
    Afternoon,

    /// <summary>Represents the evening time of day.</summary>
    Evening,

    /// <summary>Represents the night time of day.</summary>
    Night,
}
