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

namespace Sage.Net.Io.ArchiveFiles;

/// <summary>Represents an archived directory called BIG file.</summary>
/// <param name="name">A <see cref="string"/> with the file name.</param>
/// <param name="path">A <see cref="string"/> with the file path.</param>
public class BigFile(string name, string path) : ArchiveFile
{
    private readonly Lock _lock = new();

    /// <inheritdoc/>
    public override string ArchiveName => name;

    /// <inheritdoc/>
    public override string ArchivePath => path;

    /// <inheritdoc/>
    public override bool TryGetFileInfo(string fileName, out FileInfo? fileInfo)
    {
        ArchivedFileInfo? tempFileInfo = GetArchivedFileInfo(fileName);
        if (tempFileInfo is null)
        {
            fileInfo = null;
            return false;
        }

        fileInfo = new FileInfo(fileName);
        return true;
    }

    /// <inheritdoc/>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "We want to log the exception in debug mode and continue."
    )]
    public override Stream? OpenFile(string fileName, FileAccess access)
    {
        ArchivedFileInfo? fileInfo = GetArchivedFileInfo(fileName);
        if (fileInfo is null || BaseStream is null)
        {
            return null;
        }

        byte[] fileData;

        lock (_lock)
        {
            if (BaseStream.Length < fileInfo.Offset + fileInfo.Size)
            {
                return null;
            }

            BaseStream.Position = fileInfo.Offset;
            fileData = new byte[fileInfo.Size];
            BaseStream.ReadExactly(fileData);
        }

        var memoryStream = new MemoryStream(fileData, writable: true);
        if ((access & FileAccess.Read) != 0)
        {
            return memoryStream;
        }

        try
        {
            var directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                _ = Directory.CreateDirectory(directory);
            }

            FileStream localFileStream = new(fileName, FileMode.Create, access);
            memoryStream.WriteTo(localFileStream);

            localFileStream.Position = 0;
            memoryStream.Dispose();

            return localFileStream;
        }
        catch (Exception ex)
        {
            Debug.Fail($"Failed to extract to local file {fileName}", ex.ToString());
            return null;
        }
    }

    /// <inheritdoc/>
    /// <remarks>This is a no-op.</remarks>
    public override void CloseAllFiles() { }

    /// <inheritdoc/>
    /// <remarks>This is a no-op.</remarks>
    public override void SetSearchPriority(int newPriority) { }

    /// <inheritdoc/>
    /// <remarks>This is a no-op.</remarks>
    public override void Close() { }
}
