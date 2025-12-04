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
using System.Text.RegularExpressions;

namespace Sage.Net.GameEngine.Common.Io;

/// <summary>
/// Represents an abstract archive file capable of listing and opening files within
/// a virtual directory tree. Concrete implementations provide access to a specific
/// archive format.
/// </summary>
public abstract class ArchiveFile
{
    /// <summary>
    /// Gets the file name of the underlying archive (without path).
    /// </summary>
    public abstract string FileName { get; }

    /// <summary>
    /// Gets the full path of the underlying archive file.
    /// </summary>
    public abstract string FilePath { get; }

    /// <summary>
    /// Gets or sets the base stream that backs this archive.
    /// Implementations may use this stream to read from the archive.
    /// </summary>
    protected Stream? BaseStream { get; set; }

    /// <summary>
    /// Gets or sets the root directory information for the archive's virtual file system.
    /// </summary>
    protected DetailedArchivedDirectoryInfo? RootDirectory { get; set; }

    /// <summary>
    /// Gets a sorted set of file names within the specified directory of the archive.
    /// </summary>
    /// <param name="dirInfo">The starting directory info within the archive.</param>
    /// <param name="pattern">A wildcard pattern (supports '*' and '?') to filter file names.</param>
    /// <param name="recursive">If <see langword="true"/>, includes files from subdirectories.</param>
    /// <returns>A case-insensitive sorted set of matching file paths relative to <paramref name="dirInfo"/>.</returns>
    public static SortedSet<string> GetFileListInDirectory(
        [NotNull] DetailedArchivedDirectoryInfo dirInfo,
        string pattern,
        bool recursive
    )
    {
        var results = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        CollectFiles(dirInfo, string.Empty, pattern, recursive, results);
        return results;
    }

    /// <summary>
    /// Tries to get a <see cref="FileInfo"/> for a file from the archive on the physical file system.
    /// </summary>
    /// <param name="fileName">The path to the file inside the archive.</param>
    /// <param name="fileInfo">When this method returns, contains the <see cref="FileInfo"/> if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the file exists and the info was retrieved; otherwise, <see langword="false"/>.</returns>
    public abstract bool TryGetFileInfo(string fileName, out FileInfo? fileInfo);

    /// <summary>
    /// Opens a stream for a file inside the archive.
    /// </summary>
    /// <param name="fileName">The path to the file inside the archive.</param>
    /// <param name="access">The requested file access.</param>
    /// <param name="mode">The requested file mode.</param>
    /// <returns>A readable and/or writable <see cref="Stream"/>, or <see langword="null"/> if the file cannot be opened.</returns>
    public abstract Stream? OpenFile(string fileName, FileAccess access, FileMode mode);

    /// <summary>
    /// Closes all open file handles associated with this archive.
    /// </summary>
    public abstract void CloseAllFiles();

    /// <summary>
    /// Sets the search priority used when resolving files across multiple archives.
    /// The meaning of the value is implementation-specific, but higher values typically indicate higher priority.
    /// </summary>
    /// <param name="priority">An integer priority value.</param>
    public abstract void SetSearchPriority(int priority);

    /// <summary>
    /// Closes this archive and releases any resources it holds.
    /// </summary>
    public abstract void Close();

    /// <summary>
    /// Attaches the provided stream as the backing stream for this archive, disposing any previous one.
    /// </summary>
    /// <param name="fileStream">The stream to attach, or <see langword="null"/> to detach.</param>
    public void AttachFile(Stream? fileStream)
    {
        BaseStream?.Dispose();
        BaseStream = fileStream;
    }

    /// <summary>
    /// Gets a sorted set of file names within the specified directory of the archive.
    /// </summary>
    /// <param name="directory">The directory path inside the archive.</param>
    /// <param name="pattern">A wildcard pattern (supports '*' and '?') to filter file names.</param>
    /// <param name="recursive">If <see langword="true"/>, includes files from subdirectories.</param>
    /// <returns>A case-insensitive sorted set of matching file paths relative to the provided directory.</returns>
    public SortedSet<string> GetFileListInDirectory([NotNull] string directory, string pattern, bool recursive)
    {
        var results = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        if (RootDirectory is null)
        {
            return results;
        }

        DetailedArchivedDirectoryInfo? dirInfo = ResolveDirectory(RootDirectory, directory);
        if (dirInfo is null)
        {
            return results;
        }

        CollectFiles(dirInfo, directory, pattern, recursive, results);
        return results;
    }

    /// <summary>
    /// Adds the file information to the archive's virtual directory tree at the specified path.
    /// Missing directories in the path are created on the fly.
    /// </summary>
    /// <param name="path">The directory path inside the archive where the file should be placed.</param>
    /// <param name="fileInfo">The file metadata to add.</param>
    [SuppressMessage(
        "Globalization",
        "CA1308:Normalize strings to uppercase",
        Justification = "This is how the original does it."
    )]
    public void AddFile([NotNull] string path, [NotNull] ArchivedFileInfo fileInfo)
    {
        RootDirectory ??= new DetailedArchivedDirectoryInfo();
        DetailedArchivedDirectoryInfo dirInfo = RootDirectory;

        var tokens = path.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (var raw in tokens)
        {
            var token = raw.ToLowerInvariant();
            if (!dirInfo.Directories.TryGetValue(token, out DetailedArchivedDirectoryInfo? next))
            {
                next = new DetailedArchivedDirectoryInfo { DirectoryName = token };
                dirInfo.Directories[token] = next;
            }

            dirInfo = next;
        }

        dirInfo.Files[fileInfo.FileName] = fileInfo;
    }

    /// <summary>
    /// Gets the archived file information for the given file name within the archive.
    /// </summary>
    /// <param name="fileName">The path to the file inside the archive.</param>
    /// <returns>The <see cref="ArchivedFileInfo"/> if found; otherwise, <see langword="null"/>.</returns>
    protected ArchivedFileInfo? GetArchivedFileInfo(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || RootDirectory is null)
        {
            return null;
        }

        var tokens = fileName.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries
        );

        if (tokens.Length == 0)
        {
            return null;
        }

        DetailedArchivedDirectoryInfo dirInfo;
        if (tokens.Length == 1)
        {
            dirInfo = RootDirectory;
        }
        else
        {
            var dirPath = string.Join(Path.DirectorySeparatorChar, tokens[..^1]);
            DetailedArchivedDirectoryInfo? resolved = ResolveDirectory(RootDirectory, dirPath);
            if (resolved is null)
            {
                return null;
            }

            dirInfo = resolved;
        }

        var fileToken = tokens[^1];
        return dirInfo.Files.TryGetValue(fileToken, out ArchivedFileInfo? info)
            ? info
            : (
                from kvp in dirInfo.Files
                where string.Equals(kvp.Key, fileToken, StringComparison.OrdinalIgnoreCase)
                select kvp.Value
            ).FirstOrDefault();
    }

    private static DetailedArchivedDirectoryInfo? ResolveDirectory(DetailedArchivedDirectoryInfo root, string directory)
    {
        DetailedArchivedDirectoryInfo dirInfo = root;
        var tokens = directory.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (var raw in tokens)
        {
            var token = raw;
            if (!dirInfo.Directories.TryGetValue(token, out DetailedArchivedDirectoryInfo? next))
            {
                next = dirInfo
                    .Directories.FirstOrDefault(kvp =>
                        string.Equals(kvp.Key, token, StringComparison.OrdinalIgnoreCase)
                    )
                    .Value;
                if (next is null)
                {
                    return null;
                }
            }

            dirInfo = next;
        }

        return dirInfo;
    }

    private static bool WildcardMatch(string input, string pattern)
    {
        if (string.IsNullOrEmpty(pattern) || pattern == "*")
        {
            return true;
        }

        var regex =
            $"^{Regex.Escape(pattern).Replace("\\*", ".*", StringComparison.OrdinalIgnoreCase).Replace("\\?", ".", StringComparison.OrdinalIgnoreCase)}$";

        return Regex.IsMatch(input, regex, RegexOptions.IgnoreCase);
    }

    private static void CollectFiles(
        DetailedArchivedDirectoryInfo dirInfo,
        string currentDirectory,
        string pattern,
        bool recursive,
        SortedSet<string> results
    )
    {
        if (recursive)
        {
            foreach (DetailedArchivedDirectoryInfo tempDirInfo in dirInfo.Directories.Values)
            {
                var tempDirName =
                    $"{Path.Combine(currentDirectory, tempDirInfo.DirectoryName)}{Path.DirectorySeparatorChar}";

                CollectFiles(tempDirInfo, tempDirName, pattern, recursive, results);
            }
        }

#pragma warning disable IDE0055 // Fix formatting
        foreach (
            var fullPath in from fileKvp in dirInfo.Files
            select fileKvp.Value.FileName into fileName
            where WildcardMatch(fileName, pattern)
            select $"{Path.Combine(currentDirectory, fileName)}{Path.DirectorySeparatorChar}"
        )
        {
            _ = results.Add(fullPath);
        }
#pragma warning restore IDE0055 // Fix formatting
    }
}
