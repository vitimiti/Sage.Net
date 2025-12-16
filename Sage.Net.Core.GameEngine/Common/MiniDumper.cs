// -----------------------------------------------------------------------
// <copyright file="MiniDumper.cs" company="Sage.Net">
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
using Sage.Net.Core.Libraries.WwVegas.WwLib;

namespace Sage.Net.Core.GameEngine.Common;

/// <summary>A sealed class responsible for managing and generating crash dumps in a multi-threaded environment.</summary>
/// <remarks>Provides capabilities to initialize, trigger, and shut down the dumper for diagnostic purposes. This class ensures thread safety and supports different types of dump modes.</remarks>
public sealed class MiniDumper : IDisposable
{
    private const string DumpDirName = "CrashDumps";
    private const string DumpFilePrefix = "Crash";
    private const string MarkerFileName = "last_crash.json";

    private readonly Lock _sync = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
    private readonly ManualResetEventSlim _dumpRequested = new(false);
    private readonly ManualResetEventSlim _dumpComplete = new(false);
    private readonly ManualResetEventSlim _quitting = new(false);

    private bool _initialized;
    private bool _loadedDbgHelp;
    private string _appId = "App";
    private string _dumpDir = string.Empty;
    private DumpType _requestedDumpType = DumpType.Minimal;
    private string _requestedReason = "unknown";
    private Thread? _thread;
    private bool _disposed;

    private MiniDumper() { }

    /// <summary>Gets the singleton instance of the dumper.</summary>
    public static MiniDumper? Instance { get; private set; }

    /// <summary>Gets a value indicating whether the dumper is initialized.</summary>
    public bool IsInitialized
    {
        get
        {
            lock (_sync)
            {
                return _initialized;
            }
        }
    }

    /// <summary>Initializes the dumper.</summary>
    /// <param name="userDirPath">The path to the user directory.</param>
    /// <param name="appId">The application ID.</param>
    /// <exception cref="InvalidOperationException">The dumper is already initialized.</exception>
    public static void InitMiniDumper(string userDirPath, string appId)
    {
        if (Instance is not null)
        {
            throw new InvalidOperationException("MiniDumper already initialized.");
        }

        var md = new MiniDumper();
        md.Initialize(userDirPath, appId);
        Instance = md;
    }

    /// <summary>Shuts down the dumper.</summary>
    public static void ShutdownMiniDumper()
    {
        Instance?.Dispose();
        Instance = null;
    }

    /// <summary>Triggers the generation of a crash dump.</summary>
    /// <param name="dumpType">The type of dump to generate, such as minimal or full.</param>
    public void TriggerMiniDump(DumpType dumpType) => TriggerMiniDumpForException(reason: "manual", dumpType);

    /// <summary>Triggers the generation of multiple types of crash dumps for a specific exception scenario.</summary>
    /// <param name="reason">The context or reason for generating the crash dumps.</param>
    public void TriggerMiniDumpsForException(string reason)
    {
        TriggerMiniDumpForException(reason, DumpType.Minimal);
        TriggerMiniDumpForException(reason, DumpType.Full);
    }

    /// <summary>Triggers the generation of a crash dump for a specific exception scenario.</summary>
    /// <param name="reason">The reason or context for the exception triggering the crash dump.</param>
    /// <param name="dumpType">The type of dump to generate, such as minimal or full.</param>
    public void TriggerMiniDumpForException(string reason, DumpType dumpType)
    {
        lock (_sync)
        {
            if (!_initialized)
            {
                return;
            }

            if (_thread?.IsAlive != true)
            {
                return;
            }

            _requestedReason = string.IsNullOrWhiteSpace(reason) ? "unknown" : reason;
            _requestedDumpType = dumpType;

            _dumpComplete.Reset();
            _dumpRequested.Set();
        }

        _dumpComplete.Wait();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_sync)
        {
            if (!_initialized)
            {
                return;
            }

            _initialized = false;
        }

        _quitting.Set();
        if (_thread?.IsAlive == true && !_thread.Join(TimeSpan.FromSeconds(3)))
        {
            // Avoid Thread.Abort (not supported). Worst case: leave thread to die with process.
        }

        _thread = null;

        if (_loadedDbgHelp)
        {
            DbgHelpLoader.Unload();
            _loadedDbgHelp = false;
        }

        _dumpRequested.Dispose();
        _dumpComplete.Dispose();
        _quitting.Dispose();

        _disposed = true;
    }

    private static string Sanitize(string s) =>
        Path.GetInvalidFileNameChars().Aggregate(s, (current, c) => current.Replace(c, '_'));

    private void Initialize(string userDirPath, string appId)
    {
        _appId = string.IsNullOrWhiteSpace(appId) ? "App" : appId;
        _loadedDbgHelp = DbgHelpLoader.Load();
        if (!_loadedDbgHelp || DbgHelpLoader.IsFailed)
        {
            return;
        }

        _dumpDir = Path.Combine(userDirPath, DumpDirName);
        _ = Directory.CreateDirectory(_dumpDir);

        _thread = new Thread(ThreadProc) { IsBackground = true, Name = "MiniDumper" };
        _thread.Start();

        lock (_sync)
        {
            _initialized = true;
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid crash paths here."
    )]
    private void WriteMarker(CrashMarker marker)
    {
        try
        {
            var path = Path.Combine(_dumpDir, MarkerFileName);
            var json = JsonSerializer.Serialize(marker, _jsonSerializerOptions);

            var tmp = $"{path}.tmp";
            File.WriteAllText(tmp, json);
            File.Move(tmp, path, overwrite: true);
        }
        catch
        {
            // best-effort
        }
    }

    private void CreateMiniDump(DumpType dumpType, string reason)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var pid = Environment.ProcessId;

        var dumpTypeSuffix = dumpType is DumpType.Full ? "F" : "M";
        var safeReason = Sanitize(reason);

        var fileName = $"{DumpFilePrefix}{dumpTypeSuffix}_{now:yyyyMMdd_HHmmss}_{pid}_{safeReason}.dmp";
        var dumpPath = Path.Combine(_dumpDir, fileName);

        Microsoft.Diagnostics.NETCore.Client.DumpType actualDumpType =
            dumpType is DumpType.Full
                ? Microsoft.Diagnostics.NETCore.Client.DumpType.WithHeap
                : Microsoft.Diagnostics.NETCore.Client.DumpType.Normal;

        var ok = DbgHelpLoader.WriteDump(dumpPath, actualDumpType);
        WriteMarker(
            new CrashMarker
            {
                AppId = _appId,
                TimestampUtc = now,
                ProcessId = pid,
                Reason = reason,
                DumpType = dumpType.ToString(),
                DumpFileName = ok ? fileName : null,
            }
        );
    }

    private void ThreadProc()
    {
        while (true)
        {
            var signaled = WaitHandle.WaitAny([_dumpRequested.WaitHandle, _quitting.WaitHandle]);
            if (signaled == 1)
            {
                return; // Quitting.
            }

            DumpType type;
            string reason;

            lock (_sync)
            {
                type = _requestedDumpType;
                reason = _requestedReason;
                _dumpRequested.Reset();
            }

            try
            {
                CreateMiniDump(type, reason);
            }
            finally
            {
                _dumpComplete.Set();
            }
        }
    }

    private sealed class CrashMarker
    {
        public string? AppId { get; set; }

        public DateTimeOffset TimestampUtc { get; set; }

        public int ProcessId { get; set; }

        public string? Reason { get; set; }

        public string? DumpType { get; set; }

        public string? DumpFileName { get; set; }
    }
}
