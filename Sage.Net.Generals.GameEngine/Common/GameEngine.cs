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

using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Sage.Net.Core.GameEngine.Common;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Represents the core game engine for managing various subsystems and game logic.</summary>
/// <param name="logger">The logger to use.</param>
/// <remarks>The GameEngine is an abstract class inheriting from <see cref="SubsystemBase"/>. It serves as the central component of the game's architecture, managing subsystem initialization, updates, and lifecycle events. This class is responsible for high-level integration and coordination of all subsystems.</remarks>
public abstract partial class GameEngine(ILogger logger) : SubsystemBase
{
    /// <summary>Gets or sets the singleton instance of the game engine.</summary>
    public static GameEngine? TheGameEngine { get; set; }

    /// <summary>Initializes the game engine.</summary>
    public override void Initialize()
    {
        Ini ini = new();

        if (VersionHelper.TheVersion is not null)
        {
            StringBuilder sb = new();
            var versionMessage = sb.AppendLine(
                    "================================================================================"
                )
                .AppendLine(CultureInfo.InvariantCulture, $"Generals version {VersionHelper.TheVersion.StringVersion}")
                .AppendLine(CultureInfo.InvariantCulture, $"Build date: {VersionHelper.TheVersion.StringBuildTime}")
                .AppendLine(
                    CultureInfo.InvariantCulture,
                    $"Build location: {VersionHelper.TheVersion.StringBuildLocation}"
                )
                .AppendLine(CultureInfo.InvariantCulture, $"Build user: {VersionHelper.TheVersion.StringBuildUser}")
                .AppendLine(
                    CultureInfo.InvariantCulture,
                    $"Build git revision: {VersionHelper.TheVersion.StringGitCommitCount}"
                )
                .AppendLine(
                    CultureInfo.InvariantCulture,
                    $"Build git version: {VersionHelper.TheVersion.StringGitTagOrHash}"
                )
                .AppendLine(
                    CultureInfo.InvariantCulture,
                    $"Build git commit time: {VersionHelper.TheVersion.StringGitCommitTime}"
                )
                .AppendLine(
                    CultureInfo.InvariantCulture,
                    $"Build git commit author: {VersionHelper.TheVersion.StringBuildUserOrGitCommitAuthorName}"
                )
                .AppendLine("================================================================================")
                .ToString();

            Log.VersionData(logger, versionMessage);
        }

        SubsystemList.TheSubsystemList = new SubsystemList();
        SubsystemList.TheSubsystemList.AddSubsystem(this);

        RandomValue.InitializeRandom();

        NameKeyGenerator.TheNameKeyGenerator = new NameKeyGenerator(logger);
        NameKeyGenerator.TheNameKeyGenerator.Initialize();

        TransferCrc transferCrc = new();
        transferCrc.Open("lightCRC");

        Debug.Assert(
            GlobalData.TheWritableGlobalData is not null,
            $"{nameof(GlobalData.TheWritableGlobalData)} expected to be created."
        );

        InitializeSubsystem(GlobalData.TheWritableGlobalData, "TheWritableGlobalData", null);
    }

    /// <summary>Resets the game engine.</summary>
    public override void Reset() => throw new NotImplementedException();

    /// <summary>Performs the actual game engine update.</summary>
    public override void UpdateCore() => throw new NotImplementedException();

    /// <inheritdoc/>
    /// <remarks>Disposes the game engine.</remarks>
    protected override void Dispose(bool disposing)
    {
        SubsystemList.TheSubsystemList!.ShutdownAll();
        SubsystemList.TheSubsystemList.Dispose();
        SubsystemList.TheSubsystemList = null;

        base.Dispose(disposing);
    }

    private static void InitializeSubsystem(
        SubsystemBase system,
        string name,
        TransferOperation? transfer,
        string? path1 = null,
        string? path2 = null
    ) => SubsystemList.TheSubsystemList!.InitializeSubsystem(system, path1, path2, transfer, name);

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Debug, "{VersionMessage}")]
        public static partial void VersionData(ILogger logger, string versionMessage);
    }
}
