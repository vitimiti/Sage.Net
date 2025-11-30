// -----------------------------------------------------------------------
// <copyright file="SubsystemList.cs" company="Sage.Net">
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

using System.Diagnostics.CodeAnalysis;
using Sage.Net.Core.GameEngine.Common.Ini;
using Sage.Net.Core.GameEngine.Common.Transfer;
#if DUMP_PERF_STATS
using System.Globalization;
using System.Text;
#endif

namespace Sage.Net.Core.GameEngine.Common.Subsystems;

/// <summary>
/// List of subsystems.
/// </summary>
public class SubsystemList
{
    private readonly List<SubsystemBase> _subsystems = [];

#if DUMP_PERF_STATS
    private readonly List<SubsystemBase> _allSubsystems = [];
#endif

    /// <summary>
    /// Adds a subsystem.
    /// </summary>
    /// <param name="subsystem">The subsystem inheriting <see cref="SubsystemBase"/> to add.</param>
    /// <remarks>This is a no-op unless <c>DUMP_PERF_STATS</c> is enabled.</remarks>
#if !DUMP_PERF_STATS
    [SuppressMessage(
        "Roslynator",
        "RCS1163:Unused parameter",
        Justification = "Used in builds with DUMP_PERF_STATS defined."
    )]
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Used in builds with DUMP_PERF_STATS defined."
    )]
#endif
    public void AddSubsystem(SubsystemBase subsystem)
    {
#if DUMP_PERF_STATS
        _allSubsystems.Add(subsystem);
#endif
    }

    /// <summary>
    /// Removes a subsystem.
    /// </summary>
    /// <param name="subsystem">The subsystem inheriting <see cref="SubsystemBase"/> to remove.</param>
    /// <remarks>This is a no-op unless <c>DUMP_PERF_STATS</c> is enabled.</remarks>
#if !DUMP_PERF_STATS
    [SuppressMessage(
        "Roslynator",
        "RCS1163:Unused parameter",
        Justification = "Used in builds with DUMP_PERF_STATS defined."
    )]
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Used in builds with DUMP_PERF_STATS defined."
    )]
#endif
    public void RemoveSubsystem(SubsystemBase subsystem)
    {
#if DUMP_PERF_STATS
        _ = _allSubsystems.Remove(subsystem);
#endif
    }

    /// <summary>
    /// Initializes the subsystem.
    /// </summary>
    /// <param name="subsystem">The subsystem that inherits from <see cref="SubsystemBase"/> to initialize.</param>
    /// <param name="path1">The first path to load INI files from.</param>
    /// <param name="path2">The second path to load INI files from.</param>
    /// <param name="xfer">The transfer class that inherits from <see cref="Xfer"/>.</param>
    /// <param name="name">The <paramref name="subsystem"/> name.</param>
    public void InitializeSubsystem(
        [NotNull] SubsystemBase subsystem,
        string? path1,
        string? path2,
        Xfer? xfer,
        string name
    )
    {
        subsystem.Name = name;
        subsystem.Initialize();

        IniReader ini = new();
        if (path1 is not null)
        {
            _ = ini.LoadFileDirectory(path1, IniLoadType.Overwrite, xfer);
        }

        if (path2 is not null)
        {
            _ = ini.LoadFileDirectory(path2, IniLoadType.Overwrite, xfer);
        }

        _subsystems.Add(subsystem);
    }

    /// <summary>
    /// Calls the post-load processing on all subsystems.
    /// </summary>
    public void PostProcessLoadAll()
    {
        foreach (SubsystemBase subsystem in _subsystems)
        {
            subsystem.PostProcessLoad();
        }
    }

    /// <summary>
    /// Calls the reset method on all subsystems.
    /// </summary>
    public void ResetAll()
    {
        foreach (SubsystemBase subsystem in _subsystems)
        {
            subsystem.Reset();
        }
    }

    /// <summary>
    /// Clears the list of subsystems.
    /// </summary>
    public void ShutdownAll() => _subsystems.Clear();

#if DUMP_PERF_STATS
    /// <summary>
    /// Dumps the times for all subsystems.
    /// </summary>
    /// <returns>A loggable <see cref="string"/> with the performance times information.</returns>
    public string? DumpTimesForAll()
    {
        StringBuilder sb = new();
        _ = sb.Append("ALL SUBSYSTEMS:\n");

        SubsystemBase.ClearTotalTime();
        TimeSpan total = TimeSpan.Zero;
        TimeSpan misc = TimeSpan.Zero;
        foreach (SubsystemBase subsystem in _allSubsystems)
        {
            total += subsystem.UpdateTime;
            if (subsystem.DoDumpUpdate)
            {
                _ = sb.AppendFormat(
                    CultureInfo.InvariantCulture,
                    $"\tTime {subsystem.UpdateTime:g} Update() {subsystem.Name}\n"
                );
            }
            else
            {
                misc += subsystem.UpdateTime;
            }

            total += subsystem.DrawTime;
            if (subsystem.DoDumpDraw)
            {
                _ = sb.AppendFormat(
                    CultureInfo.InvariantCulture,
                    $"\tTime {subsystem.DrawTime:g} Draw() {subsystem.Name}\n"
                );
            }
            else
            {
                misc += subsystem.DrawTime;
            }
        }

        _ = sb.AppendFormat(CultureInfo.InvariantCulture, $"TOTAL {total:g}, Misc time {misc:g}");
        return sb.ToString();
    }
#endif
}
