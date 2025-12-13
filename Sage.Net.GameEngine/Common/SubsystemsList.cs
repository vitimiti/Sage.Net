// -----------------------------------------------------------------------
// <copyright file="SubsystemsList.cs" company="Sage.Net">
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
using System.Diagnostics.CodeAnalysis;
#if DUMP_PERF_STATS
using System.Globalization;
using System.Text;
#endif

namespace Sage.Net.GameEngine.Common;

/// <summary>A class that manages all subsystems.</summary>
public sealed class SubsystemsList : IDisposable
{
    private readonly List<SubsystemBase> _subsystems = [];

#if !DUMP_PERF_STATS
    [SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "Used for perf stats.")]
    [SuppressMessage("Style", "IDE0052:Remove unread private members", Justification = "Used for perf stats.")]
    [SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration", Justification = "Used for perf stats.")]
#endif
    private readonly List<SubsystemBase> _allSubsystems = [];

    private bool _disposed;

    /// <summary>Gets or sets the singleton instance of the <see cref="SubsystemsList"/>.</summary>
    public static SubsystemsList? TheSubsystemsList { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Debug.Assert(_subsystems.Count == 0, "Subsystems were removed properly.");
        ShutdownAll();
        _disposed = true;
    }

    /// <summary>Adds a subsystem to the internal subsystems list.</summary>
    /// <param name="subsystem">The <see cref="SubsystemBase"/> to add.</param>
    /// <remarks>This will only act if <c>DUMP_PERF_STATS</c> is enabled.</remarks>
    /// <seealso cref="RemoveSubsystem(SubsystemBase)"/>
    /// <seealso cref="InitializeSubsystem(SubsystemBase, string)"/>
#if !DUMP_PERF_STATS
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used for perf stats.")]
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "Used for perf stats.")]
#endif
    public void AddSubsystem(SubsystemBase subsystem)
    {
#if DUMP_PERF_STATS
        _allSubsystems.Add(subsystem);
#endif
    }

    /// <summary>Removes a subsystem from the internal subsystems list.</summary>
    /// <param name="subsystem">The <see cref="SubsystemBase"/> to remove.</param>
    /// <remarks>This will only act if <c>DUMP_PERF_STATS</c> is enabled.</remarks>
    /// <seealso cref="AddSubsystem(SubsystemBase)"/>
    /// <seealso cref="InitializeSubsystem(SubsystemBase, string)"/>
#if !DUMP_PERF_STATS
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used for perf stats.")]
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "Used for perf stats.")]
#endif
    public void RemoveSubsystem(SubsystemBase subsystem)
    {
#if DUMP_PERF_STATS
        _ = _allSubsystems.Remove(subsystem);
#endif
    }

    /// <summary>Initializes a subsystem.</summary>
    /// <param name="subsystem">The <see cref="SubsystemBase"/> to initialize.</param>
    /// <param name="path1">The first path to load INI files from; or <see langword="null"/> if no path is required.</param>
    /// <param name="path2">The second path to load INI files from; or <see langword="null"/> if no path is required.</param>
    /// <param name="transfer">The <see cref="TransferOperation"/> to transfer the loaded data with; or <see langword="null"/> if no transfer operation is required.</param>
    /// <param name="name">A <see cref="string"/> with the <paramref name="subsystem"/> name.</param>
    /// <remarks>This is the main subsystem initialization system and will act regardless of compilation options. Use this to add a <see cref="SubsystemBase"/> to the subsystems list.</remarks>
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

        using Ini ini = new();
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

    /// <summary>Calls the <see cref="SubsystemBase.PostProcessLoad"/> method on all initialized subsystems.</summary>
    /// <seealso cref="SubsystemBase.PostProcessLoad"/>
    /// <seealso cref="InitializeSubsystem(SubsystemBase, string)"/>
    public void PostProcessLoadAll() => _subsystems.ForEach(s => s.PostProcessLoad());

    /// <summary>Calls the <see cref="SubsystemBase.Reset"/> method on all initialized subsystems.</summary>
    /// <seealso cref="SubsystemBase.Reset"/>
    /// <seealso cref="InitializeSubsystem(SubsystemBase, string)"/>
    public void ResetAll() => _subsystems.ForEach(s => s.Reset());

    /// <summary>Calls the <see cref="SubsystemBase.Dispose()"/> method on all initialized subsystems and clears the subsystem list.</summary>
    /// <seealso cref="SubsystemBase.Dispose()"/>
    /// <seealso cref="InitializeSubsystem(SubsystemBase, string)"/>
    public void ShutdownAll()
    {
        _subsystems.ForEach(s => s.Dispose());
        _subsystems.Clear();
    }

#if DUMP_PERF_STATS
    /// <summary>Dumps the time spent in each subsystem +/- their update and draw times.</summary>
    /// <returns>A new <see cref="string"/> that can be logged with the time information.</returns>
    public string DumpTimesForAll()
    {
        StringBuilder sb = new();
        _ = sb.Append("ALL SUBSYSTEMS:\n");
        var misc = 0F;
        var total = 0F;
        SubsystemBase.ClearTotalTime();
        foreach (SubsystemBase subsystem in _allSubsystems)
        {
            total += subsystem.UpdateTime;
#if RTS_ZERO_HOUR
            if (subsystem.DoDumpUpdate)
#else
            if (subsystem.UpdateTime > .00001F)
#endif
            {
                _ = sb.Append(
                    CultureInfo.InvariantCulture,
                    $"\tTime {TimeSpan.FromMilliseconds(subsystem.UpdateTime * 1_000F):g} {nameof(SubsystemBase)}.{nameof(subsystem.Update)}() {subsystem.Name}\n"
                );
            }
            else
            {
                misc += subsystem.UpdateTime;
            }

            total += subsystem.DrawTime;
#if RTS_ZERO_HOUR
            if (subsystem.DoDumpDraw)
#else
            if (subsystem.DrawTime > .00001F)
#endif
            {
                _ = sb.Append(
                    CultureInfo.InvariantCulture,
                    $"\tTime {TimeSpan.FromMilliseconds(subsystem.DrawTime * 1_000F):g} {nameof(SubsystemBase)}.{nameof(subsystem.Update)}() {subsystem.Name}\n"
                );
            }
            else
            {
                misc += subsystem.DrawTime;
            }
        }

        _ = sb.Append(
            CultureInfo.InvariantCulture,
            $"TOTAL {TimeSpan.FromMilliseconds(total * 1_000F):g}, Misc time {TimeSpan.FromMilliseconds(misc * 1_000F):g}\n"
        );

        return sb.ToString();
    }
#endif
}
