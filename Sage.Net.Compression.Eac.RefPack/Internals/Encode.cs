// -----------------------------------------------------------------------
// <copyright file="Encode.cs" company="Sage.Net">
// Copyright (c) Sage.Net. All rights reserved.
// Licensed under the MIT license.
// See LICENSE.md for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using Sage.Net.Extensions;

namespace Sage.Net.Compression.Eac.RefPack.Internals;

public static class Encode
{
    private static void WriteHeader([NotNull] BinaryWriter writer, ReadOnlySpan<byte> source)
    {
        // Decide which header to write based on the length of the source data
        if (source.Length > 0xFFFFFF)
        {
            // Write the 4-byte length header
            writer.WriteUInt16BigEndian(0x90FB);

            // Write the length as a 4-byte big-endian integer
            writer.WriteUInt32BigEndian((uint)source.Length);
        }
        else
        {
            // Write the 3-byte length header
            writer.WriteUInt16BigEndian(0x10FB);

            // Write the length as a 3-byte big-endian integer
            writer.WriteUInt24BigEndian((uint)source.Length);
        }
    }
}
