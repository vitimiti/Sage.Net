// -----------------------------------------------------------------------
// <copyright file="GameEngine.cs" company="Sage.Net">
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

using Sage.Net.Core.GameEngine.Common.Ini;
using Sage.Net.Core.GameEngine.Common.Transfer;

namespace Sage.Net.Core.GameEngine.Common.Subsystems;

/// <summary>
/// The game engine subsystem.
/// </summary>
public class GameEngine : SubsystemBase
{
    private readonly EngineOptions _options = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEngine"/> class.
    /// </summary>
    /// <param name="options">The engine options.</param>
    public GameEngine(Action<EngineOptions>? options) => options?.Invoke(_options);

    /// <summary>
    /// Gets the list of subsystems.
    /// </summary>
    public static SubsystemList? TheSubsystemList { get; private set; }

    /// <summary>
    /// Gets the archive file system.
    /// </summary>
    public static ArchiveFileSystem? TheArchiveFileSystem { get; private set; }

    /// <summary>
    /// Gets the audio manager.
    /// </summary>
    public static AudioManager? TheAudio { get; private set; }

    /// <summary>
    /// Initializes the game engine.
    /// </summary>
    /// <exception cref="NotImplementedException">This is not implemented yet.</exception>
    public override void Initialize()
    {
        TheSubsystemList = new SubsystemList();
        TheSubsystemList.AddSubsystem(this);

        var patchPath = Path.Combine(
            _options.CustomGamePath ?? Environment.CurrentDirectory,
            "Data",
            "INI",
            "INIZH.big"
        );

        // Only "Run\INI\Data\INIZH.big" should exist. Remove repeats.
        if (File.Exists(patchPath))
        {
            File.Delete(patchPath);
        }

        TheArchiveFileSystem = new ArchiveFileSystem(_options);
        InitSubsystem(TheArchiveFileSystem, "TheArchiveFileSystem", null);

        TheArchiveFileSystem.LoadMods();

        TheAudio = new AudioManager();
        InitSubsystem(TheAudio, "TheAudio", null);

        throw new NotImplementedException();
    }

    /// <summary>
    /// Resets the game engine.
    /// </summary>
    /// <exception cref="NotImplementedException">This is not implemented yet.</exception>
    public override void Reset() => throw new NotImplementedException();

    /// <summary>
    /// Updates the game engine.
    /// </summary>
    /// <exception cref="NotImplementedException">This is not imlemented yet.</exception>
    public override void UpdateBase() => throw new NotImplementedException();

    private static void InitSubsystem(
        SubsystemBase subsystem,
        string name,
        Xfer? xfer,
        string? path1 = null,
        string? path2 = null
    ) => TheSubsystemList!.InitializeSubsystem(subsystem, path1, path2, xfer, name);
}
