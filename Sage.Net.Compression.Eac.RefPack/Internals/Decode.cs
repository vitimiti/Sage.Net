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
}
