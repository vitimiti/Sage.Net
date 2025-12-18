// -----------------------------------------------------------------------
// <copyright file="DumpOptions.cs" company="Sage.Net">
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

using Microsoft.Diagnostics.NETCore.Client;

namespace Sage.Net.Diagnostics;

/// <summary>
/// Represents configuration options that control generation, location, and retention
/// of diagnostic process dumps.
/// </summary>
/// <remarks>
/// These options are typically bound from application configuration, for example from the
/// configuration section <c>Sage:Diagnostics:Dumps</c>. They determine where dump files are stored,
/// how detailed the dumps are (see <see cref="DumpLevel"/>), how many are kept before older ones are
/// removed (see <see cref="MaxDumpFiles"/>), and the file name prefix used (see <see cref="FilePrefix"/>).
/// </remarks>
public class DumpOptions
{
    /// <summary>
    /// Gets or sets the directory name (relative to the application data folder) where dump files are written.
    /// </summary>
    /// <remarks>
    /// The effective path is resolved under the per-user application data directory, depending on the platform,
    /// and then combined with this value. The default is <c>"Dumps"</c>.
    /// </remarks>
    public string DumpDirectory { get; set; } = "Dumps";

    /// <summary>
    /// Gets or sets the level of detail to include in the generated process dump.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="DumpType.Normal"/>. Choose higher levels only when necessary, as they can produce
    /// significantly larger files.
    /// </remarks>
    public DumpType DumpLevel { get; set; } = DumpType.Normal;

    /// <summary>
    /// Gets or sets the maximum number of dump files to keep.
    /// </summary>
    /// <remarks>
    /// When the number of dump files exceeds this value, the oldest files are deleted automatically to maintain
    /// the limit. The default is <c>5</c> and must be greater than <c>0</c>.
    /// </remarks>
    public int MaxDumpFiles { get; set; } = 5;

    /// <summary>
    /// Gets or sets the file name prefix used for generated dumps.
    /// </summary>
    /// <remarks>
    /// The final file name uses the pattern <c>{FilePrefix}_{reason}_{timestamp}.dmp</c>. The default prefix is
    /// <c>"crash_dump"</c>.
    /// </remarks>
    public string FilePrefix { get; set; } = "crash_dump";
}
