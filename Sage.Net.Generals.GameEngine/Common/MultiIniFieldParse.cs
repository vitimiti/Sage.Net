// -----------------------------------------------------------------------
// <copyright file="MultiIniFieldParse.cs" company="Sage.Net">
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

using System.Diagnostics;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>The <see cref="MultiIniFieldParse"/> class provides functionality for parsing and managing multiple fields from INI (Initial Configuration) formatted files.</summary>
/// <remarks>It is designed to handle scenarios where multiple fields or entries need to be extracted and processed from complex or layered INI configurations.</remarks>
public class MultiIniFieldParse
{
    private const int MaxMultiFields = 16;

    private readonly FieldParse[] _fieldParse = new FieldParse[MaxMultiFields];
    private readonly uint[] _extraOffset = new uint[MaxMultiFields];

    /// <summary>Gets the number of fields parsed.</summary>
    public int Count { get; private set; }

    /// <summary>Adds a new field to the multi-field list.</summary>
    /// <param name="fieldParse">The field parse to add.</param>
    /// <param name="extraOffset">The extra offset to apply to the field.</param>
    /// <exception cref="InvalidOperationException">Thrown when the multi-field list is full.</exception>
    public void Add(FieldParse fieldParse, uint extraOffset = 0)
    {
        if (Count < MaxMultiFields)
        {
            _fieldParse[Count] = fieldParse;
            _extraOffset[Count] = extraOffset;
            Count++;
        }
        else
        {
            var message = $"Too many multi-fields ({Count}:{MaxMultiFields}) in ini file.";
            Debug.Fail(message);
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>Adds multiple fields to the multi-field list.</summary>
    /// <param name="parseTable">The parse table to add.</param>
    /// <param name="extraOffset">The extra offset to apply to the fields.</param>
    public void Add(ReadOnlySpan<FieldParse> parseTable, uint extraOffset = 0)
    {
        foreach (FieldParse field in parseTable)
        {
            Add(field, extraOffset);
        }
    }

    /// <summary>Gets the field parse for the specified index.</summary>
    /// <param name="i">The index of the field to get.</param>
    /// <returns>The field parse for the specified index.</returns>
    public FieldParse GetNthFieldParse(int i) => _fieldParse[i];

    /// <summary>Gets the extra offset for the specified index.</summary>
    /// <param name="i">The index of the extra offset to get.</param>
    /// <returns>The extra offset for the specified index.</returns>
    public uint GetNthExtraOffset(int i) => _extraOffset[i];
}
