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
using Sage.Net.Core.GameEngine.Common.GameMemory;
using Sage.Net.Core.GameEngine.Common.Ini;
using Sage.Net.Core.GameEngine.Common.Pools;

namespace Sage.Net.Core.GameEngine.Common.Subsystems;

/// <summary>
/// Name key generator.
/// </summary>
public class NameKeyGenerator : SubsystemBase, IDisposable
{
    private const int SocketCount = 45007;

    private readonly Bucket?[] _sockets = new Bucket[SocketCount];

    private int _nextId = NameKeyType.Invalid;
    private bool _disposed;

    /// <summary>
    /// Gets a string out of the next token in the ini reader as a <see cref="NameKeyType"/>.
    /// </summary>
    /// <param name="ini">The <see cref="IniReader"/> to get the name key type from.</param>
    /// <returns>A new <see cref="NameKeyType"/> from the <paramref name="ini"/> file.</returns>
    public static NameKeyType ParseStringAsNameKeyType([NotNull] IniReader ini) =>
        GlobalSingletons.TheNameKeyGenerator!.NameToKey(ini.GetNextToken());

    /// <summary>
    /// Initializes the <see cref="NameKeyGenerator"/>.
    /// </summary>
    public override void Initialize()
    {
        Debug.Assert(_nextId == NameKeyType.Invalid, $"{nameof(NameKeyGenerator)} already initialized.");

        FreeSockets();
        _nextId = 1;
    }

    /// <summary>
    /// Resets the <see cref="NameKeyGenerator"/>.
    /// </summary>
    public override void Reset()
    {
        FreeSockets();
        _nextId = 1;
    }

    /// <inheritdoc/>
    /// <note>This is a no-op.</note>
    public override void UpdateBase() { }

    /// <summary>
    /// Gets the name from the key.
    /// </summary>
    /// <param name="key">The <see cref="NameKeyType"/> to get the name from.</param>
    /// <returns>A new <see cref="string"/> with the name of the given <paramref name="key"/>; otherwise <see cref="string.Empty"/>.</returns>
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

    /// <summary>
    /// Gets the key from the name.
    /// </summary>
    /// <param name="name">The name of the key.</param>
    /// <returns>A new <see cref="NameKeyType"/> from the given <paramref name="name"/>.</returns>
    public NameKeyType NameToKey([NotNull] string name) =>
        NameToKeyCore(name, CalculateHashForString(name) % SocketCount);

    /// <summary>
    /// Gets the key from the lowercase name.
    /// </summary>
    /// <param name="name">The name of the key.</param>
    /// <returns>A new <see cref="NameToKey"/> from the given <paramref name="name"/>, which will be lowercased.</returns>
    public NameKeyType NameToLowerCaseKey([NotNull] string name) =>
        NameToKeyCore(name, CalculateHashForLowercaseString(name) % SocketCount);

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the <see cref="NameKeyGenerator"/>.
    /// </summary>
    /// <param name="disposing">Whether to dispose of managed objects or not.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            FreeSockets();
        }

        _disposed = true;
    }

    private static uint CalculateHashForString(string str) =>
        str.Aggregate(0U, (current, c) => (current << 5) + current + c);

    private static uint CalculateHashForLowercaseString(string str) =>
        str.Aggregate(0U, (current, c) => (current << 5) + current + char.ToLowerInvariant(c));

#if RETAIL_COMPATIBLE_CRC
    private bool AddReservedKey()
    {
        switch (_nextId)
        {
            case 97:
                NameToLowercaseKey(Path.Combine("Data", "English", "Language9x.ini"));
                return true;
            case 98:
                NameToLowercaseKey(Path.Combine("Data", "Audio", "Tracks", "English", "GLA_02.mp3"));
                return true;
            case 99:
                NameToLowercaseKey(Path.Combine("Data", "Audio", "Tracks", "GLA_02.mp3"));
                return true;
            default:
                return false;
        }
    }
#endif

    private void FreeSockets()
    {
        for (var i = 0; i < SocketCount; i++)
        {
            Bucket? next;
            for (Bucket? bucket = _sockets[i]; bucket is not null; bucket = next)
            {
                next = bucket.NextInSocket;
                GamePool<Bucket>.Return(bucket);
            }

            _sockets[i] = null;
        }
    }

    private NameKeyType NameToKeyCore([NotNull] string name, uint hash)
    {
        Bucket? bucket;
        for (bucket = _sockets[hash]; bucket is not null; bucket = bucket.NextInSocket)
        {
            if (name.Equals(bucket.Name, StringComparison.Ordinal))
            {
                return bucket.Key;
            }
        }

        bucket = GamePool<Bucket>.Rent();
        bucket.Key = new NameKeyType(_nextId++);
        bucket.Name = name;
        bucket.NextInSocket = _sockets[hash];
        _sockets[hash] = bucket;

        NameKeyType result = bucket.Key;

#if DEBUG
        const int maxThreshold = 3;
        var numOverThreshold = 0;
        for (var i = 0; i < SocketCount; i++)
        {
            var numInThisSocket = 0;
            for (bucket = _sockets[i]; bucket is not null; bucket = bucket.NextInSocket)
            {
                numInThisSocket++;
            }

            if (numInThisSocket > maxThreshold)
            {
                numOverThreshold++;
            }
        }

        if (numOverThreshold > SocketCount / 20)
        {
            Debug.Fail(
                $"Might need to increase the number of bucket-sockets for {nameof(NameKeyGenerator)} ({nameof(numOverThreshold)}: {numOverThreshold / (SocketCount / 20F):F2})"
            );
        }
#endif

#if RETAIL_COMPATIBLE_CRC
        while (AddReservedKey())
        {
            // Keep adding
        }
#endif
        return result;
    }
}
