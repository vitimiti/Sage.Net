// -----------------------------------------------------------------------
// <copyright file="IniInitializers.cs" company="Sage.Net">
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

public partial class Ini
{
    /// <summary>Initializes the specified object from the current INI data stream.</summary>
    /// <param name="what">The object to be initialized.</param>
    /// <param name="parseTable">The parse table containing the field names and their corresponding parsing processes.</param>
    public void InitializeFromIni(ref object what, ReadOnlySpan<FieldParse> parseTable)
    {
        MultiIniFieldParse p = new();
        p.Add(parseTable);
        InitializeFromIniMulti(ref what, p);
    }

    /// <summary>Initializes the specified object from the current INI data stream.</summary>
    /// <param name="what">The object to be initialized.</param>
    /// <param name="process">The delegate containing the field names and their corresponding parsing processes.</param>
    public void InitializeFromIniMultiProcess(ref object what, BuildMultiIniField process)
    {
        ArgumentNullException.ThrowIfNull(process);

        MultiIniFieldParse p = new();
        process(p);
        InitializeFromIniMulti(ref what, p);
    }

    /// <summary>Initializes the specified object from the current INI data stream.</summary>
    /// <param name="what">The object to be initialized.</param>
    /// <param name="parseTable">The parse table containing the field names and their corresponding parsing processes.</param>
    /// <exception cref="InvalidOperationException">Thrown if the specified object cannot be initialized from the current INI data stream.</exception>
    /// <exception cref="InvalidDataException">Thrown if the specified object cannot be parsed from the current INI data stream.</exception>
    public void InitializeFromIniMulti(ref object what, MultiIniFieldParse parseTable)
    {
        ArgumentNullException.ThrowIfNull(what);
        ArgumentNullException.ThrowIfNull(parseTable);

        while (true)
        {
            var line = ReadLine();

            // EOF before encountering END?
            if (EndOfFile && string.IsNullOrWhiteSpace(line))
            {
                ThrowMissingEndToken();
            }

            if (!TryGetFirstToken(line, out var field))
            {
                continue;
            }

            if (IsEndToken(field))
            {
                return;
            }

            if (!TryParseKnownField(field, ref what, parseTable))
            {
                ThrowUnknownField(field);
            }

            // Match original "sanity check": reached EOF without closing end token
            if (EndOfFile)
            {
                ThrowMissingEndToken();
            }
        }
    }
}
