// -----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Sage.Net">
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

namespace Sage.Net.Io.Extensions;

/// <summary>
/// Provides extension methods for string manipulation and processing.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Resolves a file system path that begins with a tilde (~) to its absolute form.
    /// The tilde (~) is expanded to the user's home directory. If the input path does not
    /// start with a tilde or is null/whitespace, the original path is returned unchanged.
    /// </summary>
    /// <param name="tildePath">
    /// The file system path to resolve. If the path starts with a tilde (~), it is replaced
    /// with the current user's home directory. May be null or whitespace.
    /// </param>
    /// <returns>
    /// A string representing the resolved absolute path, or the original string if it does
    /// not start with a tilde or is null/whitespace.
    /// </returns>
    public static string? ResolveTildePath(this string? tildePath)
    {
#pragma warning disable IDE0046 // Convert to conditional expression
        if (string.IsNullOrWhiteSpace(tildePath))
#pragma warning restore IDE0046 // Convert to conditional expression
        {
            return tildePath;
        }

        return tildePath.StartsWith('~')
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                tildePath.TrimStart('~', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            )
            : tildePath;
    }
}
