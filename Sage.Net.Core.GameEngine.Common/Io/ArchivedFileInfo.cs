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

namespace Sage.Net.Core.GameEngine.Common.Io;

/// <summary>
/// Represents information about a file archived within a container.
/// </summary>
/// <param name="FileName">The name of the file stored in the archive.</param>
/// <param name="ArchiveFileName">The name of the archive that contains the file.</param>
/// <param name="Offset">The offset location of the file within the archive, measured in bytes.</param>
/// <param name="Size">The size of the file in bytes.</param>
public record ArchivedFileInfo(string FileName, string ArchiveFileName, uint Offset, uint Size);
