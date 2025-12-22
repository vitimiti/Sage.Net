// -----------------------------------------------------------------------
// <copyright file="IScene.cs" company="Sage.Net">
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

namespace Sage.Net.Game.Scenes;

/// <summary>
/// Represents a scene in the game, providing core functionality for initialization,
/// updating, rendering, and transitioning to the next scene.
/// </summary>
internal interface IScene : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the current scene has requested to exit the application.
    /// </summary>
    /// <remarks>
    /// When this property is true, the game loop will terminate after processing the current frame.
    /// </remarks>
    bool QuitRequested { get; }

    /// <summary>
    /// Gets the next scene to transition to after the current scene completes its operations.
    /// </summary>
    /// <remarks>
    /// This property returns an instance of the next scene if a transition is ready to occur,
    /// or null if the current scene has not yet determined the next scene or the transition
    /// conditions have not been met.
    /// </remarks>
    IScene? NextScene { get; }

    /// <summary>
    /// Performs initialization for the current scene.
    /// </summary>
    /// <remarks>
    /// This method is responsible for setting up the scene's resources,
    /// preparing it for operation, and ensuring it is ready to handle updates and rendering.
    /// </remarks>
    void Initialize();

    /// <summary>
    /// Updates the state of the scene based on the elapsed time since the last update.
    /// </summary>
    /// <param name="deltaTime">The time, in seconds, that has passed since the last call to this method.</param>
    /// <remarks>
    /// This method is responsible for handling time-dependent updates, such as animations,
    /// state transitions, and logic execution. It ensures that the scene's state evolves
    /// appropriately with each frame or simulation step.
    /// </remarks>
    void Update(double deltaTime);

    /// <summary>
    /// Renders the current state of the scene.
    /// </summary>
    /// <remarks>
    /// This method is responsible for drawing all visual elements associated with the scene.
    /// It delegates the rendering logic to the underlying graphics system or display components.
    /// </remarks>
    void Draw();
}
