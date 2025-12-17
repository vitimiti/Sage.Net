// -----------------------------------------------------------------------
// <copyright file="SdlGameEngine.cs" company="Sage.Net">
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

using Microsoft.Extensions.Logging;

namespace Sage.Net.Generals.GameEngine.GameEngineDevice.SdlDevice;

/// <summary>Provides an SDL-based implementation of the game engine for managing game state and logic specific to SDL (Simple DirectMedia Layer) subsystems. Inherits from the base <see cref="GameEngine"/> class, incorporating SDL-specific functionality. This class serves as a specialization of the core <see cref="GameEngine"/> tailored for games or applications leveraging SDL for rendering, input handling, and other services.</summary>
/// <param name="logger">The logger instance used for logging runtime information, warnings, and errors.</param>
/// <remarks>The <c>SdlGameEngine</c> class extends the primary game engine functionality to include SDL platform-specific implementations, ensuring seamless integration of SDL subsystems within the broader game engine architecture. The responsibility of this engine includes initializing SDL-related components, managing their lifecycle, and coordinating their interaction with the core game engine systems.</remarks>
public class SdlGameEngine(ILogger logger) : Common.GameEngine(logger) { }
