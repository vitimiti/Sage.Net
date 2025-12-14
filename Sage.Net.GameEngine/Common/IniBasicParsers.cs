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
using Sage.Net.GameEngine.Common.Exceptions.IniExceptions;

namespace Sage.Net.GameEngine.Common;

public partial class Ini
{
    /// <summary>Parses a byte value from the current line.</summary>
    /// <param name="ini">The <see cref="Ini"/> file to parse from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> where the resulting byte value will be stored.</param>
    /// <param name="userData">The user data is unused.</param>
    /// <exception cref="IniInvalidDataException">When the parsed value was not a <see cref="byte"/>.</exception>
    public static void ParseByte(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanInt32(token);
        if (value is < 0 or > 255)
        {
            var message = $"[LINE: {ini.LineNumber} - FILE: '{ini.FileName}'] Invalid byte value '{token}'.";
            Debug.Fail(message);
            throw new IniInvalidDataException(message);
        }

        store = (byte)value;
    }

    /// <summary>Parses a <see cref="short"/> value from the current line.</summary>
    /// <param name="ini">The <see cref="Ini"/> file to parse from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> where the resulting <see cref="short"/> value will be stored.</param>
    /// <param name="userData">The user data is unused.</param>
    /// <exception cref="IniInvalidDataException">When the parsed value was not a <see cref="short"/>.</exception>
    public static void ParseInt16(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanInt32(token);
        if (value is < -32768 or > 32767)
        {
            var message = $"[LINE: {ini.LineNumber} - FILE: '{ini.FileName}'] Invalid short value '{token}'.";
            Debug.Fail(message);
            throw new IniInvalidDataException(message);
        }

        store = (short)value;
    }

    /// <summary>Parses an <see cref="ushort"/> value from the current line.</summary>
    /// <param name="ini">The <see cref="Ini"/> file to parse from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> where the resulting <see cref="ushort"/> value will be stored.</param>
    /// <param name="userData">The user data is unused.</param>
    /// <exception cref="IniInvalidDataException">When the parsed value was not an <see cref="ushort"/>.</exception>
    public static void ParseUInt16(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanInt32(token);
        if (value is < 0 or > 65535)
        {
            var message = $"[LINE: {ini.LineNumber} - FILE: '{ini.FileName}'] Invalid ushort value '{token}'.";
            Debug.Fail(message);
            throw new IniInvalidDataException(message);
        }

        store = (ushort)value;
    }

    /// <summary>Parses an <see cref="int"/> value from the current line.</summary>
    /// <param name="ini">The <see cref="Ini"/> file to parse from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> where the resulting <see cref="int"/> value will be stored.</param>
    /// <param name="userData">The user data is unused.</param>
    public static void ParseInt32(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanInt32(token);
        store = value;
    }

    /// <summary>Parses an <see cref="uint"/> value from the current line.</summary>
    /// <param name="ini">The <see cref="Ini"/> file to parse from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> where the resulting <see cref="uint"/> value will be stored.</param>
    /// <param name="userData">The user data is unused.</param>
    public static void ParseUInt32(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanUInt32(token);
        store = value;
    }

    /// <summary>Parses a <see cref="float"/> value from the current line.</summary>
    /// <param name="ini">The <see cref="Ini"/> file to parse from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> where the resulting <see cref="float"/> value will be stored.</param>
    /// <param name="userData">The user data is unused.</param>
    public static void ParseSingle(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanSingle(token);
        store = value;
    }

    /// <summary>Parses a <see cref="float"/> value from the current line that is positive and non-zero.</summary>
    /// <param name="ini">The <see cref="Ini"/> file to parse from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> where the resulting <see cref="float"/> value will be stored.</param>
    /// <param name="userData">The user data is unused.</param>
    /// <exception cref="IniInvalidDataException">When the parsed value was not a positive non-zero <see cref="float"/>.</exception>
    public static void ParsePositiveNonZeroSingle(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanSingle(token);
        if (value <= 0f)
        {
            var message =
                $"[LINE: {ini.LineNumber} - FILE: '{ini.FileName}'] Invalid positive non-zero single value '{token}'.";

            Debug.Fail(message);
            throw new IniInvalidDataException(message);
        }

        store = value;
    }

    /// <summary>Parses a <see cref="float"/> value from the current line as a degree value (0 to 360) and store the radian value of that degree.</summary>
    /// <param name="ini">The <see cref="Ini"/> file to parse from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> where the resulting radian value will be stored.</param>
    /// <param name="userData">The user data is unused.</param>
    public static void ParseAngleSingle(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        const float radsPerDegree = float.Pi / 180F;

        var token = ini.GetNextToken();
        var value = ScanSingle(token);
        store = value * radsPerDegree;
    }

    /// <summary>Parses a <see cref="float"/> value from the current line as angular velocity in degrees per second and stores the radians per frame value of that velocity.</summary>
    /// <param name="ini">The <see cref="Ini"/> file to parse from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> where the resulting radian per frame value will be stored.</param>
    /// <param name="userData">The user data is unused.</param>
    public static void ParseAngularVelocitySingle(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        var value = ScanSingle(token);
        var converted = GameCommon.ConvertAngularVelocityInDegreesPerSecondToRadiansPerFrame(value);
        store = converted;
    }

    /// <summary>Parses a <see cref="bool"/> value from the current line.</summary>
    /// <param name="ini">The <see cref="Ini"/> file to parse from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> where the resulting <see cref="bool"/> value will be stored.</param>
    /// <param name="userData">The user data is unused.</param>
    public static void ParseBool(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var token = ini.GetNextToken();
        store = ScanBool(token);
    }

    /// <summary>Parses a <see cref="bool"/> token from the current line and sets or clears the specified bit in a 32-bit bitfield.</summary>
    /// <param name="ini">The <see cref="Ini"/> reader to read the next token from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> holding the 32-bit bitfield to update and store back.</param>
    /// <param name="userData">An <see cref="uint"/> mask indicating the bit(s) to set or clear.</param>
    public static void ParseBitInInt32(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var s = (uint)(store ?? 0);
        var mask = (uint)(userData ?? 0);
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

    /// <summary>Parses the next ASCII string token from the INI file and stores it in the provided storage object.</summary>
    /// <param name="ini">The <see cref="Ini"/> reader to read the next token from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> holding the ASCII string to update and store back.</param>
    /// <param name="userData">The user data is unused.</param>
    public static void ParseAsciiString(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var asciiString = ini.GetNextAsciiString();
        store = asciiString;
    }

    /// <summary>Parses the next quoted ASCII string token from the INI file and stores it in the provided storage object.</summary>
    /// <param name="ini">The <see cref="Ini"/> reader to read the next token from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> holding the quoted ASCII string to update and store back.</param>
    /// <param name="userData">The user data is unused.</param>
    public static void ParseQuotedAsciiString(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        var asciiString = ini.GetNextQuotedAsciiString();
        store = asciiString;
    }

    /// <summary>Parses a list of ASCII string tokens from the INI file and stores it in the provided storage object.</summary>
    /// <param name="ini">The <see cref="Ini"/> reader to read the next token from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> holding the list of ASCII strings to update and store back. This will destroy (clear) any data passed as a <see cref="IList{T}"/> of <see cref="string"/>.</param>
    /// <param name="userData">The user data is unused.</param>
    public static void ParseAsciiStringList(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);
        IList<string> list = store as IList<string> ?? [];
        list.Clear();
        for (var token = ini.GetNextTokenOrNull(); token is not null; token = ini.GetNextTokenOrNull())
        {
            list.Add(token);
        }

        store = list;
    }

    /// <summary>Parses a list of ASCII string tokens from the INI file and stores it in the provided storage object.</summary>
    /// <param name="ini">The <see cref="Ini"/> reader to read the next token from.</param>
    /// <param name="instance">The instance is unused.</param>
    /// <param name="store">The target <see cref="object"/> holding the list of ASCII string to update and store back. This will append to any existing data passed as a <see cref="IList{T}"/> of <see cref="string"/>.</param>
    /// <param name="userData">The user data is unused.</param>
    public static void ParseAsciiStringListAppend(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);
        IList<string> list = store as IList<string> ?? [];
        for (var token = ini.GetNextTokenOrNull(); token is not null; token = ini.GetNextTokenOrNull())
        {
            list.Add(token);
        }

        store = list;
    }
}
