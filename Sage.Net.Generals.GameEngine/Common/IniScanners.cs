// -----------------------------------------------------------------------
// <copyright file="IniScanners.cs" company="Sage.Net">
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
    /// <summary>Converts a string token into a 32-bit integer.</summary>
    /// <param name="token">The string representation of the integer to be parsed.</param>
    /// <returns>The parsed 32-bit integer value.</returns>
    /// <exception cref="InvalidDataException">Thrown if the specified <paramref name="token"/> cannot be parsed as a valid integer.</exception>
    public static int ScanInt32(string token) =>
        !int.TryParse(token, out var result)
            ? throw new InvalidDataException($"Invalid integer token '{token}'")
            : result;

    /// <summary>Converts a string token into a 32-bit unsigned integer.</summary>
    /// <param name="token">The string representation of the unsigned integer to be parsed.</param>
    /// <returns>The parsed 32-bit unsigned integer value.</returns>
    /// <exception cref="InvalidDataException">Thrown if the specified <paramref name="token"/> cannot be parsed as a valid unsigned integer.</exception>
    public static uint ScanUInt32(string token) =>
        !uint.TryParse(token, out var result)
            ? throw new InvalidDataException($"Invalid integer token '{token}'")
            : result;

    /// <summary>Converts a string token into a single-precision floating-point number.</summary>
    /// <param name="token">The string representation of the float to be parsed.</param>
    /// <returns>The parsed single-precision floating-point number.</returns>
    /// <exception cref="InvalidDataException">Thrown if the specified <paramref name="token"/> cannot be parsed as a valid float.</exception>
    public static float ScanFloat(string token) =>
        !float.TryParse(token, out var result)
            ? throw new InvalidDataException($"Invalid float token '{token}'")
            : result;

    /// <summary>Converts a string token into a boolean value.</summary>
    /// <param name="token">The string representation of the boolean value to be parsed.</param>
    /// <returns>The parsed boolean value.</returns>
    /// <exception cref="InvalidDataException">Thrown if the specified <paramref name="token"/> cannot be parsed as a valid boolean value.</exception>
    public static bool ScanBool(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (token.Equals("yes", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (token.Equals("no", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var message = $"Invalid boolean token '{token}' -- Expected 'yes' or 'no'";
        Debug.Fail(message);
        throw new InvalidDataException(message);
    }
}
