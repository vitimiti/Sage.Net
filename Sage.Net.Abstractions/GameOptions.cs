// -----------------------------------------------------------------------
// <copyright file="GameOptions.cs" company="Sage.Net">
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

namespace Sage.Net.Abstractions;

/// <summary>
/// Provides configuration options for the Sage.Net game engine.
/// </summary>
/// <remarks>
/// This class encapsulates settings that define the identity and visual presentation
/// of the game, such as its identifier and window title. These options are typically
/// loaded from configuration files at startup.
/// </remarks>
public class GameOptions
{
    /// <summary>
    /// Gets or sets the unique identifier for the game.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the game's unique ID. The default is <c>"SageGame"</c>.
    /// </value>
    /// <remarks>
    /// The <see cref="GameId"/> is used as a technical identifier for the game context,
    /// affecting directory paths for data storage, configuration lookups, and internal
    /// engine registration.
    /// </remarks>
    public string GameId { get; set; } = "SageGame";

    /// <summary>
    /// Gets or sets the title displayed in the game's application window.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the user-facing window title. The default is <c>"Sage.Net Game"</c>.
    /// </value>
    /// <remarks>
    /// This property defines the text that appears in the operating system's window
    /// title bar. It is primarily used for branding and user identification of the
    /// running application.
    /// </remarks>
    public string WindowTitle { get; set; } = "Sage.Net Game";

    /// <summary>
    /// Gets the collection of base directories where game assets are located.
    /// </summary>
    /// <value>
    /// An <see cref="IList{String}"/> containing paths to the primary directories used for loading game assets.
    /// The collection can include multiple paths to accommodate distributed or modular content storage.
    /// </value>
    /// <remarks>
    /// <para>
    /// The <see cref="BaseGamePaths"/> property enables the game engine to locate essential resources,
    /// such as textures, models, and other assets, across specified directories.
    /// This is particularly useful for scenarios where the game assets are spread across multiple locations,
    /// such as custom mods, extensions, or localized configurations.
    /// </para>
    /// <para>
    /// The game assets will be searched for in the order specified in the collection. The first matching asset
    /// will be used, and the rest will be ignored.
    /// </para>
    /// </remarks>
    public IList<string> BaseGamePaths { get; } = [Environment.CurrentDirectory];
}
