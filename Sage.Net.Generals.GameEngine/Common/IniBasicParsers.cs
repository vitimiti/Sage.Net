// -----------------------------------------------------------------------
// <copyright file="IniBasicParsers.cs" company="Sage.Net">
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

public partial class Ini
{
    /// <summary>Parses a byte value from the next token in the provided INI instance.</summary>
    /// <param name="ini">The <see cref="Ini"/> instance containing the data to parse.</param>
    /// <param name="instance">A reference to the object instance being initialized. Not used in this method.</param>
    /// <param name="store">A reference to the storage location for the parsed byte value.</param>
    /// <param name="userData">Optional user-provided data to assist in parsing. Not used in this method.</param>
    /// <exception cref="InvalidDataException">Thrown when the token cannot be parsed as a valid byte value or falls outside the [0,255] range.</exception>
    public static void ParseByte(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanInt32(token);
        if (value is < byte.MinValue or > byte.MaxValue)
        {
            var message = $"Invalid byte value '{token}'";
            Debug.Fail(message);
            throw new InvalidDataException(message);
        }

        store = value;
    }

    /// <summary>Parses a 16-bit signed integer value from the next token in the provided INI instance.</summary>
    /// <param name="ini">The <see cref="Ini"/> instance containing the data to parse.</param>
    /// <param name="instance">A reference to the object instance being initialized. Not used in this method.</param>
    /// <param name="store">A reference to the storage location for the parsed 16-bit signed integer value.</param>
    /// <param name="userData">Optional user-provided data to assist in parsing. Not used in this method.</param>
    /// <exception cref="InvalidDataException">Thrown when the token cannot be parsed as a valid 16-bit signed integer value or falls outside the [-32768,32767] range.</exception>
    public static void ParseInt16(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanInt32(token);
        if (value is < short.MinValue or > short.MaxValue)
        {
            var message = $"Invalid integer value '{token}'";
            Debug.Fail(message);
            throw new InvalidDataException(message);
        }

        store = value;
    }

    /// <summary>Parses an unsigned 16-bit integer value from the next token in the provided INI parser.</summary>
    /// <param name="ini">The INI parser instance from which the token will be read.</param>
    /// <param name="instance">A reference to the object being initialized or processed. Not used in this method.</param>
    /// <param name="store">A reference where the parsed unsigned 16-bit integer value will be stored.</param>
    /// <param name="userData">Optional user-defined data to be passed to the parsing process. Not used in this method.</param>
    /// <exception cref="InvalidDataException">Thrown if the token cannot be parsed as a valid unsigned 16-bit integer.</exception>
    public static void ParseUInt16(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanInt32(token);
        if (value is < ushort.MinValue or > ushort.MaxValue)
        {
            var message = $"Invalid integer value '{token}'";
            Debug.Fail(message);
            throw new InvalidDataException(message);
        }

        store = value;
    }

    /// <summary>Parses a 32-bit signed integer value from the next token in the provided INI instance.</summary>
    /// <param name="ini">The <see cref="Ini"/> instance containing the data to parse.</param>
    /// <param name="instance">A reference to the object instance being initialized. Not used in this method.</param>
    /// <param name="store">A reference to the storage location for the parsed 32-bit signed integer value.</param>
    /// <param name="userData">Optional user-provided data to assist in parsing. Not used in this method.</param>
    public static void ParseInt32(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanInt32(token);
        store = value;
    }

    /// <summary>Parses an unsigned 32-bit integer value from the next token in the provided INI instance.</summary>
    /// <param name="ini">The <see cref="Ini"/> instance containing the data to parse.</param>
    /// <param name="instance">A reference to the object instance being initialized. Not used in this method.</param>
    /// <param name="store">A reference to the storage location for the parsed unsigned 32-bit integer value.</param>
    /// <param name="userData">Optional user-provided data to assist in parsing. Not used in this method.</param>
    public static void ParseUInt32(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanUInt32(token);
        store = value;
    }

    /// <summary>Parses a string token from the provided <see cref="Ini"/> instance and converts it into a single-precision floating point value.</summary>
    /// <param name="ini">The <see cref="Ini"/> instance used to retrieve the token to be parsed.</param>
    /// <param name="instance">A reference to the current object being processed. Not used in this method.</param>
    /// <param name="store">A reference to store the parsed float value.</param>
    /// <param name="userData">Optional user-defined data passed to the method. Not used in this method.</param>
    public static void ParseFloat(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanFloat(token);
        store = value;
    }

    /// <summary>Parses the next token from the given ini object as a positive, non-zero single-precision floating point value.</summary>
    /// <param name="ini">The ini object from which the token will be retrieved.</param>
    /// <param name="instance">An optional parameter representing the instance being populated. Not used in this method.</param>
    /// <param name="store">A reference to an object where the parsed value will be stored as a float.</param>
    /// <param name="userData">Optional user-defined data. Not used in this method.</param>
    /// <exception cref="InvalidDataException">Thrown if the next token cannot be parsed as a valid float or is not a positive, non-zero value.</exception>
    public static void ParsePositiveNonZeroFloat(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanFloat(token);
        if (value <= 0F)
        {
            var message = $"Invalid positive non-zero real value '{token}'";
            Debug.Fail(message);
            throw new InvalidDataException(message);
        }

        store = value;
    }

    /// <summary>Parses a boolean value from the current token in the INI file.</summary>
    /// <param name="ini">The INI file parser instance containing the context for parsing.</param>
    /// <param name="instance">An optional reference to the object instance being parsed. Not used in this method.</param>
    /// <param name="store">A reference where the parsed boolean value will be stored.</param>
    /// <param name="userData">Optional user-defined data or context for the parsing operation. Not used in this method.</param>
    public static void ParseBool(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        store = bool.Parse(token);
    }

    /// <summary>Parses a bit flag value from the current token in the INI file.</summary>
    /// <param name="ini">The INI file parser instance containing the context for parsing.</param>
    /// <param name="instance">An optional reference to the object instance being parsed. Not used in this method.</param>
    /// <param name="store">A reference where the parsed bit flag value will be stored.</param>
    /// <param name="userData">Optional user-defined masks to add to the <paramref name="store"/> value if the next token is <see langword="true"/>; otherwise remove from the <paramref name="store"/>.</param>
    public static void ParseBitInt32(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var s = (uint)(store ?? 0U);
        var mask = (uint)(userData ?? 0U);

        if (ScanBool(ini.GetNextToken()))
        {
            s |= mask;
        }
        else
        {
            s &= ~mask;
        }

        store = s;
    }

    /// <summary>Parses an ASCII string value from the next token in the provided INI instance.</summary>
    /// <param name="ini">The <see cref="Ini"/> instance containing the data to parse.</param>
    /// <param name="instance">A reference to the object instance being initialized. Not used in this method.</param>
    /// <param name="store">A reference to the storage location for the parsed ASCII string value.</param>
    /// <param name="userData">Optional user-provided data to assist in parsing. Not used in this method.</param>
    public static void ParseAsciiString(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var asciiString = ini.GetNextAsciiString();
        store = asciiString;
    }

    /// <summary>Parses a quoted ASCII string value from the next token in the provided INI instance.</summary>
    /// <param name="ini">The <see cref="Ini"/> instance containing the data to parse.</param>
    /// <param name="instance">A reference to the object instance being initialized. Not used in this method.</param>
    /// <param name="store">A reference to the storage location for the parsed ASCII string value.</param>
    /// <param name="userData">Optional user-provided data to assist in parsing. Not used in this method.</param>
    public static void ParseQuotedAsciiString(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var asciiString = ini.GetNextQuotedAsciiString();
        store = asciiString;
    }

    /// <summary>Parses a list of ASCII string values from the sequential tokens in the provided INI instance.</summary>
    /// <param name="ini">The <see cref="Ini"/> instance containing the data to parse.</param>
    /// <param name="instance">A reference to the object instance being initialized. Not used in this method.</param>
    /// <param name="store">A reference to the storage location for the parsed list of ASCII strings. This will be cleared and populated with the parsed values.</param>
    /// <param name="userData">Optional user-provided data to assist in parsing. Not used in this method.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="ini"/> parameter is null.</exception>
    public static void ParseAsciiStringList(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        List<string> asl = store as List<string> ?? [];
        asl.Clear();
        for (var token = ini.GetNextTokenOrNull(); token != null; token = ini.GetNextTokenOrNull())
        {
            asl.Add(token);
        }

        store = asl;
    }

    /// <summary>Appends ASCII strings from the provided INI instance to the existing string list in the storage location.</summary>
    /// <param name="ini">The <see cref="Ini"/> instance used to read and parse tokens.</param>
    /// <param name="instance">A reference to the object instance being initialized. Not used in this method.</param>
    /// <param name="store">A reference to the storage location where the parsed ASCII strings list will be appended. Expected to be a <see cref="List{String}"/>.</param>
    /// <param name="userData">Optional user-provided data to assist in parsing. Not used in this method.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="ini"/> is null.</exception>
    public static void ParseAsciiStringListAppend(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        List<string> asl = store as List<string> ?? [];
        for (var token = ini.GetNextTokenOrNull(); token != null; token = ini.GetNextTokenOrNull())
        {
            asl.Add(token);
        }

        store = asl;
    }
}
