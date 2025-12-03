// -----------------------------------------------------------------------
// <copyright file="IniParser.cs" company="Sage.Net">
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
using System.Diagnostics.CodeAnalysis;
using Sage.Net.Core.GameEngine.Common.Ini.IniExceptions;

namespace Sage.Net.Core.GameEngine.Common.Ini;

/// <summary>
/// INI parser methods.
/// </summary>
public static class IniParser
{
    /// <summary>
    /// Parse a <see cref="string"/>.
    /// </summary>
    /// <param name="ini">The <see cref="IniReader"/> instance.</param>
    /// <param name="instance">The instance.</param>
    /// <param name="store">The store.</param>
    /// <param name="userData">The user data.</param>
    /// <remarks>
    /// <para>This parser expects <paramref name="store"/> to be a <see cref="string"/>.</para>
    /// <para>This parser does not use <paramref name="instance"/> or <paramref name="userData"/>.</para>
    /// </remarks>
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "This is for the FieldParse delegate.")]
    public static void ParseString(
        [NotNull] IniReader ini,
        ref object? instance,
        ref object? store,
        object? userData
    ) => store = ini.GetNextString();

    /// <summary>
    /// Parse an <see cref="int"/>.
    /// </summary>
    /// <param name="ini">The <see cref="IniReader"/> instance.</param>
    /// <param name="instance">The instance.</param>
    /// <param name="store">The store.</param>
    /// <param name="userData">The user data.</param>
    /// <remarks>
    /// <para>This parser expects <paramref name="store"/> to be a <see cref="int"/>.</para>
    /// <para>This parser does not use <paramref name="instance"/> or <paramref name="userData"/>.</para>
    /// </remarks>
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "This is for the FieldParse delegate.")]
    public static void ParseInt32([NotNull] IniReader ini, ref object? instance, ref object? store, object? userData) =>
        store = IniReader.ScanInt32(ini.GetNextToken());

    /// <summary>
    /// Parse a string as a bitstring.
    /// </summary>
    /// <param name="ini">The <see cref="IniReader"/> instance.</param>
    /// <param name="instance">The instance.</param>
    /// <param name="store">The store.</param>
    /// <param name="userData">The user data.</param>
    /// <remarks>
    /// <para>This parser expects <paramref name="store"/> to be a <see cref="int"/>.</para>
    /// <para>This parser expects <paramref name="userData"/> to be a <see cref="List{T}"/> of <see cref="string"/>s.</para>
    /// <para>This parser does not use <paramref name="instance"/>.</para>
    /// </remarks>
    /// <exception cref="IniInvalidNameListException">Flag list is null or empty.</exception>
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "This is for the FieldParse delegate.")]
    public static void ParseBitString32(
        [NotNull] IniReader ini,
        ref object? instance,
        ref object? store,
        object? userData
    )
    {
        if (userData is not List<string> flagList || flagList.Count == 0)
        {
            const string message = "Flag list is null or empty.";
            Debug.Fail(message);
            throw new IniInvalidNameListException(message);
        }

        var foundNormal = false;
        var foundAddOrSub = false;
        const string generalMessage = "You may NOT mix normal and +/- ops in bitstring lists";

        var bits = (int)(store ?? 0);
        for (var token = ini.GetNextTokenOrNull(); token is not null; token = ini.GetNextTokenOrNull())
        {
            if (token.Equals("NONE", StringComparison.OrdinalIgnoreCase))
            {
                if (foundNormal || foundAddOrSub)
                {
                    Debug.Fail(generalMessage);
                    throw new IniInvalidNameListException(generalMessage);
                }

                break;
            }

            ProcessBitStringToken(token, flagList, ref bits, ref foundNormal, ref foundAddOrSub);
        }

        store = bits;
    }

    private static void ProcessBitStringToken(
        string token,
        List<string> flagList,
        ref int bits,
        ref bool foundNormal,
        ref bool foundAddOrSub
    )
    {
        const string generalMessage = "You may NOT mix normal and +/- ops in bitstring lists";

        switch (token[0])
        {
            case '+' when foundNormal:
                Debug.Fail(generalMessage);
                throw new IniInvalidNameListException(generalMessage);
            case '+':
            {
                var bitIndex = IniReader.ScanIndexList(token[1..], flagList);
                bits |= 1 << bitIndex;
                foundAddOrSub = true;
                break;
            }

            case '-' when foundNormal:
                Debug.Fail(generalMessage);
                throw new IniInvalidNameListException(generalMessage);
            case '-':
            {
                var bitIndex = IniReader.ScanIndexList(token[1..], flagList);
                bits &= ~(1 << bitIndex);
                foundAddOrSub = true;
                break;
            }

            default:
            {
                if (foundAddOrSub)
                {
                    Debug.Fail(generalMessage);
                    throw new IniInvalidNameListException(generalMessage);
                }

                if (!foundNormal)
                {
                    bits = 0;
                }

                var bitIndex = IniReader.ScanIndexList(token, flagList);
                bits |= 1 << bitIndex;
                foundNormal = true;
                break;
            }
        }
    }
}
