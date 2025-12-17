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

using System.Globalization;
using System.Reflection;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>The VersionHelper class provides utilities for managing and retrieving versioning information, build metadata, and Git-specific details for a software project.</summary>
/// <remarks>It allows configuring version components, extracting build information such as user and location, and generating string representations of version and build data.</remarks>
public class VersionHelper
{
    private int _major = 1;
    private int _minor;
    private int _buildNum;
    private int _localBuildNum;
    private string _buildTime = string.Empty;
    private string _buildDate = string.Empty;

    /// <summary>Initializes a new instance of the <see cref="VersionHelper"/> class.</summary>
    public VersionHelper()
    {
        StringGitCommitCount = BuildGitCommitCount();
        StringGitTagOrHash = BuildGitTagOrHash();
        StringGitCommitTime = BuildGitCommitTime();

#if RTS_DEBUG
        ShowFullVersion = true;
#endif
    }

    /// <summary>Gets or sets the singleton instance of the version helper.</summary>
    public static VersionHelper? TheVersion { get; set; }

    /// <summary>Gets the Git commit count string.</summary>
    public static int GitCommitCount => TryParseInt(GetMetadata("GitRevision"));

    /// <summary>Gets the Git commit time string.</summary>
    public static long GitCommitTimeUnixSeconds => TryParseLong(GetMetadata("GitCommitUnixTime"));

    /// <summary>Gets the Git commit author name.</summary>
    public static string GitCommitAuthorName => GetMetadata("GitAuthor") ?? string.Empty;

    /// <summary>Gets or sets a value indicating whether to show the full version.</summary>
    public bool ShowFullVersion { get; set; }

    /// <summary>Gets the version number as a 32-bit unsigned integer.</summary>
    public uint VersionNumber => ((uint)_major << 16) | (uint)_minor;

    /// <summary>Gets the version string.</summary>
    public string StringVersion
    {
        get
        {
            string version;

            if (ShowFullVersion)
            {
                if (_localBuildNum == 0)
                {
                    version = $"{_major}.{_minor}.{_buildNum}";
                }
                else
                {
                    var user = StringBuildUserOrGitCommitAuthorName;
                    if (user.Length < 2)
                    {
                        user += "xx";
                    }

                    version = $"{_major}.{_minor}.{_buildNum}.{_localBuildNum}{user[0]}{user[1]}";
                }
            }
            else
            {
                version = $"{_major}.{_minor}";
            }

#if RTS_DEBUG
            version += " Debug";
#endif
            return version;
        }
    }

    /// <summary>Gets the build date and time string.</summary>
    public string StringBuildTime => $"{_buildDate} {_buildTime}".Trim();

    /// <summary>Gets the build location string.</summary>
    public string StringBuildLocation { get; private set; } = string.Empty;

    /// <summary>Gets the build user string.</summary>
    public string StringBuildUser { get; private set; } = string.Empty;

    /// <summary>Gets the Git commit count string.</summary>
    public string StringGitCommitCount { get; }

    /// <summary>Gets the Git tag or hash string.</summary>
    public string StringGitTagOrHash { get; }

    /// <summary>Gets the Git commit time string.</summary>
    public string StringGitCommitTime { get; }

    /// <summary>Gets the Git version string.</summary>
    public string StringGitVersion =>
        ShowFullVersion ? $"{StringGitCommitCount} {StringGitTagOrHash}".Trim() : $"{StringGitCommitCount}".Trim();

    /// <summary>Gets the build user or Git commit author name string.</summary>
    public string StringBuildUserOrGitCommitAuthorName
    {
        get
        {
            var user = StringBuildUser;
            if (string.IsNullOrWhiteSpace(user))
            {
                user = GitCommitAuthorName;
            }

            return user;
        }
    }

    /// <summary>Sets the version information and build details for this instance of the <see cref="VersionHelper"/> class.</summary>
    /// <param name="versionNumber">A tuple containing version components: major version, minor version, build number, and local build number.</param>
    /// <param name="buildInfo">A tuple containing build metadata: user name, build location, build time, and build date.</param>
    public void SetVersion(
        (int Major, int Minor, int BuildNumber, int LocalBuildNumber) versionNumber,
        (string User, string Location, string BuildTime, string BuildDate) buildInfo
    )
    {
        _major = versionNumber.Major;
        _minor = versionNumber.Minor;
        _buildNum = versionNumber.BuildNumber;
        _localBuildNum = versionNumber.LocalBuildNumber;
        StringBuildUser = buildInfo.User;
        StringBuildLocation = buildInfo.Location;
        _buildTime = buildInfo.BuildTime;
        _buildDate = buildInfo.BuildDate;
    }

    private static string? GetMetadata(string key)
    {
        var asm = Assembly.GetExecutingAssembly();
        AssemblyMetadataAttribute? attr = asm.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => string.Equals(a.Key, key, StringComparison.OrdinalIgnoreCase));

        return attr?.Value?.Trim();
    }

    private static bool GetGitUncommittedChanges()
    {
        var dirtyRaw = GetMetadata("GitUncommittedChanges") ?? "false";
        return bool.TryParse(dirtyRaw, out var dirty) && dirty;
    }

    private static string BuildGitCommitCount()
    {
        var revision = GetMetadata("GitRevision") ?? string.Empty;
        return $"{(GetGitUncommittedChanges() ? "~" : string.Empty)}{revision}";
    }

    private static string BuildGitTagOrHash()
    {
        var tag = GetMetadata("GitTag") ?? string.Empty;
        var sha = GetMetadata("GitShortSha") ?? string.Empty;

        var tagOrHash = !string.IsNullOrWhiteSpace(tag) ? tag : sha;
        return $"{(GetGitUncommittedChanges() ? "~" : string.Empty)}{tagOrHash}";
    }

    private static string BuildGitCommitTime()
    {
        var seconds = GitCommitTimeUnixSeconds;
        if (seconds <= 0)
        {
            return string.Empty;
        }

        DateTime dt = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        return dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private static int TryParseInt(string? s) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;

    private static long TryParseLong(string? s) =>
        long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;
}
