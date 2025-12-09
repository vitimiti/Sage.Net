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

namespace Sage.Net.GameEngine.Common;

/// <summary>A record representing information about an archived directory.</summary>
/// <param name="Path">A <see cref="string"/> with the archived directory path.</param>
/// <param name="DirectoryName">A <see cref="string"/> with the archived directory name.</param>
/// <param name="Directories">A <see cref="Dictionary{TKey,TValue}"/> of <see cref="string"/> keys and <see cref="ArchivedDirectoryInfo"/> values with the list of directories in the archived directory.</param>
/// <param name="Files">A <see cref="Dictionary{TKey,TValue}"/> of <see cref="string"/> keys and <see cref="ArchiveFile"/> values with the list of files in the archived directory.</param>
public record ArchivedDirectoryInfo(
    string Path,
    string DirectoryName,
    Dictionary<string, ArchivedDirectoryInfo> Directories,
    Dictionary<string, ArchiveFile?> Files
);
