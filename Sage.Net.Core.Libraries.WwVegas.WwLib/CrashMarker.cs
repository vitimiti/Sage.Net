// -----------------------------------------------------------------------
// <copyright file="CrashMarker.cs" company="Sage.Net">
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

namespace Sage.Net.Core.Libraries.WwVegas.WwLib;

/// <summary>Represents metadata and contextual information about a crash event.</summary>
/// <remarks>This class encapsulates details that are logged and utilized in crash recovery and diagnostics workflows.</remarks>
public sealed class CrashMarker
{
    /// <summary>Gets or sets the application identifier.</summary>
    public string? AppId { get; set; }

    /// <summary>Gets or sets the UTC timestamp.</summary>
    public DateTimeOffset TimestampUtc { get; set; }

    /// <summary>Gets or sets the process identifier.</summary>
    public int ProcessId { get; set; }

    /// <summary>Gets or sets the crash reason.</summary>
    public string? Reason { get; set; }

    /// <summary>Gets or sets the name of the dump file.</summary>
    public string? DumpFileName { get; set; }
}
