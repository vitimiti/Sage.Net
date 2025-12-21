// -----------------------------------------------------------------------
// <copyright file="BigArchiveEntry.cs" company="Sage.Net">
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

namespace Sage.Net.Io.Big;

/// <summary>
/// Represents an entry in a Big archive file.
/// </summary>
/// <remarks>
/// A Big archive is a file format used for packaging data such as files or resources
/// in a compressed archive. This record encapsulates the metadata for a specific file
/// contained within the archive.
/// </remarks>
/// <param name="FileName">
/// The name of the file within the Big archive.
/// </param>
/// <param name="InternalPath">
/// The internal path of the file within the archive.
/// </param>
/// <param name="Offset">
/// The byte offset in the archive file where this entry's data starts.
/// </param>
/// <param name="Size">
/// The size in bytes of the file data within the archive.
/// </param>
public record BigArchiveEntry(string FileName, string InternalPath, uint Offset, uint Size);
