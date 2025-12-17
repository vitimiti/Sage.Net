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

using System.Globalization;
using System.Text;
#if !DUMP_PERF_STATS
using System.Diagnostics.CodeAnalysis;
#endif

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>A list of subsystems.</summary>
public class SubsystemList : IDisposable
{
    private readonly List<SubsystemBase> _subsystems = [];
#if DUMP_PERF_STATS
    private readonly List<SubsystemBase> _allSubsystems = [];
#endif

    private bool _disposed;

    /// <summary>Gets or sets the singleton instance of the subsystem list.</summary>
    public static SubsystemList? TheSubsystemList { get; set; }

    /// <summary>Adds a subsystem to the list of subsystems. This method is conditional and only available when performance statistics dumping is enabled.</summary>
    /// <param name="subsystem">The <see cref="SubsystemBase"/> instance to add to the subsystem list.</param>
#if !DUMP_PERF_STATS
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "This access internal data when DUMP_PERF_STATS is enabled."
    )]
#endif
    public void AddSubsystem(SubsystemBase subsystem)
    {
#if DUMP_PERF_STATS
        _allSubsystems.Add(subsystem);
#endif
    }

    /// <summary>Removes a subsystem from the list of subsystems. This method is conditional and only available when performance statistics dumping is enabled.</summary>
    /// <param name="subsystem">The <see cref="SubsystemBase"/> instance to remove from the subsystem list.</param>
#if !DUMP_PERF_STATS
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "This access internal data when DUMP_PERF_STATS is enabled."
    )]
#endif
    public void RemoveSubsystem(SubsystemBase subsystem)
    {
#if DUMP_PERF_STATS
        ArgumentNullException.ThrowIfNull(subsystem);

        subsystem.Dispose();
        _ = _allSubsystems.Remove(subsystem);
#endif
    }

    /// <summary>Initializes a subsystem by configuring its properties, loading configuration files, and adding it to the subsystem list.</summary>
    /// <param name="subsystem">The <see cref="SubsystemBase"/> instance to initialize and add to the subsystem list.</param>
    /// <param name="path1">The optional file path to the first configuration file to load. Can be <see langword="null"/>.</param>
    /// <param name="path2">The optional file path to the second configuration file to load. Can be <see langword="null"/>.</param>
    /// <param name="transfer">An optional <see cref="TransferOperation"/> instance used when loading configuration files. Can be <see langword="null"/>.</param>
    /// <param name="name">The name to assign to the subsystem. Must not be <see langword="null"/>.</param>
    public void InitializeSubsystem(
        SubsystemBase subsystem,
        string? path1,
        string? path2,
        TransferOperation? transfer,
        string name
    )
    {
        ArgumentNullException.ThrowIfNull(subsystem);

        subsystem.Name = name;
        subsystem.Initialize();

        Ini ini = new();
        if (path1 is not null)
        {
            _ = ini.LoadFileDirectory(path1, IniLoadType.Overwrite, transfer);
        }

        if (path2 is not null)
        {
            _ = ini.LoadFileDirectory(path2, IniLoadType.Overwrite, transfer);
        }

        _subsystems.Add(subsystem);
    }

    /// <summary>Executes the post-processing logic for all subsystems in the subsystem list.</summary>
    /// <remarks>This method iterates through each <see cref="SubsystemBase"/> instance in the internal list and invokes its <see cref="SubsystemBase.PostProcessLoad"/> method.</remarks>
    public void PostProcessLoadAll()
    {
        foreach (SubsystemBase subsystem in _subsystems)
        {
            subsystem.PostProcessLoad();
        }
    }

    /// <summary>Resets all subsystems in the list by invoking their respective Reset methods.</summary>
    /// <remarks>This method iterates through each <see cref="SubsystemBase"/> instance in the internal list and invokes its <see cref="SubsystemBase.Reset"/> method.</remarks>
    public void ResetAll()
    {
        foreach (SubsystemBase subsystem in _subsystems)
        {
            subsystem.Reset();
        }
    }

    /// <summary>Shuts down all subsystems in the list by invoking their respective Dispose methods.</summary>
    /// <remarks>This method iterates through each <see cref="SubsystemBase"/> instance in the internal list and invokes its <see cref="SubsystemBase.Dispose()"/> method. It then clears the internal subsystems list.</remarks>
    public void ShutdownAll()
    {
        foreach (SubsystemBase subsystem in _subsystems)
        {
            subsystem.Dispose();
        }

        _subsystems.Clear();
    }

#if DUMP_PERF_STATS
    /// <summary>Dumps the total time consumed by all subsystems to the console.</summary>
    /// <returns>A string containing the total time consumed by all subsystems.</returns>
    /// <remarks>This method iterates through each <see cref="SubsystemBase"/> instance in the internal list and dumps its total time to a <see cref="string"/>.</remarks>
    public string DumpTimesForAll()
    {
        StringBuilder sb = new();
        _ = sb.AppendLine("ALL SUBSYSTEMS:\n");
        var misc = 0F;
        var total = 0F;
        SubsystemBase.ClearTotalTime();
        foreach (SubsystemBase subsystem in _allSubsystems)
        {
            total += subsystem.UpdateTime;
            if (subsystem.UpdateTime > .00001F)
            {
                _ = sb.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"\tTime {TimeSpan.FromMilliseconds(subsystem.UpdateTime * 1_000F):g} Update() {subsystem.Name}\n"
                );
            }
            else
            {
                misc += subsystem.UpdateTime;
            }

            total += subsystem.DrawTime;
            if (subsystem.DrawTime > .00001F)
            {
                _ = sb.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"\tTime {TimeSpan.FromMilliseconds(subsystem.DrawTime * 1_000F):g} Draw() {subsystem.Name}\n"
                );
            }
            else
            {
                misc += subsystem.DrawTime;
            }
        }

        _ = sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"TOTAL {TimeSpan.FromMilliseconds(total * 1_000F):g}, Misc time {TimeSpan.FromMilliseconds(misc * 1_000F):g}\n"
        );

        return sb.ToString();
    }
#endif

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Disposes the subsystem list.</summary>
    /// <param name="disposing">Indicates whether the method is called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            ShutdownAll();
        }

        _disposed = true;
    }
}
