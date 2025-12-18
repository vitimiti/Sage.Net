// -----------------------------------------------------------------------
// <copyright file="IDumpService.cs" company="Sage.Net">
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

namespace Sage.Net.Diagnostics;

/// <summary>
/// Defines a service responsible for creating diagnostic process dumps for the current process.
/// </summary>
/// <remarks>
/// Implementations should be resilient: <see cref="WriteDump(string)"/> is expected to handle failures
/// internally (for example, by logging) and avoid throwing to prevent cascading faults during error handling.
/// </remarks>
public interface IDumpService
{
    /// <summary>
    /// Gets the full path to the directory where dump files are written.
    /// </summary>
    string DumpDirectoryPath { get; }

    /// <summary>
    /// Creates a diagnostic process dump for the current process using the configured
    /// <see cref="DumpOptions"/>. The dump file name incorporates the specified <paramref name="reason"/>.
    /// </summary>
    /// <param name="reason">A short, file-system friendly description of why the dump is being taken.
    /// This text is used as part of the dump file name. For example: <c>"UnhandledException"</c> or <c>"Watchdog"</c>.</param>
    /// <remarks>
    /// Implementations should be resilient and must not throw if dump creation fails. Instead, they should
    /// handle failures internally (for example, by logging) to avoid cascading errors during fault handling.
    /// </remarks>
    void WriteDump(string reason);
}
