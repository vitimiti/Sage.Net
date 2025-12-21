// -----------------------------------------------------------------------
// <copyright file="ISplashScreen.cs" company="Sage.Net">
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
/// Represents a splash screen interface for initializing and updating the splash screen
/// during the startup of a game using the Sage.Net framework.
/// </summary>
/// <remarks>
/// The implementation of this interface is responsible for managing the splash screen
/// lifecycle, including setting it up with the provided game options and updating
/// its state as necessary during the initialization process.
/// </remarks>
public interface ISplashScreen : IDisposable
{
    /// <summary>
    /// Gets or sets a value indicating whether the initialization process of the splash screen
    /// has been completed successfully.
    /// </summary>
    /// <remarks>
    /// This property is used to determine if the splash screen has finished all necessary
    /// setup and is ready for subsequent processes. The underlying implementation
    /// ensures that the property reflects the final state of the initialization sequence.
    /// </remarks>
    bool InitializationIsComplete { get; set; }

    /// <summary>
    /// Initializes the splash screen with the specified game options.
    /// </summary>
    /// <param name="baseGamePath">The path to the base game files.</param>
    /// <param name="options">
    /// The game options used to configure the splash screen during initialization.
    /// These options specify settings such as the game identifier, window title,
    /// and file paths for base game assets and modifications.
    /// </param>
    void Initialize(string baseGamePath, GameOptions options);

    /// <summary>
    /// Updates the current state of the splash screen during the initialization process.
    /// </summary>
    /// <remarks>
    /// This method is responsible for refreshing or modifying the splash screen's visual
    /// or operational state as necessary to reflect progress or changes during startup.
    /// It may handle tasks such as updating progress indicators, changing text, or triggering
    /// animations specific to the splash screen.
    /// </remarks>
    void Update();

    /// <summary>
    /// Renders the splash screen with its current state.
    /// </summary>
    /// <remarks>
    /// This method is responsible for drawing the splash screen's visual elements,
    /// such as background images, progress bars, or logos, to the screen.
    /// It should be called periodically to ensure the splash screen is displayed
    /// accurately as updates occur during the initialization process.
    /// </remarks>
    void Draw();
}
