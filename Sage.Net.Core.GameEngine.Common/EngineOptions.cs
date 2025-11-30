// -----------------------------------------------------------------------
// <copyright file="EngineOptions.cs" company="Sage.Net">
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

namespace Sage.Net.Core.GameEngine.Common;

/// <summary>
/// Options for the engine.
/// </summary>
public record EngineOptions
{
    /// <summary>
    /// Gets or sets the custom game path.
    /// </summary>
    public string? CustomGamePath { get; set; }

    /// <summary>
    /// Gets or sets the custom BIG file extension.
    /// </summary>
    /// <remarks>This is to support modding.</remarks>
    public string? ModBigCustomExtension { get; set; }

    /// <summary>
    /// Gets or sets the custom big files.
    /// </summary>
    /// <remarks>This is to support modding.</remarks>
    public string? ModBig { get; set; }

    /// <summary>
    /// Gets or sets the mod directory.
    /// </summary>
    /// <remarks>This is to support modding.</remarks>
    public string? ModDir { get; set; }
}
