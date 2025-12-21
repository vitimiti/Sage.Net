// -----------------------------------------------------------------------
// <copyright file="ArchiveFileSystem.cs" company="Sage.Net">
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sage.Net.Io.Big;
using Sage.Net.LoggerHelper;

namespace Sage.Net.Game;

internal sealed partial class ArchiveFileSystem : IDisposable
{
    private readonly List<BigArchive> _archives = [];
    private readonly Dictionary<string, BigArchive> _fileToArchiveMap = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<BigArchive> Archives => _archives;

    public void Initialize(
        IServiceProvider services,
        string baseGameDirectory,
        string? modBigFilesDirectory,
        string? modBigFileExtension
    )
    {
        ILogger? logger = services.GetService<ILoggerFactory>()?.CreateLogger("Sage.Net.Game.ArchiveFileSystem");
        IDisposable? logContext = null;
        if (logger is not null)
        {
            logContext = LogContext.BeginOperation(logger, nameof(Initialize));
        }

        LoadArchives(services, baseGameDirectory, "*.big", logger, Log.BaseArchiveLoaded);

        if (modBigFilesDirectory is not null)
        {
            LoadArchives(services, modBigFilesDirectory, $"*{modBigFileExtension}", logger, Log.ModdedArchiveLoaded);
        }

        if (logger is not null)
        {
            Log.Count(logger, _archives.Count);
        }

        logContext?.Dispose();
    }

    /// <summary>
    /// Opens a file with the specified internal path from the archive system, if it exists.
    /// </summary>
    /// <param name="internalPath">
    /// The normalized internal path of the file to open, which is case-insensitive and expects
    /// forward slashes ('/') as path separators.
    /// </param>
    /// <returns>
    /// A <see cref="Stream"/> representing the opened file if the file exists in the archive;
    /// otherwise, <c>null</c> if the file could not be found or the path is invalid.
    /// </returns>
    public Stream? OpenFile(string internalPath)
    {
        if (string.IsNullOrWhiteSpace(internalPath))
        {
            return null;
        }

        var normalizedPath = internalPath.Replace('\\', '/');
        return _fileToArchiveMap.TryGetValue(normalizedPath, out BigArchive? archive)
            ? archive.OpenFile(normalizedPath)
            : null;
    }

    public void Dispose()
    {
        foreach (BigArchive archive in _archives)
        {
            archive.Dispose();
        }
    }

    private void LoadArchives(
        IServiceProvider services,
        string directory,
        string searchPattern,
        ILogger? logger,
        Action<ILogger, string> logAction
    )
    {
        IEnumerable<string> files = Directory.EnumerateFiles(
            directory,
            searchPattern,
            new EnumerationOptions { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive }
        );

        foreach (var file in files)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var archive = BigArchive.Open(services, file);
#pragma warning restore CA2000 // Dispose objects before losing scope

            if (archive is null)
            {
                continue;
            }

            _archives.Add(archive);
            logAction?.Invoke(logger!, file);

            // Add or Overwrite the mapping for every file in this archive
            foreach (var internalPath in archive.EnumerateFiles())
            {
                _fileToArchiveMap[internalPath] = archive;
            }
        }
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Debug, Message = "Loaded base BIG archive: {Path}.")]
        public static partial void BaseArchiveLoaded(ILogger logger, string path);

        [LoggerMessage(LogLevel.Debug, Message = "Loaded modded BIG archive: {Path}.")]
        public static partial void ModdedArchiveLoaded(ILogger logger, string path);

        [LoggerMessage(LogLevel.Debug, Message = "Loaded {Count} BIG archives.")]
        public static partial void Count(ILogger logger, int count);
    }
}
