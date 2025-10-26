// -----------------------------------------------------------------------
// <copyright file="Decode.cs" company="Sage.Net">
// Copyright (c) Sage.Net. All rights reserved.
// Licensed under the MIT license.
// See LICENSE.md for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using Sage.Net.Extensions;

namespace Sage.Net.Compression.Eac.BinaryTree.Internals;

/// <summary>
/// Provides methods for decoding BinaryTree compressed streams.
/// </summary>
internal static class Decode
{
    /// <summary>
    /// Determines whether the provided binary stream represents a valid BinaryTree structure.
    /// </summary>
    /// <param name="reader">The binary reader containing the stream to validate. The stream must not be null.</param>
    /// <returns><see langword="true"/> if the stream matches a known BinaryTree signature; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidBinaryTreeStream([NotNull] BinaryReader reader)
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
            var header = reader.ReadUInt16BigEndian();

            // Check if the header matches any of the known BinaryTree signatures
            return header is 0x46FB or 0x47FB;
        }
        finally
        {
            // Restore the original position
            _ = reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
        }
    }

    /// <summary>
    /// Retrieves the size of a decompressed BinaryTree stream from the provided binary reader.
    /// </summary>
    /// <param name="reader">The binary reader from which to read the size. The reader must not be null, and its stream must contain sufficient data for the operation.</param>
    /// <returns>The size of the decompressed BinaryTree stream, as an integer derived from the stream.</returns>
    public static int RetrieveDecompressedBinaryTreeStreamSize([NotNull] BinaryReader reader)
    {
        // Save the current position to restore later
        var currentPosition = reader.BaseStream.Position;
        try
        {
            // Read the first two bytes as big-endian
            var header = reader.ReadUInt16BigEndian();

            // Check if the header matches the known BinaryTree signature that indicates a 3-byte size header
            var shouldSkip = header is 0x46FB;
            if (shouldSkip)
            {
                // Skip 3 bytes
                _ = reader.BaseStream.Seek(3, SeekOrigin.Current);
            }

            // Read the remaining 3 bytes as big-endian
            return (int)reader.ReadUInt24BigEndian();
        }
        finally
        {
            // Restore the original position
            _ = reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
        }
    }

    /// <summary>
    /// Decompresses a binary tree encoded stream and reconstructs its original byte sequence.
    /// </summary>
    /// <param name="reader">The binary reader containing the encoded binary tree stream. The stream must not be null and must have valid encoded data.</param>
    /// <returns>A list of bytes representing the decompressed data.</returns>
    public static List<byte> Decompress([NotNull] BinaryReader reader)
    {
        if (reader.BaseStream.Length == 0)
        {
            return [];
        }

        DecodeContext context = new() { Destination = { Capacity = DecodeCalculateSize(reader) } };

        var clue = reader.ReadByte();
        context.ClueTable[clue] = 1; // Mark this clue as special

        var nodes = reader.ReadByte();
        int node;
        for (var i = 0; i < nodes; i++)
        {
            node = reader.ReadByte();
            context.Left[node] = reader.ReadByte();
            context.Right[node] = reader.ReadByte();
            context.ClueTable[node] = -1;
        }

        while (true)
        {
            node = reader.ReadByte();
            var currentClue = context.ClueTable[node];
            switch (currentClue)
            {
                case 0:
                    context.Destination.Add((byte)node);
                    continue;
                case < 0:
                    Chase(context, context.Left[node]);
                    Chase(context, context.Right[node]);
                    continue;
                default:
                    break;
            }

            node = reader.ReadByte();
            if (node != 0)
            {
                context.Destination.Add((byte)node);
                continue;
            }

            break;
        }

        return context.Destination;
    }

    /// <summary>
    /// Computes the size required to decode a binary tree stream.
    /// </summary>
    /// <param name="reader">The binary reader containing the stream to analyze. The stream must not be null.</param>
    /// <returns>The calculated size of the decompressed binary tree stream.</returns>
    private static int DecodeCalculateSize([NotNull] BinaryReader reader)
    {
        var header = reader.ReadUInt16BigEndian();
        var shouldSkip = header is 0x46FB;
        if (shouldSkip)
        {
            _ = reader.ReadUInt24BigEndian();
        }

        return (int)reader.ReadUInt24BigEndian();
    }

    /// <summary>
    /// Traverses a binary tree structure using the provided context, recursively visiting left and right child nodes
    /// and adding leaf nodes to the designated destination.
    /// </summary>
    /// <param name="context">The decoding context containing the tree structure data, including clue table, left and right child nodes, and destination.</param>
    /// <param name="node">The current node in the binary tree on which the traversal operation is performed.</param>
    private static void Chase(DecodeContext context, byte node)
    {
        // If the node is not a leaf, chase the left and right children
        if (context.ClueTable[node] != 0)
        {
            Chase(context, context.Left[node]);
            Chase(context, context.Right[node]);
        }

        // Add the node to the destination
        context.Destination.Add(node);
    }

    /// <summary>
    /// Represents the decoding context used for traversing and reconstructing a binary tree structure.
    /// This context provides the necessary data structures and references to perform decoding operations.
    /// </summary>
    private sealed class DecodeContext
    {
        /// <summary>
        /// Gets the clue table.
        /// </summary>
        public sbyte[] ClueTable { get; } = new sbyte[256];

        /// <summary>
        /// Gets the left child nodes.
        /// </summary>
        public byte[] Left { get; } = new byte[256];

        /// <summary>
        /// Gets the right child nodes.
        /// </summary>
        public byte[] Right { get; } = new byte[256];

        /// <summary>
        /// Gets the destination.
        /// </summary>
        public List<byte> Destination { get; } = [];
    }
}
