// -----------------------------------------------------------------------
// <copyright file="CommandLineData.cs" company="Sage.Net">
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

/// <summary>Represents data parsed from the command line, typically used for startup and initialization within the game engine.</summary>
public class CommandLineData
{
    /// <summary>Gets or sets a value indicating whether the command line has been parsed for startup.</summary>
    public bool HasParsedCommandLineForStartup { get; set; }

    /// <summary>Gets or sets a value indicating whether the command line has been parsed for engine initialization.</summary>
    public bool HasParsedCommandLineForEngineInitialization { get; set; }
}
