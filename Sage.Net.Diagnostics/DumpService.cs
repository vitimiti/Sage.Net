// -----------------------------------------------------------------------
// <copyright file="DumpService.cs" company="Sage.Net">
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
using System.Globalization;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sage.Net.Abstractions;
using Sage.Net.LoggerHelper;

namespace Sage.Net.Diagnostics;

/// <summary>
/// Provides functionality to generate diagnostic process dumps for the current process based on the
/// configured <see cref="DumpOptions"/>.
/// </summary>
/// <remarks>
/// The service resolves a per-user dump directory under the platform-specific application data folder and
/// enforces retention by removing older dump files once the configured maximum is reached.
/// </remarks>
public partial class DumpService : IDumpService
{
    private readonly DumpOptions _options;
    private readonly ILogger<DumpService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DumpService"/> class.
    /// </summary>
    /// <param name="dumpOptions">The options used to configure dump generation.</param>
    /// <param name="gameOptions">The options used to identify the current game.</param>
    /// <param name="logger">The logger used to log events.</param>
    /// <remarks>
    /// This service generates process dumps and stores them in a designated directory, which is derived based on application configuration and platform-specific user data paths.
    /// It also manages dump file retention by enforcing a maximum file count limit and removing older files as needed.
    /// </remarks>
    public DumpService(
        IOptions<DumpOptions> dumpOptions,
        IOptions<GameOptions> gameOptions,
        ILogger<DumpService> logger
    )
    {
        ArgumentNullException.ThrowIfNull(dumpOptions);
        ArgumentNullException.ThrowIfNull(gameOptions);

        _options = dumpOptions.Value;
        GameOptions gameOpts = gameOptions.Value;
        _logger = logger;

        DumpDirectoryPath = Path.Combine(
            Environment.GetFolderPath(
                OperatingSystem.IsWindows()
                    ? Environment.SpecialFolder.LocalApplicationData
                    : Environment.SpecialFolder.ApplicationData
            ),
            gameOpts.GameId,
            _options.DumpDirectory
        );
    }

    /// <inheritdoc />
    public string DumpDirectoryPath { get; }

    /// <inheritdoc />
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid exceptions in crashing pathways."
    )]
    public void WriteDump(string reason)
    {
        using IDisposable? logContext = LogContext.BeginOperation(_logger, nameof(WriteDump));

        if (!_options.Enabled)
        {
            Log.DumpDisabled(_logger);
            return;
        }

        try
        {
            if (!Directory.Exists(DumpDirectoryPath))
            {
                _ = Directory.CreateDirectory(DumpDirectoryPath);
            }

            CleanOldDumps();

            var processId = Environment.ProcessId;
            var client = new DiagnosticsClient(processId);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var fileName = $"{_options.FilePrefix}_{reason}_{timestamp}.dmp";
            var fullPath = Path.Combine(DumpDirectoryPath, fileName);

            // This is the core call that works cross-platform
            client.WriteDump(_options.DumpLevel, fullPath);

            Log.DumpCreatedSuccessfully(_logger, fullPath);
        }
        catch (Exception ex)
        {
            Log.FailedToGenerateDump(_logger, ex);
        }
    }

    private void CleanOldDumps()
    {
        if (!Directory.Exists(DumpDirectoryPath))
        {
            return;
        }

        var files = new DirectoryInfo(DumpDirectoryPath)
            .GetFiles($"{_options.FilePrefix}*.dmp")
            .OrderByDescending(f => f.CreationTime)
            .ToList();

        if (files.Count < _options.MaxDumpFiles)
        {
            return;
        }

        foreach (FileInfo file in files.Skip(_options.MaxDumpFiles - 1))
        {
            file.Delete();
        }
    }

    private static partial class Log
    {
        [LoggerMessage(
            LogLevel.Trace,
            Message = "Dump creation requested but skipped because it is disabled in configuration."
        )]
        public static partial void DumpDisabled(ILogger logger);

        [LoggerMessage(LogLevel.Information, Message = "Dump created successfully: {Path}")]
        public static partial void DumpCreatedSuccessfully(ILogger logger, string path);

        [LoggerMessage(LogLevel.Error, "Failed to generate dump")]
        public static partial void FailedToGenerateDump(ILogger logger, Exception ex);
    }
}
