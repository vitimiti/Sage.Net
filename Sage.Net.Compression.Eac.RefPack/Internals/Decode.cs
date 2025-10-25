// -----------------------------------------------------------------------
// <copyright file="Decode.cs" company="Sage.Net">
// Copyright (c) Sage.Net. All rights reserved.
// Licensed under the MIT license.
// See LICENSE.md for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using Sage.Net.Extensions;

namespace Sage.Net.Compression.Eac.RefPack.Internals;

/// <summary>
/// Provides methods for decoding RefPack compressed streams.
/// </summary>
internal static class Decode
{
    /// <summary>
    /// Determines whether the provided stream is a valid RefPack compressed stream.
    /// </summary>
    /// <param name="reader">The binary reader for the stream to check.</param>
    /// <returns><see langword="true"/> if the stream is a valid RefPack stream; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method checks the first two bytes of the stream against known RefPack signatures and doesn't alter the stream's position.
    /// </remarks>
    public static bool IsValidRefPackStream(BinaryReader reader)
    {
        if (reader.BaseStream.Length < 2)
        {
            return false; // Not enough data to read the header
        }

        // Save the current position to restore later
        var currentPosition = reader.BaseStream.Position;
        try
        {
            // Read the first two bytes as big-endian
            _ = reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var headerMagic = reader.ReadUInt16BigEndian();

            // Check if the header matches any of the known RefPack signatures
            return headerMagic is 0x10FB or 0x11FB or 0x90FB or 0x91FB;
        }
        finally
        {
            // Restore the original position
            _ = reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
        }
    }

    /// <summary>
    /// Retrieves the size of the decompressed data from a RefPack compressed stream.
    /// </summary>
    /// <param name="reader">The binary reader for the RefPack compressed stream.</param>
    /// <returns>The size of the decompressed data.</returns>
    /// <remarks>
    /// This method reads the size information from the stream header and does not alter the stream's position.
    /// </remarks>
    public static int RetrieveDecompressedRefPackStreamSize(BinaryReader reader)
    {
        // Save the current position to restore later
        var currentPosition = reader.BaseStream.Position;
        try
        {
            // Read the first two bytes as big-endian
            _ = reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var headerMagic = reader.ReadUInt16BigEndian();

            // Check if the header starts with 0x9 or 0x1
            var isOx9 = (headerMagic & 0x8000) != 0;

            // Determine the size of the decompressed data based on the header
            var sizeBytes = isOx9 ? 4 : 3;

            // Skip the size bytes if it's version 0x(.)1(..)
            var shouldSkip = (headerMagic & 0x1000) != 0;
            if (shouldSkip)
            {
                _ = reader.BaseStream.Seek(sizeBytes, SeekOrigin.Current);
            }

            // Read and return the decompressed size
            return sizeBytes == 3 ? (int)reader.ReadUInt24BigEndian() : (int)reader.ReadUInt32BigEndian();
        }
        finally
        {
            // Restore the original position
            _ = reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
        }
    }

    /// <summary>
    /// Decompresses data from a RefPack compressed stream.
    /// </summary>
    /// <param name="reader">The binary reader for the RefPack compressed stream.</param>
    /// <returns>A list of bytes representing the decompressed data.</returns>
    public static List<byte> Decompress([NotNull] BinaryReader reader)
    {
        if (reader.BaseStream.Length == 0)
        {
            return [];
        }

        var unpackedLength = DecodeCalculateSize(reader);
        List<byte> destination = new(unpackedLength);

        while (true)
        {
            var first = reader.ReadByte();
            if (TryAndProcessShortForm(reader, destination, first))
            {
                continue;
            }

            if (TryAndProcessIntForm(reader, destination, first))
            {
                continue;
            }

            if (TryAndProcessVeryIntForm(reader, destination, first))
            {
                continue;
            }

            if (TryAndProcessLiteral(reader, destination, first))
            {
                continue;
            }

            ProcessEofLiteral(reader, destination, first);
            break;
        }

        return destination;
    }

    private static int DecodeCalculateSize([NotNull] BinaryReader reader)
    {
        // Get the header
        var type = reader.ReadUInt16BigEndian();

        // Check whether we should skip the size bytes
        var shouldSkip = (type & 0x1000) != 0;

        // Determine the size of the decompressed data based on the header
        var sizeBytes = (type & 0x8000) != 0 ? 4 : 3;

        // If we are reading 4 bytes of size data
        if (sizeBytes == 4)
        {
            if (shouldSkip)
            {
                // Skip 4 bytes
                _ = reader.BaseStream.Seek(4, SeekOrigin.Current);
            }

            return (int)reader.ReadUInt32BigEndian();
        }

        // If we are reading 3 bytes of size data
        if (shouldSkip)
        {
            // Skip 3 bytes
            _ = reader.BaseStream.Seek(3, SeekOrigin.Current);
        }

        return (int)reader.ReadUInt24BigEndian();
    }

    private static bool TryAndProcessShortForm([NotNull] BinaryReader reader, List<byte> destination, byte first)
    {
        if ((first & 0x80) != 0)
        {
            return false;
        }

        var second = reader.ReadByte();
        var runlength = first & 3;
        while (runlength > 0)
        {
            destination.Add(reader.ReadByte());
            runlength--;
        }

        var referenceIndex = destination.Count - 1 - (((first & 0x60) << 3) + second);
        runlength = ((first & 0x1C) >> 2) + 3 - 1;
        do
        {
            destination.Add(destination[referenceIndex++]);
            runlength--;
        } while (runlength > 0);

        return true;
    }

    private static bool TryAndProcessIntForm([NotNull] BinaryReader reader, List<byte> destination, byte first)
    {
        if ((first & 0x40) != 0)
        {
            return false;
        }

        var second = reader.ReadByte();
        var third = reader.ReadByte();
        var runlength = second >> 6;
        while (runlength > 0)
        {
            destination.Add(reader.ReadByte());
            runlength--;
        }

        var referenceIndex = destination.Count - 1 - (((second & 0x3F) << 8) + third);
        runlength = (first & 0x3F) + 4 - 1;
        do
        {
            destination.Add(destination[referenceIndex++]);
            runlength--;
        } while (runlength > 0);

        return true;
    }

    private static bool TryAndProcessVeryIntForm([NotNull] BinaryReader reader, List<byte> destination, byte first)
    {
        var second = reader.ReadByte();
        var third = reader.ReadByte();
        var fourth = reader.ReadByte();
        var runlength = first & 3;
        while (runlength > 0)
        {
            destination.Add(reader.ReadByte());
            runlength--;
        }

        var referenceIndex = destination.Count - 1 - ((((first & 0x10) >> 4) << 16) + (second << 8) + third);

        runlength = (((first & 0x0C) >> 2) << 8) + fourth + 5 - 1;
        do
        {
            destination.Add(destination[referenceIndex++]);
            runlength--;
        } while (runlength > 0);

        return true;
    }

    private static bool TryAndProcessLiteral([NotNull] BinaryReader reader, List<byte> destination, byte first)
    {
        var runlength = ((first & 0x1F) << 2) + 4;
        if (runlength > 112)
        {
            return false;
        }

        while (runlength > 0)
        {
            destination.Add(reader.ReadByte());
            runlength--;
        }

        return true;
    }

    private static void ProcessEofLiteral([NotNull] BinaryReader reader, List<byte> destination, byte first)
    {
        var runlength = first & 3;
        while (runlength > 0)
        {
            destination.Add(reader.ReadByte());
            runlength--;
        }
    }
}
