// -----------------------------------------------------------------------
// <copyright file="IGameWindow.cs" company="Sage.Net">
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
/// Defines the contract for a game window in the application.
/// </summary>
/// <remarks>
/// A game window serves as the primary rendering surface and provides lifecycle
/// management methods for initialization, updating state, and rendering.
/// </remarks>
public interface IGameWindow : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether a request to quit the game window has been made.
    /// </summary>
    /// <remarks>
    /// This property is typically used during the game loop to determine whether
    /// the application should cease execution and exit. The value is set internally
    /// by the implementation when a quit event, such as a user-initiated action or
    /// system request, is detected.
    /// </remarks>
    bool QuitRequested { get; }

    /// <summary>
    /// Initializes the game window and its associated resources.
    /// </summary>
    /// <remarks>
    /// This method performs the necessary setup for the game window, which includes
    /// creating the graphics device, configuring the rendering environment,
    /// and preparing the window for display. It must be called before rendering
    /// or updating the game window.
    /// </remarks>
    void Initialize();

    /// <summary>
    /// Updates the state of the game window and processes input or system events.
    /// </summary>
    /// <remarks>
    /// This method is responsible for handling the internal state transitions,
    /// system event processing, and other tasks necessary to update the game window
    /// and its associated resources. It should be called in the application's main loop
    /// to ensure the game window remains responsive and up-to-date.
    /// </remarks>
    void Update();

    /// <summary>
    /// Renders the current frame to the game window's display surface.
    /// </summary>
    /// <remarks>
    /// This method performs all rendering operations required to display the current
    /// frame, including initiating a render pass, submitting drawing commands, and
    /// presenting the final output to the screen. It ensures that all necessary
    /// resources are utilized correctly during the rendering process.
    /// </remarks>
    void Draw();
}
