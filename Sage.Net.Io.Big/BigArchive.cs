// -----------------------------------------------------------------------
// <copyright file="BigArchive.cs" company="Sage.Net">
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

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sage.Net.Io.Extensions;
using Sage.Net.LoggerHelper;

namespace Sage.Net.Io.Big;

/// <summary>
/// Represents a Big Archive file container, which can store and provide access
/// to multiple files using a single archive.
/// This class provides methods for opening, reading, and enumerating files within
/// the archive.
/// </summary>
/// <remarks>
/// The BigArchive is a sealed class that implements <see cref="IDisposable"/> to
/// ensure proper resource management when working with underlying file streams.
/// It provides support for common file-based operations, such as checking for
/// the existence of files, retrieving file streams, and enumerating files stored
/// in the archive.
/// </remarks>
public sealed partial class BigArchive : IDisposable
{
    private const uint BigFSignature = 0x46474942; // "BIGF" in little endian

    private static ILogger? _logger;

    private readonly FileStream _archiveStream;
    private readonly FrozenDictionary<string, BigArchiveEntry> _entries;

    private bool _disposed;

    /// <summary>
    /// Gets the name of the archive file.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the full path to the archive file.
    /// </summary>
    public string FullPath { get; }

    private BigArchive(FileStream stream, string name, string path, Dictionary<string, BigArchiveEntry> entries)
    {
        _archiveStream = stream;
        Name = name;
        FullPath = path;
        _entries = entries.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Opens a BigArchive file from the specified path using the provided service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to retrieve dependencies such as logging services.</param>
    /// <param name="filePath">The full path to the BigArchive file to be opened.</param>
    /// <returns>
    /// A BigArchive instance if the file is successfully opened and parsed; otherwise, null.
    /// </returns>
    /// <exception cref="InvalidDataException">Thrown when the file's data does not conform to the expected BigArchive format.</exception>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Exception free method, returns null on failure after attempting to log the error."
    )]
    public static BigArchive? Open(IServiceProvider serviceProvider, string filePath)
    {
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("Sage.Net.Io.Big.BigArchive");
        IDisposable? logContext = null;
        if (_logger is not null)
        {
            logContext = LogContext.BeginOperation(_logger, nameof(Open));
        }

        try
        {
            FileStream stream = new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true
            );

            using BinaryReader reader = new(stream, Encoding.ASCII, leaveOpen: true);
            var signature = reader.ReadUInt32();
            if (signature != BigFSignature)
            {
                if (_logger is not null)
                {
                    Log.BadArchive(_logger, filePath);
                }

                return null;
            }

            // Header metadata:
            // Format: ArchiveSize (4), FileCount (4, BE), HeaderSize (4, BE)
            var totalSize = reader.ReadUInt32();
            var fileCount = reader.ReadUInt32BigEndian();

            if (_logger is not null)
            {
                Log.Opened(_logger, filePath, fileCount, totalSize);
            }

            // Directory Index
            var entries = new Dictionary<string, BigArchiveEntry>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < fileCount; i++)
            {
                var offset = reader.ReadUInt32BigEndian();
                var size = reader.ReadUInt32BigEndian();

                var rawInternalPath = reader.ReadNullTerminatedString();
                var internalPath = rawInternalPath.Replace('\\', '/');
                var pathParts = internalPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
                var fileName = pathParts.Length > 0 ? pathParts[^1] : internalPath;

                BigArchiveEntry entry = new(fileName, internalPath, offset, size);
                entries[internalPath] = entry;
                if (_logger is not null)
                {
                    // "Reading entry '{EntryName}' from '{FilePath}' with offset {Offset}, size {Size} and internal path of '{InternalPath}'."
                    Log.FoundEntry(_logger, entry.FileName, filePath, offset, size, internalPath);
                }
            }

            if (_logger is not null)
            {
                Log.FoundEntries(_logger, filePath, fileCount);
            }

            return new BigArchive(stream, Path.GetFileName(filePath), filePath, entries);
        }
        catch (Exception ex)
        {
            if (_logger is not null)
            {
                Log.FailedToOpen(_logger, filePath, ex.ToString());
            }

            return null;
        }
        finally
        {
            logContext?.Dispose();
        }
    }

    /// <summary>
    /// Opens a stream for reading the specified file within the BigArchive.
    /// </summary>
    /// <param name="internalPath">The internal path of the file to be opened within the archive.</param>
    /// <returns>
    /// A stream for reading the file if the specified file exists; otherwise, null.
    /// </returns>
    public Stream? OpenFile(string internalPath)
    {
        if (_entries.TryGetValue(internalPath, out BigArchiveEntry? entry))
        {
            return new BigFileStream(_archiveStream, entry.Offset, entry.Size);
        }

        if (_logger is not null)
        {
            Log.EntryNotFound(_logger, FullPath, internalPath);
        }

        return null;
    }

    /// <summary>
    /// Determines whether the archive contains a file with the specified internal path.
    /// </summary>
    /// <param name="internalPath">The relative path of the file within the archive to check for existence.</param>
    /// <returns>
    /// True if a file with the specified path exists in the archive; otherwise, false.
    /// </returns>
    public bool FileExists(string internalPath) => _entries.ContainsKey(internalPath);

    /// <summary>
    /// Enumerates the file paths of all entries stored in the BigArchive.
    /// </summary>
    /// <returns>
    /// An enumerable collection of strings representing the file paths of the entries within the archive.
    /// </returns>
    public IEnumerable<string> EnumerateFiles() => _entries.Keys;

    /// <summary>
    /// Releases all resources used by the BigArchive instance, including the underlying file stream.
    /// </summary>
    /// <remarks>
    /// This method should be called when the BigArchive instance is no longer needed to ensure proper release
    /// of system resources. Failure to do so may result in file handle leaks or other resource contention issues.
    /// Once this method is called, the instance should not be used further.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _archiveStream.Dispose();

        _disposed = true;
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Error, "File '{FilePath}' is not a valid BigArchive file.")]
        public static partial void BadArchive(ILogger logger, string filePath);

        [LoggerMessage(
            LogLevel.Debug,
            "Opened BigArchive file '{FilePath}' with {FileCount} entries and a size of {FileSize}."
        )]
        public static partial void Opened(ILogger logger, string filePath, uint fileCount, uint fileSize);

        [LoggerMessage(
            LogLevel.Trace,
            "Reading entry '{EntryName}' from '{FilePath}' with offset {Offset}, size {Size} and internal path of '{InternalPath}'."
        )]
        public static partial void FoundEntry(
            ILogger logger,
            string entryName,
            string filePath,
            uint offset,
            uint size,
            string internalPath
        );

        [LoggerMessage(LogLevel.Debug, "Found {EntryCount} entries in '{FilePath}'.")]
        public static partial void FoundEntries(ILogger logger, string filePath, uint entryCount);

        [LoggerMessage(LogLevel.Error, "Failed to open BIG file '{FilePath}': {ExceptionMessage}.")]
        public static partial void FailedToOpen(ILogger logger, string filePath, string exceptionMessage);

        [LoggerMessage(LogLevel.Error, "Cannot find entry '{EntryName}' in '{FilePath}'.")]
        public static partial void EntryNotFound(ILogger logger, string filePath, string entryName);
    }
}
