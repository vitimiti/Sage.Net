// -----------------------------------------------------------------------
// <copyright file="AddonCompatibility.cs" company="Sage.Net">
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
using System.Text;

namespace Sage.Net.Core.GameEngine.Common;

/// <summary>Provides functionality to determine the compatibility of addons by verifying the presence and validity of specific data files.</summary>
public static class AddonCompatibility
{
    /// <summary>Determines whether the "fullviewport.dat" file exists and contains valid data.</summary>
    /// <returns><see langword="true"/> if the "fullviewport.dat" file exists and its first byte is not equal to the byte representation of '0'; otherwise, <see langword="false"/>.</returns>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "We don't need to get errors, just the existence of the file with data in it."
    )]
    public static bool HasFullViewportDat()
    {
        var datFile = Path.Combine("GenTool", "fullviewport.dat");
        if (!File.Exists(datFile))
        {
            return false;
        }

        byte value;
        try
        {
            using FileStream file = File.OpenRead(datFile);
            if (file.Length < 1)
            {
                return false;
            }

            using BinaryReader br = new(file, Encoding.Default, leaveOpen: true);
            value = br.ReadByte();
        }
        catch
        {
            return false;
        }

        return value != (byte)'0';
    }
}
