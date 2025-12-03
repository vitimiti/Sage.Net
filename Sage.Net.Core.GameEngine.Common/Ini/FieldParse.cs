// -----------------------------------------------------------------------
// <copyright file="FieldParse.cs" company="Sage.Net">
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

namespace Sage.Net.Core.GameEngine.Common.Ini;

/// <summary>
/// Represents a process delegate used for parsing fields in an INI file using a custom handler.
/// </summary>
/// <param name="ini">The <see cref="IniReader"/> instance used to read the INI data.</param>
/// <param name="instance">A reference to an object instance to be modified based on the parsed field data.</param>
/// <param name="store">A reference to an object used to store additional field data or parsed results.</param>
/// <param name="userData">Optional user-defined data passed to the delegate for custom processing.</param>
public delegate void FieldParseProcess(
    [NotNull] IniReader ini,
    ref object? instance,
    ref object? store,
    object? userData
);

/// <summary>
/// Represents a record used to define a field parsing operation for an INI file. Each instance of this
/// record specifies the token name, optional user-defined data, and parsing behavior for a field.
/// </summary>
/// <param name="Token">
/// The name of the token to match in the INI file. If the token matches during parsing, the associated
/// parsing logic is executed.
/// </param>
/// <param name="UserData">
/// Optional user-defined data that can be utilized during the parsing process for additional context or instructions.
/// </param>
/// <param name="Parse">
/// A delegate that defines the logic for parsing the field. The delegate provides hooks to modify an object instance,
/// store parsed data, and access custom user data.
/// </param>
public record FieldParse(string? Token, object? UserData, FieldParseProcess? Parse);
