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

namespace Sage.Net.GameEngine.Common;

/// <summary>A delegate with how to parse a field from an INI file.</summary>
/// <param name="ini">The INI file.</param>
/// <param name="instance">The instance of what we're loading.</param>
/// <param name="store">Where to store the data parsed.</param>
/// <param name="userData">Extra user data.</param>
public delegate void IniFieldParse(Ini ini, ref object? instance, ref object? store, object? userData);

/// <summary>Represents a structure used for parsing INI file fields.</summary>
/// <param name="Token">The token used to identify the associated field in the INI file.</param>
/// <param name="Parse">A delegate function responsible for parsing the field value from the INI file.</param>
/// <param name="UserData">Additional user-defined data that may be used during parsing.</param>
/// <param name="Offset">The offset value that may represent the position or indexing of the field.</param>
public record FieldParse(string Token, IniFieldParse? Parse, object? UserData, int Offset);
