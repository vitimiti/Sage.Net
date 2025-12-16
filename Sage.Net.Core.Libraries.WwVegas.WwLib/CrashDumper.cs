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
using System.Text.Json;
using Microsoft.Diagnostics.NETCore.Client;

namespace Sage.Net.Core.Libraries.WwVegas.WwLib;

/// <summary>Provides functionality for managing crash dumps in .NET applications.</summary>
public static class CrashDumper
{
    private const string MarkerFileName = "last_crash.json";

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static int _installed; // 0/1
    private static int _dumpAttempted; // 0/1

    private static string _dumpDirectory = string.Empty;
    private static DumpType _dumpType;
    private static int _keepNewest;
    private static long _maxTotalBytes;
    private static string _appId = "App";

    /// <summary>Installs the crash dumping mechanism to enable the creation and management of crash dumps in the event of unhandled exceptions or unobserved task exceptions.</summary>
    /// <param name="dumpDirectory">The directory where crash dump files will be stored. Must be a non-empty string.</param>
    /// <param name="dumpType">The type of crash dump to generate. Defaults to <see cref="DumpType.WithHeap"/>.</param>
    /// <param name="keepNewest">The maximum number of most recent dump files to retain. Must be greater than or equal to 1. Defaults to 10.</param>
    /// <param name="maxTotalBytes">The maximum total size of all retained dump files in bytes. Must be a non-negative value. Defaults to 2GB.</param>
    /// <param name="appId">An application-specific identifier to prefix the metadata of the crash dumps. Defaults to "App".</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dumpDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="keepNewest"/> is less than 1 or <paramref name="maxTotalBytes"/> is negative.</exception>
    public static void Install(
        string dumpDirectory,
        DumpType dumpType = DumpType.WithHeap,
        int keepNewest = 10,
        long maxTotalBytes = 2L * 1024 * 1024 * 1024,
        string appId = "App"
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
        _appId = string.IsNullOrWhiteSpace(appId) ? "App" : appId;

        _ = Directory.CreateDirectory(_dumpDirectory);

        TryEnforceRetention();
        AppDomain.CurrentDomain.UnhandledException += (_, _) => TryWriteDumpOnce("unhandled");
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            TryWriteDumpOnce("unobserved-task");
            e.SetObserved();
        };
    }

    /// <summary>Attempts to read the metadata of the last recorded crash from a predefined marker file.</summary>
    /// <param name="marker">When this method returns, contains the deserialized <see cref="CrashMarker"/> object representing the last recorded crash, or <see langword="null"/> if no valid marker was found or an error occurred during reading.</param>
    /// <returns><see langword="true"/> if the last crash metadata is successfully read and deserialized; <see langword="false"/> otherwise.</returns>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid exceptions escaping from crash paths."
    )]
    public static bool TryReadLastCrash(out CrashMarker? marker)
    {
        marker = null;
        try
        {
            var path = Path.Combine(_dumpDirectory, MarkerFileName);
            if (!File.Exists(path))
            {
                return false;
            }

            var json = File.ReadAllText(path);
            marker = JsonSerializer.Deserialize<CrashMarker>(json);
            return marker != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Clears the file that records metadata about the last crash, if it exists in the configured dump directory.</summary>
    /// <remarks>This method removes the "last_crash.json" file from the dump directory if it exists. It silently ignores errors to prevent exceptions from propagating during critical crash handling scenarios.</remarks>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid exceptions escaping from crash paths."
    )]
    public static void ClearLastCrashMarker()
    {
        try
        {
            var path = Path.Combine(_dumpDirectory, MarkerFileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Avoid exceptions escaping from crash paths.
        }
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

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        var pid = Environment.ProcessId;

        try
        {
            var dumpFileName = $"crash_{nowUtc:yyyyMMdd_HHmmss}_{pid}_{Sanitize(reason)}.dmp";

            var dumpPath = Path.Combine(_dumpDirectory, dumpFileName);

            var client = new DiagnosticsClient(pid);
            client.WriteDump(_dumpType, dumpPath, logDumpGeneration: false);

            // Write marker after dump succeeds (so marker implies a dump exists).
            TryWriteMarker(
                new CrashMarker
                {
                    AppId = _appId,
                    TimestampUtc = nowUtc,
                    ProcessId = pid,
                    Reason = reason,
                    DumpFileName = dumpFileName,
                }
            );

            TryEnforceRetention();
        }
        catch
        {
            TryWriteMarker(
                new CrashMarker
                {
                    AppId = _appId,
                    TimestampUtc = nowUtc,
                    ProcessId = pid,
                    Reason = reason,
                    DumpFileName = null,
                }
            );
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid exceptions escaping from crash paths."
    )]
    private static void TryWriteMarker(CrashMarker marker)
    {
        try
        {
            var path = Path.Combine(_dumpDirectory, MarkerFileName);
            var json = JsonSerializer.Serialize(marker, JsonOptions);

            // Best effort atomic-ish write: write temp then replace.
            var tmp = $"{path}.tmp";
            File.WriteAllText(tmp, json);

            try
            {
                // Replace is atomic on Windows; on Unix it replaces the target path.
                File.Move(tmp, path, overwrite: true);
            }
            catch
            {
                // Fallback if replace/move fails
                File.Copy(tmp, path, overwrite: true);
                File.Delete(tmp);
            }
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

            if (dumps.Count == 0)
            {
                return;
            }

            // --- 1) Enforce count cap, but keep at least 1 (the newest) ---
            // Never delete index 0 (newest).
            var allowedCount = int.Max(1, _keepNewest);
            for (var i = allowedCount; i < dumps.Count; i++)
            {
                _ = TryDelete(dumps[i]);
            }

            // Refresh list after deletes
            dumps =
            [
                .. di.EnumerateFiles("*.dmp", SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTimeUtc),
            ];

            if (dumps.Count == 0)
            {
                return;
            }

            // --- 2) Enforce size cap by deleting oldest first, but keep at least 1 dump ---
            var total = dumps.Sum(SafeLength);

            // If maxTotalBytes is 0, we still keep 1 newest dump.
            if (dumps.Count == 1)
            {
                return;
            }

            for (var i = dumps.Count - 1; i >= 1 && total > _maxTotalBytes; i--)
            {
                var len = SafeLength(dumps[i]);
                if (TryDelete(dumps[i]))
                {
                    total -= len;
                }

                // Keep at least one dump (index 0). Loop condition i >= 1 enforces that.
            }
        }
        catch
        {
            // Avoid exceptions escaping from crash paths.
        }
    }
}
