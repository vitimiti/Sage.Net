// -----------------------------------------------------------------------
// <copyright file="VersionHelper.cs" company="Sage.Net">
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

using System.Reflection;

namespace Sage.Net.GameEngine.Common;

/// <summary>The version information helper.</summary>
public static class VersionHelper
{
    private static readonly Assembly Asm = Assembly.GetExecutingAssembly();
    private static readonly Dictionary<string, string> Metadata = Asm.GetCustomAttributes<AssemblyMetadataAttribute>()
        .ToDictionary(k => k.Key, v => v.Value ?? "N/A");

    /// <summary>Gets the assembly version.</summary>
    /// <returns>A new <see cref="string"/> with the assembly version.</returns>
    public static string GetVersion() => Asm.GetName().Version?.ToString() ?? "0.0.0.0";

    /// <summary>Gets the numeric representation of the assembly version.</summary>
    /// <returns>A <see cref="uint"/> value representing the major and minor version numbers packed into a 32-bit integer.</returns>
    public static uint GetVersionNumber()
    {
        var versionStr = GetVersion();
        var majorStr = versionStr.Split('.')[0];
        var minorStr = versionStr.Split('.')[1];

        if (!uint.TryParse(majorStr, out var major))
        {
            major = 1;
        }

        if (!uint.TryParse(minorStr, out var minor))
        {
            minor = 0;
        }

        return (major << 16) | minor;
    }

    /// <summary>Gets the build date.</summary>
    /// <returns>A new <see cref="string"/> with the build date.</returns>
    public static string GetBuildDate() => Get("BuildDate");

    /// <summary>Gets the build date.</summary>
    /// <returns>A new <see cref="string"/> with the build date.</returns>
    public static string GetBuildLocation() => Get("BuildLocation");

    /// <summary>Gets the build date.</summary>
    /// <returns>A new <see cref="string"/> with the build date.</returns>
    public static string GetBuildUser() => Get("BuildUser");

    /// <summary>Gets the git revision.</summary>
    /// <returns>A new <see cref="string"/> with the git revision.</returns>
    public static string GetGitRevision() => Get("GitRevision");

    /// <summary>Gets the git version.</summary>
    /// <returns>A new <see cref="string"/> with the git version.</returns>
    public static string GetGitVersion() => Get("GitVersion");

    /// <summary>Gets the git commit time.</summary>
    /// <returns>A new <see cref="string"/> with the git commit time.</returns>
    public static string GetGitCommitTime() => Get("GitTime");

    /// <summary>Gets the git author.</summary>
    /// <returns>A new <see cref="string"/> with the git author.</returns>
    public static string GetGitAuthor() => Get("GitAuthor");

    private static string Get(string key) => Metadata.GetValueOrDefault(key, "Unknown");
}
