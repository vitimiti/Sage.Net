// -----------------------------------------------------------------------
// <copyright file="CrashDumper.cs" company="Sage.Net">
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
using Microsoft.Diagnostics.NETCore.Client;

namespace Sage.Net.Core.Libraries.WwVegas.WwLib;

/// <summary>Provides functionality for managing crash dumps in .NET applications.</summary>
public static class CrashDumper
{
    private static int _installed; // 0/1
    private static int _dumpAttempted; // 0/1

    private static string _dumpDirectory = string.Empty;
    private static DumpType _dumpType = DumpType.WithHeap;
    private static int _keepNewest = 10;

    /// <summary>Installs a crash dump handler for the application, enabling the creation of diagnostic dumps during unhandled exceptions or unobserved task exceptions.</summary>
    /// <param name="dumpDirectory">The directory where crash dump files will be stored. This must be a non-empty string pointing to a valid file system path.</param>
    /// <param name="dumpType">Specifies the type of crash dump to create. Defaults to <see cref="DumpType.WithHeap"/>.</param>
    /// <param name="keepNewest">The maximum number of most recent crash dump files to retain in the specified directory. This must be a positive integer greater than or equal to 1. Older files exceeding this number will be automatically deleted.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="dumpDirectory"/> is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="keepNewest"/> is less than 1.</exception>
    public static void Install(string dumpDirectory, DumpType dumpType = DumpType.WithHeap, int keepNewest = 10)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(keepNewest, 1);

        if (string.IsNullOrWhiteSpace(dumpDirectory))
        {
            throw new ArgumentException("Dump directory must be non-empty.", nameof(dumpDirectory));
        }

        if (Interlocked.Exchange(ref _installed, 1) != 0)
        {
            return; // already installed
        }

        _dumpDirectory = dumpDirectory;
        _dumpType = dumpType;
        _keepNewest = keepNewest;

        _ = Directory.CreateDirectory(_dumpDirectory);

        TryEnforceRetention();
        AppDomain.CurrentDomain.UnhandledException += (_, _) => TryWriteDumpOnce("unhandled");
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            TryWriteDumpOnce("unobserved-task");
            e.SetObserved();
        };
    }

    private static string Sanitize(string s) =>
        Path.GetInvalidFileNameChars().Aggregate(s, (current, c) => current.Replace(c, '_'));

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid exceptions escaping from crash paths."
    )]
    private static void TryWriteDumpOnce(string reason)
    {
        if (Interlocked.Exchange(ref _dumpAttempted, 1) != 0)
        {
            return;
        }

        try
        {
            var dumpPath = Path.Combine(
                _dumpDirectory,
                $"crash_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Environment.ProcessId}_{Sanitize(reason)}.dmp"
            );

            var client = new DiagnosticsClient(Environment.ProcessId);
            client.WriteDump(_dumpType, dumpPath, logDumpGeneration: false);
        }
        catch
        {
            // Avoid exceptions escaping from crash paths.
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid exceptions escaping from crash paths."
    )]
    private static void TryEnforceRetention()
    {
        try
        {
            if (!Directory.Exists(_dumpDirectory))
            {
                return;
            }

            // Enumerate only dumps; if we later add metadata files, they won't be touched.
            var files = new DirectoryInfo(_dumpDirectory)
                .EnumerateFiles("*.dmp", SearchOption.TopDirectoryOnly)
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .ToList();

            for (var i = _keepNewest; i < files.Count; i++)
            {
                try
                {
                    files[i].Delete();
                }
                catch
                {
                    // Avoid exceptions escaping from crash paths.
                }
            }
        }
        catch
        {
            // Avoid exceptions escaping from crash paths.
        }
    }
}
