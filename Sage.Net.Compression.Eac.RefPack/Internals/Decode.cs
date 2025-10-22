// -----------------------------------------------------------------------
// <copyright file="Decode.cs" company="Sage.Net">
// Copyright (c) Sage.Net. All rights reserved.
// Licensed under the MIT license.
// See LICENSE.md for more information.
// </copyright>
// -----------------------------------------------------------------------

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
    internal static bool IsValidRefPackStream(BinaryReader reader)
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
    internal static int RetrieveDecompressedRefPackStreamSize(BinaryReader reader)
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
}
