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

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>A record representing the INI field being parsed.</summary>
/// <param name="Token">The token representing the field.</param>
/// <param name="Parse">The process used to parse the field.</param>
/// <param name="UserData">User data.</param>
/// <param name="Offset">The offset of the field within the instance.</param>
public record FieldParse(string Token, Ini.FieldParseProcess Parse, object? UserData, int Offset);
