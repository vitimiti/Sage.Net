// -----------------------------------------------------------------------
// <copyright file="LocalFileSystem.cs" company="Sage.Net">
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
using Sage.Net.GameEngine.Common.Io;

namespace Sage.Net.GameEngine.Common.Subsystems;

/// <summary>
/// Provides helper methods for interacting with the local file system, including opening files,
/// checking for existence, enumerating files, retrieving file information, creating directories,
/// and normalizing paths.
/// </summary>
public class LocalFileSystem : Subsystem, ILocalFileSystem
{
    /// <inheritdoc/>
    /// <remarks>This is a no-op.</remarks>
    public override void Initialize() { }

    /// <inheritdoc/>
    /// <remarks>This is a no-op.</remarks>
    public override void Reset() { }

    /// <inheritdoc/>
    /// <remarks>This is a no-op.</remarks>
    public override void UpdateCore() { }

    /// <summary>
    /// Opens a file at the specified <paramref name="path"/> with the provided <paramref name="access"/>.
    /// If the access allows writing, the parent directory is created if it does not exist.
    /// </summary>
    /// <param name="path">The path of the file to open.</param>
    /// <param name="access">The desired file access.</param>
    /// <returns>
    /// A <see cref="FileStream"/> for the file, or <see langword="null"/> if the path is invalid
    /// or an error occurs while opening the file.
    /// </returns>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "We want to log the error and return null."
    )]
    public FileStream? OpenFile(string path, FileAccess access)
    {
        if (string.IsNullOrEmpty(path) || Path.EndsInDirectorySeparator(path))
        {
            return null;
        }

        var isWritable = access.HasFlag(FileAccess.Write) || access.HasFlag(FileAccess.ReadWrite);
        if (isWritable)
        {
            var dir = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;
            if (!Directory.Exists(dir) && !Directory.CreateDirectory(dir).Exists)
            {
                Debug.WriteLine($"{nameof(LocalFileSystem)}.{nameof(OpenFile)} - Error creating directory {dir}");
                return null;
            }
        }

        try
        {
            return File.Open(path, isWritable ? FileMode.OpenOrCreate : FileMode.Open, access);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{nameof(LocalFileSystem)}.{nameof(OpenFile)} - Error opening file {path}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Determines whether a file exists at the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns><see langword="true"/> if the file exists; otherwise, <see langword="false"/>.</returns>
    public bool DoesFileExist(string path) => File.Exists(path);

    /// <summary>
    /// Gets a list of files under a directory that match the given <paramref name="searchName"/> pattern.
    /// </summary>
    /// <param name="currentDirectory">The relative directory to search within.</param>
    /// <param name="originalDirectory">The root directory combined with <paramref name="currentDirectory"/>.</param>
    /// <param name="searchName">The search pattern (supports wildcards, e.g., <c>*.txt</c>).</param>
    /// <param name="searchSubdirectories">Whether to search recursively in subdirectories.</param>
    /// <returns>
    /// A sequence of file paths that match the pattern, or an empty sequence if an error occurs.
    /// </returns>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "We want to log the error and return an empty list."
    )]
    public IEnumerable<string> GetFileListInDirectory(
        string currentDirectory,
        string originalDirectory,
        string searchName,
        bool searchSubdirectories
    )
    {
        var search = Path.Combine(originalDirectory, currentDirectory);
        var extension = Path.GetExtension(searchName);
        if (string.IsNullOrEmpty(extension))
        {
            search = ".";
        }

        try
        {
            return Directory.GetFiles(
                search,
                searchName,
                new EnumerationOptions
                {
                    RecurseSubdirectories = searchSubdirectories,
                    IgnoreInaccessible = true,
                    MatchCasing = MatchCasing.CaseInsensitive,
                }
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"{nameof(LocalFileSystem)}.{nameof(GetFileListInDirectory)} - Error getting file list: {ex.Message}"
            );

            return [];
        }
    }

    /// <summary>
    /// Attempts to retrieve <see cref="FileInformation"/> for the file at the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="fileInformation">
    /// When this method returns <see langword="true"/>, contains the retrieved file information;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if the file information was retrieved; otherwise, <see langword="false"/>.</returns>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "We want to log the error and return false."
    )]
    public bool TryGetFileInformation(string path, out FileInformation? fileInformation)
    {
        fileInformation = null;
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        try
        {
            var nativeFileInfo = new FileInfo(path);
            fileInformation = new FileInformation(
                (int)(nativeFileInfo.Length >> 32),
                (int)(nativeFileInfo.Length & uint.MaxValue),
                (int)(nativeFileInfo.LastWriteTimeUtc.Ticks >> 32),
                (int)(nativeFileInfo.LastWriteTimeUtc.Ticks & uint.MaxValue)
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get the file information for {path}: {ex.Message}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Ensures that the directory at <paramref name="path"/> exists by creating it if necessary.
    /// </summary>
    /// <param name="path">The path of the directory to create.</param>
    /// <returns>
    /// <see langword="true"/> if the directory exists (either already existed or was created);
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "We want to log the error and return false."
    )]
    public bool TryCreateDirectory(string path)
    {
        var result = false;
        if (string.IsNullOrEmpty(path))
        {
            return result;
        }

        try
        {
            result = Directory.CreateDirectory(path).Exists;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to create directory {path}: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Normalizes a file system path by converting mixed directory separators to the platform-specific separator.
    /// </summary>
    /// <param name="path">The path to normalize. Must not be <see langword="null"/>.</param>
    /// <returns>The normalized path string.</returns>
    /// <remarks>Does not resolve relative segments such as <c>..</c> or <c>.</c>.</remarks>
    public string NormalizePath([NotNull] string path)
    {
        var tokens = path.Split(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar,
            StringSplitOptions.RemoveEmptyEntries
        );

        return Path.Combine(tokens);
    }
}
