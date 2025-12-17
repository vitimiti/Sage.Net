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
using Microsoft.Extensions.Logging;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Provides functionality for generating and managing name-key mappings within a game engine context. This class allows for bidirectional conversion between string names and their corresponding keys, ensuring efficient access and manipulation of uniquely identified entities.</summary>
/// <remarks>The class maintains internal hashing and socketing mechanisms to optimize lookup and manage mappings robustly. It also ensures compatibility with both original engine expectations and enhanced modern behavior.</remarks>
public partial class NameKeyGenerator(ILogger logger) : SubsystemBase
{
    private const int SocketCount = 6_473;

    private readonly Bucket?[] _sockets = new Bucket?[SocketCount];

    private NameKeyType _nextId = NameKeyType.Invalid;

    /// <summary>Gets or sets the singleton instance of the <see cref="NameKeyGenerator"/> class.</summary>
    public static NameKeyGenerator? TheNameKeyGenerator { get; set; }

    /// <summary>Parses a string from the given Ini instance and converts it into a name key type, storing the result in the provided storage reference.</summary>
    /// <param name="ini">The <c>Ini</c> instance used to retrieve the next token for parsing.</param>
    /// <param name="instance">A reference to an object instance that can optionally be modified during parsing.</param>
    /// <param name="store">A reference to an object instance where the resulting name key type is stored.</param>
    /// <param name="userData">Optional user-defined data that may influence the parsing process.</param>
    /// <remarks>This method assumes that the <c>TheNameKeyGenerator</c> static property is initialized and available for mapping string names to corresponding keys.</remarks>
    public static void ParseStringAsNameKeyType(Ini ini, ref object? instance, ref object? store, object? userData)
    {
        ArgumentNullException.ThrowIfNull(ini);

        store = NameKeyGenerator.TheNameKeyGenerator!.NameToKey(ini.GetNextToken());
    }

    /// <summary>Initializes the <see cref="NameKeyGenerator"/> class.</summary>
    public override void Initialize()
    {
        Debug.Assert(_nextId == NameKeyType.Invalid, $"{nameof(NameKeyGenerator)} already initialized.");

        Reset();
    }

    /// <summary>Resets the <see cref="NameKeyGenerator"/> instance.</summary>
    public override void Reset()
    {
        FreeSockets();
        _nextId = 1;
    }

    /// <summary>Performs the actual subsystem update.</summary>
    /// <remarks>This method does nothing.</remarks>
    public override void UpdateCore() { }

    /// <summary>Retrieves the name associated with a specified key by searching through internal data structures.</summary>
    /// <param name="key">The <c>NameKeyType</c> instance representing the key whose associated name is to be retrieved.</param>
    /// <returns>A string containing the name associated with the specified key. Returns an empty string if no matching name is found.</returns>
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

    /// <summary>Converts a string name into a corresponding key of type <c>NameKeyType</c>.</summary>
    /// <param name="name">The string name to be converted into a key.</param>
    /// <returns>A <c>NameKeyType</c> representing the key generated from the provided string name.</returns>
    public NameKeyType NameToKey(string name) => NameToKeyCore(name, CalculateHashForString, StringComparison.Ordinal);

    /// <summary>Converts a given name string to a corresponding <c>NameKeyType</c>, using a case-insensitive hash calculation.</summary>
    /// <param name="name">The name string to be converted to a <c>NameKeyType</c>.</param>
    /// <returns>A <c>NameKeyType</c> instance that represents the hashed key corresponding to the provided name.</returns>
    public NameKeyType NameToLowerCaseKey(string name) =>
        NameToKeyCore(name, CalculateHashForLowerCaseString, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        FreeSockets();
        base.Dispose(disposing);
    }

    private static uint CalculateHashForString(string p) =>
        p.Aggregate(0U, (current, c) => (current << 5) + current + c);

    [SuppressMessage(
        "Globalization",
        "CA1308:Normalize strings to uppercase",
        Justification = "Compatibility with the original engine expectations."
    )]
    private static uint CalculateHashForLowerCaseString(string p) => CalculateHashForString(p.ToLowerInvariant());

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

#if DEBUG
    private void DebugCheckSocketThresholds()
    {
        const int maxThreshold = 3;
        var overThresholdCount = 0;

        for (var i = 0; i < SocketCount; i++)
        {
            var inThisSocketCount = 0;
            for (Bucket? bucket = _sockets[i]; bucket is not null; bucket = bucket.NextInSocket)
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
            Log.TooSmallSocketCount(
                logger,
                nameof(NameKeyGenerator),
                nameof(overThresholdCount),
                overThresholdCount,
                overThresholdCount / (SocketCount / 20F)
            );
        }
    }
#endif

    private NameKeyType NameToKeyCore(string name, Func<string, uint> calculateHash, StringComparison comparison)
    {
        ArgumentNullException.ThrowIfNull(name);

        var hash = calculateHash(name) % SocketCount;
        Bucket? bucket;

        for (bucket = _sockets[hash]; bucket is not null; bucket = bucket.NextInSocket)
        {
            if (name.Equals(bucket.Name, comparison))
            {
                return bucket.Key;
            }
        }

        bucket = Bucket.New();
        bucket.Key = _nextId++;
        bucket.Name = name;
        bucket.NextInSocket = _sockets[hash];
        _sockets[hash] = bucket;

        NameKeyType result = bucket.Key;

#if DEBUG
        DebugCheckSocketThresholds();
#endif

        return result;
    }

    private static partial class Log
    {
        [LoggerMessage(
            LogLevel.Debug,
            Message = "Hmm, might need to increase the number of bucket-sockets for {NameKeyGeneratorName} ({OverThresholdCountName} {OverThreshold} = {Rate:F2}"
        )]
        public static partial void TooSmallSocketCount(
            ILogger logger,
            string nameKeyGeneratorName,
            string overThresholdCountName,
            int overThreshold,
            float rate
        );
    }
}
