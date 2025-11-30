// -----------------------------------------------------------------------
// <copyright file="AddonCompat.cs" company="Sage.Net">
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

namespace Sage.Net.Core.GameEngine.Common;

/// <summary>
/// Addon compatibility methods.
/// </summary>
public static class AddonCompat
{
    /// <summary>
    /// Checks if the full viewport data file exists.
    /// </summary>
    /// <param name="gamePath">The path to the game directory, or <see langword="null"/> to use the working directory.</param>
    /// <returns><see langword="true"/> if the full viewpoert data file exists AND had data in it; otherwise <see langword="false"/>.</returns>
    public static bool HasFullViewportDataFile(string? gamePath)
    {
        var filePath = Path.Combine(gamePath ?? Environment.CurrentDirectory, "GenTool", "fullviewport.dat");
        if (!File.Exists(filePath))
        {
            return false;
        }

        using FileStream fileStream = File.OpenRead(filePath);
        return fileStream.Length > 0;
    }
}
