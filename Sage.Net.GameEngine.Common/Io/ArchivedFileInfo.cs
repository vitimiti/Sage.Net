// -----------------------------------------------------------------------
// <copyright file="ArchivedFileInfo.cs" company="Sage.Net">
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
/// Represents a file entry inside an archive, including its name, the archive it belongs to,
/// and its location and length within that archive.
/// </summary>
public record ArchivedFileInfo
{
    /// <summary>
    /// Gets or sets the file name (without any directory path) as stored in the archive.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the physical archive file name that contains this entry.
    /// Typically this is the path or name of the <c>.big</c>/<c>.mix</c> (or similar) archive file.
    /// </summary>
    public string ArchiveFileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the byte offset within the archive where this file's data begins.
    /// </summary>
    public uint Offset { get; set; }

    /// <summary>
    /// Gets or sets the size of the file data in bytes inside the archive.
    /// </summary>
    public uint Size { get; set; }
}
