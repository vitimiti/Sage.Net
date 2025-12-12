// -----------------------------------------------------------------------
// <copyright file="ArchiveFile.cs" company="Sage.Net">
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

namespace Sage.Net.GameEngine.Common;

/// <summary>A class that represents an archive file.</summary>
public abstract class ArchiveFile : IDisposable, IAsyncDisposable
{
    private bool _disposed;

    /// <summary>Gets a <see cref="string"/> with the archive name.</summary>
    public abstract string ArchiveName { get; }

    /// <summary>Gets a <see cref="string"/> with the archive path.</summary>
    public abstract string ArchivePath { get; }

    /// <summary>Gets or sets the base <see cref="Stream"/> that holds the archive file data.</summary>
    protected Stream? BaseStream { get; set; }

    /// <summary>Gets the rood directory <see cref="DetailedArchivedDirectoryInfo"/>.</summary>
    protected DetailedArchivedDirectoryInfo RootDirectory { get; } = new(string.Empty, [], []);

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>Tries to get the file information of a file.</summary>
    /// <param name="fileName">A <see cref="string"/> with the file to get the information from.</param>
    /// <param name="fileInfo">A new instance of <see cref="FileInfo"/> with the <paramref name="fileName"/> information; or <see langword="null"/> on failure.</param>
    /// <returns><see langword="true"/> if the <paramref name="fileInfo"/> could be retrieved; otherwise <see langword="false"/>.</returns>
    public abstract bool TryGetFileInfo(string fileName, out FileInfo? fileInfo);

    /// <summary>Opens the given archive file.</summary>
    /// <param name="fileName">A <see cref="string"/> with the file to open.</param>
    /// <param name="access">The <see cref="FileAccess"/> requested to open the <paramref name="fileName"/>.</param>
    /// <returns>A new <see cref="Stream"/> with the given <paramref name="fileName"/> data; or <see langword="null"/> on failure.</returns>
    public abstract Stream? OpenFile(string fileName, FileAccess access);

    /// <summary>Closes all files.</summary>
    public abstract void CloseAllFiles();

    /// <summary>Sets the search priority for the archive files.</summary>
    /// <param name="newPriority">An <see cref="int"/> with the priority value.</param>
    public abstract void SetSearchPriority(int newPriority);

    /// <summary>Closes the archive file.</summary>
    public abstract void Close();

    /// <summary>Attaches a file to the archive file.</summary>
    /// <param name="file">The <see cref="Stream"/> to attach.</param>
    public void AttachFile(Stream file)
    {
        BaseStream?.Dispose();
        BaseStream = file;
    }

    /// <summary>Retrieves the list of files in the archive directory.</summary>
    /// <param name="directoryPath">A <see cref="string"/> with the directory path.</param>
    /// <param name="searchPattern">A <see cref="string"/> with the search pattern.</param>
    /// <param name="fileNameList">A <see cref="ISet{T}"/> of <see cref="string"/> populated with the file names.</param>
    /// <param name="searchSubdirectories"><see langword="true"/> to search recursively; otherwise <see langword="false"/>.</param>
    public void GetFileListInDirectory(
        string directoryPath,
        string searchPattern,
        ISet<string> fileNameList,
        bool searchSubdirectories
    )
    {
        ArgumentNullException.ThrowIfNull(fileNameList);

        DetailedArchivedDirectoryInfo dirInfo = RootDirectory;
        if (!string.IsNullOrEmpty(directoryPath))
        {
            var tokens = directoryPath.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (var rawToken in tokens)
            {
                var token = rawToken.ToUpperInvariant();
                if (dirInfo.Directories.TryGetValue(token, out DetailedArchivedDirectoryInfo? nextDir))
                {
                    dirInfo = nextDir;
                }
                else
                {
                    return;
                }
            }
        }

        GetFileListInDirectory(dirInfo, directoryPath, searchPattern, fileNameList, searchSubdirectories);
    }

    /// <summary>Adds a file to the archive.</summary>
    /// <param name="path">A <see cref="string"/> with the file path to add.</param>
    /// <param name="fileInfo">The <see cref="ArchivedFileInfo"/> with the file information.</param>
    [SuppressMessage(
        "Globalization",
        "CA1308:Normalize strings to uppercase",
        Justification = "Backwards compatibility with original files."
    )]
    public void AddFile(string path, ArchivedFileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(fileInfo);

        var tokens = path.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries
        );

        DetailedArchivedDirectoryInfo currentDir = RootDirectory;
        foreach (var rawToken in tokens)
        {
            var token = rawToken.ToLowerInvariant();
            if (!currentDir.Directories.TryGetValue(token, out DetailedArchivedDirectoryInfo? nextDir))
            {
                nextDir = new DetailedArchivedDirectoryInfo(token, [], []);
                currentDir.Directories[token] = nextDir;
            }

            currentDir = nextDir;
        }

        currentDir.Files[fileInfo.FileName] = fileInfo;
    }

    /// <summary>Safely disposes the managed data of the <see cref="ArchiveFile"/> instance.</summary>
    /// <param name="disposing"><see langword="true"/> when disposing the managed resources; otherwise <see langword="false"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            BaseStream?.Dispose();
        }

        _disposed = true;
    }

    /// <summary>Asynchronously disposes the managed data of the <see cref="ArchiveFile"/> instance.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (BaseStream is not null)
        {
            await BaseStream.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>Gets the archived file information.</summary>
    /// <param name="fileName">A <see cref="string"/> with the archived file information.</param>
    /// <returns>A new <see cref="ArchivedFileInfo"/> instance; or <see langword="null"/> on failure.</returns>
    protected ArchivedFileInfo? GetArchivedFileInfo(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        DetailedArchivedDirectoryInfo dirInfo = RootDirectory;
        var tokens = fileName.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (var rawToken in tokens)
        {
            var token = rawToken.ToUpperInvariant();
            if (dirInfo.Files.TryGetValue(token, out ArchivedFileInfo? fileInfo))
            {
                return fileInfo;
            }

            if (dirInfo.Directories.TryGetValue(token, out DetailedArchivedDirectoryInfo? nextDir))
            {
                dirInfo = nextDir;
            }
            else
            {
                return null;
            }
        }

        return null;
    }

    private static bool SearchStringMatches(string str, string searchString) =>
        SearchStringMatches(str.AsSpan(), searchString.AsSpan());

    private static bool SearchStringMatches(ReadOnlySpan<char> str, ReadOnlySpan<char> searchString)
    {
        if (str.IsEmpty)
        {
            return searchString.IsEmpty;
        }

        if (searchString.IsEmpty)
        {
            return false;
        }

        var strIndex = 0;
        var searchIndex = 0;

        while (strIndex < str.Length && searchIndex < searchString.Length)
        {
            var s = str[strIndex];
            var p = searchString[searchIndex];

            if (p == '*')
            {
                return SearchWildcard(str[strIndex..], searchString[(searchIndex + 1)..]);
            }

            if (s != p && p != '?')
            {
                return false;
            }

            strIndex++;
            searchIndex++;
        }

        return strIndex == str.Length && searchIndex == searchString.Length;
    }

    private static bool SearchWildcard(ReadOnlySpan<char> str, ReadOnlySpan<char> searchString)
    {
        if (searchString.IsEmpty)
        {
            return true;
        }

        for (var i = 0; i < str.Length; i++)
        {
            if (SearchStringMatches(str[i..], searchString))
            {
                return true;
            }
        }

        return false;
    }

    private static string AppendPath(string basePath, string part)
    {
#pragma warning disable IDE0046 // 'if' statement can be simplified
        if (string.IsNullOrEmpty(basePath))
#pragma warning restore IDE0046 // 'if' statement can be simplified
        {
            return part;
        }

        return basePath.EndsWith(Path.DirectorySeparatorChar) || basePath.EndsWith(Path.AltDirectorySeparatorChar)
            ? basePath + part
            : basePath + Path.DirectorySeparatorChar + part;
    }

    private static void GetFileListInDirectory(
        DetailedArchivedDirectoryInfo dirInfo,
        string currentPath,
        string searchPattern,
        ISet<string> filenameList,
        bool searchSubdirectories
    )
    {
        ArgumentNullException.ThrowIfNull(filenameList);

        if (searchSubdirectories)
        {
            foreach (DetailedArchivedDirectoryInfo subDir in dirInfo.Directories.Values)
            {
                var newPath = AppendPath(currentPath, subDir.DirectoryName);
                GetFileListInDirectory(subDir, newPath, searchPattern, filenameList, searchSubdirectories);
            }
        }

#pragma warning disable IDE0055 // Fix formatting
        foreach (
            var fullPath in from filePair in dirInfo.Files
            select filePair.Value into fileInfo
            where SearchStringMatches(fileInfo.FileName, searchPattern)
            select AppendPath(currentPath, fileInfo.FileName)
        )
#pragma warning restore IDE0055 // Fix formatting
        {
            _ = filenameList.Add(fullPath);
        }
    }
}
