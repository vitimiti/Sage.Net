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

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Sage.Net.Core.GameEngine.Common.Io;

/// <summary>
/// Represents a BIG file.
/// </summary>
/// <param name="name">The name of the BIG file.</param>
/// <param name="path">The path to the BIG file.</param>
[DebuggerDisplay("{Name} ({Path})")]
public class BigFile(string name, string path) : IDisposable, IAsyncDisposable
{
    private readonly Lock _streamLock = new();

    private bool _disposed;

    /// <summary>
    /// Gets the name of the BIG file.
    /// </summary>
    public string Name => name;

    /// <summary>
    /// Gets the path to the BIG file.
    /// </summary>
    public string Path => path;

    /// <summary>
    /// Gets or sets the base stream.
    /// </summary>
    protected Stream? BaseStream { get; set; }

    /// <summary>
    /// Gets or sets the root directory info.
    /// </summary>
    protected DetailedArchivedDirectoryInfo RootDirectoryInfo { get; set; } = new();

    /// <summary>
    /// Gets a list of files in the BIG directory.
    /// </summary>
    /// <param name="dirInfo">The <see cref="DetailedArchivedDirectoryInfo"/> for the directory.</param>
    /// <param name="currentDirectory">The current directory.</param>
    /// <param name="searchName">The search name.</param>
    /// <param name="searchSubdirectories">Whether to search recursively.</param>
    /// <returns>A new <see cref="IEnumerable{T}"/> with the files in the directory.</returns>
    /// <seealso cref="GetFileListInDirectory(string,string,bool)"/>
    public static IEnumerable<string> GetFileListInDirectory(
        [NotNull] DetailedArchivedDirectoryInfo dirInfo,
        string currentDirectory,
        string searchName,
        bool searchSubdirectories
    )
    {
        if (searchSubdirectories)
        {
#pragma warning disable IDE0055 // Fix formatting
            foreach (
                var file in from subDirInfo in dirInfo.Directories.Values
                let subPath = AppendPath(currentDirectory, subDirInfo.DirectoryName)
                from file in GetFileListInDirectory(subDirInfo, subPath, searchName, searchSubdirectories)
                select file
            )
            {
                yield return file;
            }
#pragma warning restore IDE0055 // Fix formatting
        }

        foreach (var fileName in dirInfo.Files.Keys.Where(fileName => SearchStringMatches(fileName, searchName)))
        {
            yield return AppendPath(currentDirectory, fileName);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(continueOnCapturedContext: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Tries to get the file info.
    /// </summary>
    /// <param name="fileName">The file name to get the information from.</param>
    /// <param name="fileInfo">A new <see cref="FileInfo"/> instance on success; <see langword="null"/> otherwise.</param>
    /// <returns><see langword="true"/> on success; <see langword="false"/> otherwise.</returns>
    public bool TryGetFileInfo(string fileName, out FileInfo? fileInfo)
    {
        fileInfo = null;
        ArchivedFileInfo? tempFileInfo = GetArchivedFileInfo(fileName);
        if (tempFileInfo is null)
        {
            return false;
        }

        fileInfo = new FileInfo(tempFileInfo.FileName);
        return true;
    }

    /// <summary>
    /// Opens a file in the BIG file.
    /// </summary>
    /// <param name="fileName">The name of the BIG file.</param>
    /// <param name="access">The <see cref="FileAccess"/> used for the file.</param>
    /// <returns>A new <see cref="Stream"/> that holds the data of the given <paramref name="fileName"/>.</returns>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "This is exception safe, catching everything to return null."
    )]
    public Stream? OpenFile(string fileName, FileAccess access)
    {
        ArchivedFileInfo? fileInfo = GetArchivedFileInfo(fileName);
        if (fileInfo is null || BaseStream is null)
        {
            return null;
        }

        var buffer = ArrayPool<byte>.Shared.Rent((int)fileInfo.Size);
        try
        {
            lock (_streamLock)
            {
                _ = BaseStream.Seek(fileInfo.Offset, SeekOrigin.Begin);
                BaseStream.ReadExactly(buffer, 0, (int)fileInfo.Size);
            }

            MemoryStream ms = new(buffer);
            if (access is FileAccess.Read)
            {
                return ms;
            }

            try
            {
                FileStream localFile = File.Open(fileName, FileMode.Create, FileAccess.Write);
                ms.CopyTo(localFile);

                localFile.Position = 0;
                return localFile;
            }
            catch
            {
                return null;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Attaches a file to the BIG file.
    /// </summary>
    /// <param name="file">The <see cref="Stream"/> with the file data to attach.</param>
    public void AttachFile(Stream? file)
    {
        if (file is not null)
        {
            BaseStream?.Dispose();
            BaseStream = null;
        }

        BaseStream = file;
    }

    /// <summary>
    /// Gets a list of files in the BIG directory.
    /// </summary>
    /// <param name="originalDirectory">The original directory.</param>
    /// <param name="searchName">The search name.</param>
    /// <param name="searchSubdirectories">Whether to search recursively.</param>
    /// <returns>A new <see cref="IEnumerable{T}"/> with the files in the directory.</returns>
    public IEnumerable<string> GetFileListInDirectory(
        [NotNull] string originalDirectory,
        string searchName,
        bool searchSubdirectories
    )
    {
        DetailedArchivedDirectoryInfo dirInfo = RootDirectoryInfo;
        var tokens = originalDirectory.ToUpperInvariant().Split('\\', '/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens)
        {
            if (dirInfo.Directories.TryGetValue(token, out DetailedArchivedDirectoryInfo? nextDir))
            {
                dirInfo = nextDir;
            }
            else
            {
                // Directory doesn't exist, so no files can be found.
                return [];
            }
        }

        return GetFileListInDirectory(dirInfo, originalDirectory, searchName, searchSubdirectories);
    }

    /// <summary>
    /// Adds a file to the BIG file.
    /// </summary>
    /// <param name="filePath">The path to the file to add.</param>
    /// <param name="fileInfo">The <see cref="ArchivedFileInfo"/> with the file <paramref name="filePath"/> information.</param>
    public void AddFile([NotNull] string filePath, [NotNull] ArchivedFileInfo fileInfo)
    {
        DetailedArchivedDirectoryInfo dirInfo = RootDirectoryInfo;
        var tokens = filePath.ToUpperInvariant().Split('\\', '/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            if (!dirInfo.Directories.TryGetValue(token, out DetailedArchivedDirectoryInfo? nextDir))
            {
                nextDir = new DetailedArchivedDirectoryInfo { DirectoryName = token };
                dirInfo.Directories[token] = nextDir;
            }

            dirInfo = nextDir;
        }

        dirInfo.Files[fileInfo.FileName] = fileInfo;
    }

    /// <summary>
    /// Disposes the BIG file.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed data or not.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            BaseStream?.Dispose();
            BaseStream = null;
        }

        _disposed = true;
    }

    /// <summary>
    /// Disposes the BIG file asynchronously.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (BaseStream is not null)
        {
            await BaseStream.DisposeAsync().ConfigureAwait(continueOnCapturedContext: true);
        }
    }

    /// <summary>
    /// Gets the archived file info.
    /// </summary>
    /// <param name="fileName">The file to get the information from.</param>
    /// <returns>A new <see cref="ArchivedFileInfo"/> if the file was found; <see langword="null"/> otherwise.</returns>
    protected ArchivedFileInfo? GetArchivedFileInfo([NotNull] string fileName)
    {
        DetailedArchivedDirectoryInfo dirInfo = RootDirectoryInfo;

        var tokens = fileName.ToUpperInvariant().Split('\\', '/', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < tokens.Length - 1; i++)
        {
            if (dirInfo.Directories.TryGetValue(tokens[i], out DetailedArchivedDirectoryInfo? nextDir))
            {
                dirInfo = nextDir;
            }
            else
            {
                return null;
            }
        }

        return tokens.Length > 0 && dirInfo.Files.TryGetValue(tokens[^1], out ArchivedFileInfo? fileInfo)
            ? fileInfo
            : null;
    }

    private static bool SearchStringMatches(string str, string searchString)
    {
        if (string.IsNullOrEmpty(str))
        {
            return string.IsNullOrEmpty(searchString);
        }

        if (string.IsNullOrEmpty(searchString))
        {
            return false;
        }

        // Convert glob pattern (*, ?) to Regex
        var pattern =
            "^"
            + Regex
                .Escape(searchString)
                .Replace("\\*", ".*", StringComparison.InvariantCulture)
                .Replace("\\?", ".", StringComparison.InvariantCulture)
            + "$";

        return Regex.IsMatch(str, pattern, RegexOptions.IgnoreCase);
    }

    private static string AppendPath(string path, string token) =>
        !string.IsNullOrEmpty(path)
        && !path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? path + System.IO.Path.DirectorySeparatorChar + token
            : path + token;
}
