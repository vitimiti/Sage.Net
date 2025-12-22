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
    /// Gets or sets the horizontal resolution of the game window.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing the width, in pixels, of the game window.
    /// The default value is <c>800</c>.
    /// </value>
    /// <remarks>
    /// The <see cref="XResolution"/> determines the horizontal display size of the game,
    /// impacting how the game content is rendered on screen. It should be combined
    /// with <see cref="YResolution"/> for specifying the complete resolution.
    /// </remarks>
    public int XResolution { get; set; } = 800;

    /// <summary>
    /// Gets or sets the vertical resolution of the game window.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing the height, in pixels, of the game window.
    /// The default value is <c>600</c>.
    /// </value>
    /// <remarks>
    /// The <see cref="YResolution"/> determines the vertical display size of the game,
    /// affecting how the game content is displayed on screen. It should be used together
    /// with <see cref="XResolution"/> to define the full resolution of the game window.
    /// </remarks>
    public int YResolution { get; set; } = 600;

    /// <summary>
    /// Gets or sets a value indicating whether the game runs in windowed mode.
    /// </summary>
    /// <value>
    /// A <see cref="bool"/> where <c>true</c> means the game will run in windowed mode,
    /// and <c>false</c> means the game will run in fullscreen mode.
    /// </value>
    /// <remarks>
    /// This property determines the display mode for the game. Windowed mode is useful
    /// for development, multitasking, or running the game in a constrained display environment.
    /// Depending on the graphics engine settings, switching modes might require restarting the game.
    /// </remarks>
    public bool Windowed { get; set; }

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
    public IList<string> BaseGamePaths { get; } = [];

    /// <summary>
    /// Gets or sets the path where the mod's files are located.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the directory path for mod-related files.
    /// This value may be <c>null</c> if no specific path is defined.
    /// </value>
    /// <remarks>
    /// The <see cref="ModFilesPath"/> is used to locate files associated with a mod.
    /// These files typically contain game data such as assets, configurations, and other
    /// resources specific to the mod. If not specified, the game engine will NOT load any mods intentionally.
    /// Mods may be loaded if they have the <c>.big</c> extension and are located within the found base game path, but
    /// this is NOT recommended.
    /// </remarks>
    public string? ModFilesPath { get; set; }

    /// <summary>
    /// Gets or sets the file extension used for identifying large mod files.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> that represents the file extension for mod big files,
    /// such as <c>".big"</c>. The value is optional and can be <c>null</c>.
    /// </value>
    /// <remarks>
    /// The <see cref="ModBigFilesExtension"/> property determines the specific file extension
    /// that the engine will recognize and use when loading large mod files. This setting
    /// facilitates customization by mod creators, enabling support for non-default file extensions.
    /// </remarks>
    public string? ModBigFilesExtension { get; set; }

    /// <summary>
    /// Gets or sets the file name of the splash screen bitmap for the mod.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the name of the bitmap file used as the splash screen
    /// when the mod is launched. The default value is <c>null</c>, indicating no splash screen is specified and
    /// the default game splash screen will be used instead.
    /// </value>
    /// <remarks>
    /// The <see cref="ModSplashScreenBmpFileName"/> determines the visual branding displayed
    /// during the initialization of a mod. The file should be a valid bitmap (.bmp) image and
    /// resides in the mod's directory.
    /// </remarks>
    public string? ModSplashScreenBmpFileName { get; set; }
}
