// -----------------------------------------------------------------------
// <copyright file="NameKeyGenerator.cs" company="Sage.Net">
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

namespace Sage.Net.GameEngine.Common;

/// <summary>A name key generator.</summary>
public class NameKeyGenerator : SubsystemBase
{
#if RTS_ZERO_HOUR
    private const int SocketCount = 45_007;
#else
    private const int SocketCount = 6_473;
#endif

    private readonly Bucket?[] _sockets = new Bucket[SocketCount];

    private uint _nextId = (uint)NameKeyType.Invalid;

    /// <summary>Gets or sets the singleton instance of the <see cref="NameKeyGenerator"/> class.</summary>
    public static NameKeyGenerator? TheNameKeyGenerator { get; set; }

    /// <inheritdoc/>
    public override void Initialize()
    {
        Debug.Assert(_nextId == (uint)NameKeyType.Invalid, $"{nameof(NameKeyGenerator)} already initialized.");

        FreeSockets();
        _nextId = 1;
    }

    /// <inheritdoc/>
    public override void Reset()
    {
        FreeSockets();
        _nextId = 1;
    }

    /// <inheritdoc/>
    /// <remarks>This is a no-op.</remarks>
    public override void UpdateCore() { }

    /// <summary>Converts a given name to a <see cref="NameKeyType"/>.</summary>
    /// <param name="name">A <see cref="string"/> to convert.</param>
    /// <returns>A new <see cref="NameKeyType"/> from the given <paramref name="name"/>.</returns>
    /// <remarks>If a <see cref="Bucket"/> already exists with this key name, it will retrieve the key from it; otherwise, it will create and populate a new <see cref="Bucket"/>.</remarks>
    /// <seealso cref="NameToLowerCaseKey"/>
    public NameKeyType NameToKey(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var hash = CalculateHashForString(name) % SocketCount;
        Bucket? bucket;
        for (bucket = _sockets[0]; bucket is not null; bucket = bucket.NextInSocket)
        {
            if (name.Equals(bucket.Name, StringComparison.Ordinal))
            {
                return bucket.Key;
            }
        }

        return NameToKeyCore(hash, name);
    }

    /// <summary>Converts a given name in lower case to a <see cref="NameKeyType"/>.</summary>
    /// <param name="name">A <see cref="string"/> to convert.</param>
    /// <returns>A new <see cref="NameKeyType"/> from the given <paramref name="name"/>.</returns>
    /// <remarks>If a <see cref="Bucket"/> already exists with this key name, it will retrieve the key from it; otherwise, it will create and populate a new <see cref="Bucket"/>. This will use the invariant lower case form of the <paramref name="name"/>.</remarks>
    /// <seealso cref="NameToKey"/>
    public NameKeyType NameToLowerCaseKey(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var hash = CalculateHashForLowerCaseString(name) % SocketCount;
        Bucket? bucket;
        for (bucket = _sockets[0]; bucket is not null; bucket = bucket.NextInSocket)
        {
            if (name.Equals(bucket.Name, StringComparison.OrdinalIgnoreCase))
            {
                return bucket.Key;
            }
        }

        return NameToKeyCore(hash, name);
    }

    /// <summary>Converts a key to a name.</summary>
    /// <param name="key">The <see cref="NameKeyType"/> to convert.</param>
    /// <returns>A <see cref="string"/> with the name of the <paramref name="key"/>; <see cref="string.Empty"/> if it couldn't be found in the <see cref="Bucket"/>s.</returns>
    public string KeyToName(NameKeyType key)
    {
        for (var i = 0; i < SocketCount; i++)
        {
            for (Bucket? bucket = _sockets[i]; bucket is not null; bucket = bucket.NextInSocket)
            {
                if (key == bucket.Key)
                {
                    return bucket.Name;
                }
            }
        }

        return string.Empty;
    }

    /// <inheritdoc/>
    /// <remarks>This will first remove all the <see cref="Bucket"/>s used by returning them to their pool.</remarks>
    protected override void Dispose(bool disposing)
    {
        FreeSockets();
        base.Dispose(disposing);
    }

    private static uint CalculateHashForString(string str) =>
        str.Aggregate(0U, (current, c) => (current << 5) + current + c);

    [SuppressMessage(
        "Globalization",
        "CA1308:Normalize strings to uppercase",
        Justification = "Compatibility with the original code."
    )]
    private static uint CalculateHashForLowerCaseString(string str) => CalculateHashForString(str.ToLowerInvariant());

#if RTS_ZERO_HOUR && RETAIL_COMPATIBLE_CRC
    private bool AddReservedKey()
    {
        switch (_nextId)
        {
            case 97:
                _ = NameToLowerCaseKey(Path.Combine("Data", "English", "Language9x.ini"));
                return true;
            case 98:
                _ = NameToLowerCaseKey(Path.Combine("Data", "Audio", "Tracks", "English", "GLA_02.mp3"));
                return true;
            case 99:
                _ = NameToLowerCaseKey(Path.Combine("Data", "Audio", "Tracks", "GLA_02.mp3"));
                return true;
            default:
                break;
        }

        return false;
    }
#endif

    private NameKeyType NameToKeyCore(uint hash, string name)
    {
        var bucket = Bucket.New();
        bucket.Key = new NameKeyType((int)_nextId++);
        bucket.Name = name;
        bucket.NextInSocket = _sockets[hash];
        _sockets[hash] = bucket;

        NameKeyType result = bucket.Key;

#if DEBUG
        const int maxThreshold = 3;
        var overThresholdCount = 0;
        for (var i = 0; i < SocketCount; i++)
        {
            var inThisSocketCount = 0;
            for (bucket = _sockets[i]; bucket is not null; bucket = bucket.NextInSocket)
            {
                inThisSocketCount++;
            }

            if (inThisSocketCount > maxThreshold)
            {
                overThresholdCount++;
            }
        }

        if (overThresholdCount > SocketCount / 20)
        {
            Debug.Fail(
                $"Might need to increase the number of bucket-sockets for {nameof(NameKeyGenerator)} ({nameof(overThresholdCount)} {overThresholdCount} = {overThresholdCount / (SocketCount / 20F):F2})"
            );
        }
#endif
#if RTS_ZERO_HOUR && RETAIL_COMPATIBLE_CRC
        while (AddReservedKey())
        {
            // Keep adding
        }
#endif
        return result;
    }

    private void FreeSockets()
    {
        for (var i = 0; i < SocketCount; i++)
        {
            Bucket? next;
            for (Bucket? bucket = _sockets[i]; bucket is not null; bucket = next)
            {
                next = bucket.NextInSocket;
                bucket.Delete();
            }

            _sockets[i] = null;
        }
    }
}
