// -----------------------------------------------------------------------
// <copyright file="BigArchiveBuilder.cs" company="Sage.Net">
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

using System.Text;
using Microsoft.Extensions.Logging;
using Sage.Net.Io.Extensions;
using Sage.Net.LoggerHelper;

namespace Sage.Net.Io.Big;

/// <summary>
/// Responsible for constructing a BIG archive file, a custom archive format used to store multiple files.
/// </summary>
/// <param name="logger">The logger to use for logging events.</param>
/// <remarks>
/// This class allows for adding individual files with specified internal paths
/// and assembling them into a single archive output stream.
/// </remarks>
public sealed partial class BigArchiveBuilder(ILogger logger)
{
    private readonly List<(string InternalPath, Func<Stream> OpenSource)> _pendingFiles = [];

    /// <summary>
    /// Adds a file to the BIG archive with a specified internal path.
    /// </summary>
    /// <param name="internalPath">The internal path of the file within the archive.</param>
    /// <param name="openSource">
    /// A function that provides a stream containing the file's data. The stream should support reading.
    /// </param>
    public void AddFile(string internalPath, Func<Stream> openSource) => _pendingFiles.Add((internalPath, openSource));

    /// <summary>
    /// Builds a BIG archive by writing all added files and associated metadata to the specified destination stream.
    /// The destination stream will contain a formatted BIG archive file.
    /// </summary>
    /// <param name="destination">
    /// The destination stream where the BIG archive will be written.
    /// The stream must support writing and will remain open after the method completes.
    /// </param>
    public void Build(Stream destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        using IDisposable? logContext = LogContext.BeginOperation(logger, nameof(Build));
        using BinaryWriter writer = new(destination, Encoding.ASCII, leaveOpen: true);

        // BIGF (4) + TotalSize (4) + FileCount (4) + HeaderSize (4)
        var headerSize = 16U;
        foreach ((var path, Func<Stream> _) in _pendingFiles)
        {
            // Offset (4) + Size (4) + Path + Null Terminator (1)
            headerSize += 8 + (uint)Encoding.ASCII.GetByteCount(path) + 1;
        }

        var fileCount = _pendingFiles.Count;
        var currentDataOffset = headerSize;

        writer.Write(0x46474942); // "BIGF"
        writer.WriteInt32BigEndian(fileCount);
        writer.WriteUInt32BigEndian(headerSize);

        foreach ((var path, Func<Stream> sourceFactory) in _pendingFiles)
        {
            using Stream source = sourceFactory();
            var size = (uint)source.Length;

            writer.WriteUInt32BigEndian(currentDataOffset);
            writer.WriteUInt32BigEndian(size);
            writer.Write(Encoding.ASCII.GetBytes(path));
            writer.Write((byte)0);

            currentDataOffset += size;

            Log.FileAdded(logger, path);
        }

        foreach ((var path, Func<Stream> sourceFactory) in _pendingFiles)
        {
            using Stream source = sourceFactory();
            source.CopyTo(destination);

            Log.FileDataAdded(logger, path);
        }

        var totalSize = (uint)destination.Length;
        destination.Position = 4;
        writer.Write(totalSize);

        destination.Flush();
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Debug, "Added file '{Path}' to BIG archive.")]
        public static partial void FileAdded(ILogger logger, string path);

        [LoggerMessage(LogLevel.Debug, "Added file '{Path}' data to BIG archive.")]
        public static partial void FileDataAdded(ILogger logger, string path);
    }
}
