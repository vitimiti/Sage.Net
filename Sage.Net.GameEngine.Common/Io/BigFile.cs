// -----------------------------------------------------------------------
// <copyright file="BigFile.cs" company="Sage.Net">
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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sage.Net.GameEngine.Common.Io;

/// <summary>
/// Represents a BIG archive file entry point that provides basic operations
/// for querying file metadata and opening file streams from the underlying
/// storage. The current implementation performs lookups via <see cref="ArchivedFileInfo"/>
/// and returns in-memory streams for opened files.
/// </summary>
/// <param name="name">The logical name of the archive.</param>
/// <param name="path">The full file system path to the archive.</param>
public class BigFile(string name, string path) : ArchiveFile
{
    /// <summary>
    /// Gets the logical file name of the archive.
    /// </summary>
    public override string FileName => name;

    /// <summary>
    /// Gets the full path to the archive on disk.
    /// </summary>
    public override string FilePath => path;

    /// <summary>
    /// Attempts to retrieve a <see cref="FileInfo"/> for the specified file inside the archive.
    /// </summary>
    /// <param name="fileName">The path of the file within the archive.</param>
    /// <param name="fileInfo">When this method returns, contains the resulting <see cref="FileInfo"/>, if found; otherwise, <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the file exists and the <see cref="FileInfo"/> was created; otherwise, <see langword="false"/>.
    /// </returns>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "We want to log the error and return false."
    )]
    public override bool TryGetFileInfo(string fileName, out FileInfo? fileInfo)
    {
        fileInfo = null;
        ArchivedFileInfo? tempFileInfo = GetArchivedFileInfo(fileName);
        if (tempFileInfo is null)
        {
            return false;
        }

        try
        {
            fileInfo = new FileInfo(fileName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get file info for {fileName}: {ex.Message}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Opens the specified file from the archive and returns its contents as a readable in-memory stream.
    /// </summary>
    /// <param name="fileName">The path of the file within the archive.</param>
    /// <param name="access">The requested file access. Currently informational and may be ignored by the implementation.</param>
    /// <param name="mode">The file mode. Currently informational and may be ignored by the implementation.</param>
    /// <returns>
    /// A <see cref="Stream"/> positioned at the beginning of the file data if successful; otherwise, <see langword="null"/>.
    /// </returns>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "We want to log the error and return null."
    )]
    public override Stream? OpenFile(string fileName, FileAccess access, FileMode mode)
    {
        ArchivedFileInfo? fileInfo = GetArchivedFileInfo(fileName);
        if (fileInfo is null)
        {
            return null;
        }

        try
        {
            using FileStream stream = File.OpenRead(fileName);
            MemoryStream ms = new();
            stream.CopyTo(ms);
            return ms;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to open file {fileName}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Closes all open file handles associated with this archive. This implementation is a no-op.
    /// </summary>
    public override void CloseAllFiles() { }

    /// <summary>
    /// Sets the search priority for this archive when resolving files across multiple sources.
    /// This implementation is a no-op.
    /// </summary>
    /// <param name="priority">The priority value; higher values indicate higher priority.</param>
    public override void SetSearchPriority(int priority) { }

    /// <summary>
    /// Closes the archive and releases any associated resources. This implementation is a no-op.
    /// </summary>
    public override void Close() { }
}
