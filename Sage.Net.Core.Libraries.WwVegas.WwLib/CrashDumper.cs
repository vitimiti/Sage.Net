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
    private static int _keepNewest;
    private static long _maxTotalBytes;

    /// <summary>Installs the crash dump handler, configuring the application to generate crash dump files in case of unhandled exceptions or unobserved task exceptions.</summary>
    /// <param name="dumpDirectory">The directory where the crash dump files will be stored. The directory must be non-empty and writable.</param>
    /// <param name="dumpType">The type of the crash dump files to be created. Defaults to <see cref="DumpType.WithHeap"/>.</param>
    /// <param name="keepNewest">The maximum number of crash dump files to retain. Defaults to 10. Must be at least 1.</param>
    /// <param name="maxTotalBytes">The maximum total size, in bytes, of all retained crash dump files. Defaults to 2 GB. Must be non-negative.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dumpDirectory"/> is null, empty, or contains only white space.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="keepNewest"/> is less than 1 or <paramref name="maxTotalBytes"/> is negative.</exception>
    public static void Install(
        string dumpDirectory,
        DumpType dumpType = DumpType.WithHeap,
        int keepNewest = 10,
        long maxTotalBytes = 2L * 1024 * 1024 * 1024
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(keepNewest, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(maxTotalBytes);
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
        _maxTotalBytes = maxTotalBytes;

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
    private static long SafeLength(FileInfo f)
    {
        try
        {
            return f.Length;
        }
        catch
        {
            return 0;
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid exceptions escaping from crash paths."
    )]
    private static bool TryDelete(FileInfo f)
    {
        try
        {
            f.Delete();
            return true;
        }
        catch
        {
            // Avoid exceptions escaping from crash paths.
            return false;
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

            var di = new DirectoryInfo(_dumpDirectory);

            // Newest first
            var dumps = di.EnumerateFiles("*.dmp", SearchOption.TopDirectoryOnly)
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .ToList();

            // 1) Enforce count cap first
            for (var i = _keepNewest; i < dumps.Count; i++)
            {
                _ = TryDelete(dumps[i]);
            }

            // Refresh list after deletes
            dumps =
            [
                .. di.EnumerateFiles("*.dmp", SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTimeUtc),
            ];

            // 2) Enforce size cap by deleting oldest until under cap
            var total = dumps.Sum(SafeLength);

            if (_maxTotalBytes == 0)
            {
                // Special case: keep nothing if size cap is zero (but still "best effort").
                foreach (FileInfo f in dumps)
                {
                    _ = TryDelete(f);
                }

                return;
            }

            for (var i = dumps.Count - 1; i >= 0 && total > _maxTotalBytes; i--)
            {
                var len = SafeLength(dumps[i]);
                if (TryDelete(dumps[i]))
                {
                    total -= len;
                }
            }
        }
        catch
        {
            // Avoid exceptions escaping from crash paths.
        }
    }
}
