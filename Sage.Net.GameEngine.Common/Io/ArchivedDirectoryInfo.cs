// -----------------------------------------------------------------------
// <copyright file="ArchivedDirectoryInfo.cs" company="Sage.Net">
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

namespace Sage.Net.GameEngine.Common.Io;

/// <summary>
/// Represents a directory entry inside an archive, including its path, name,
/// child directories, and files contained within it.
/// </summary>
public record ArchivedDirectoryInfo
{
    /// <summary>
    /// Gets or sets the full path of this directory within the archive.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the directory name (the last segment of <see cref="Path"/>).
    /// </summary>
    public string DirectoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the immediate child directories of this directory, keyed by directory name.
    /// </summary>
    public Dictionary<string, ArchivedDirectoryInfo> Directories { get; } = [];

    /// <summary>
    /// Gets the files contained directly in this directory, keyed by file name.
    /// </summary>
    public Dictionary<string, ArchiveFile> Files { get; } = [];
}
