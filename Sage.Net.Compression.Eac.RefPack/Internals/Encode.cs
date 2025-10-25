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

/// <summary>
/// Provides methods for encoding data using the RefPack compression algorithm.
/// </summary>
internal static class Encode
{
    /// <summary>
    /// Compresses the source span and writes the compressed data to the binary writer.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    /// <param name="source">The source span.</param>
    /// <param name="quick">Whether to use quick compression.</param>
    public static void Compress([NotNull] BinaryWriter writer, ReadOnlySpan<byte> source, bool quick)
    {
        WriteHeader(writer, source);

        if (source.Length == 0)
        {
            return;
        }

        const int maxBack = 131071;
        const int hashTableSize = 65536;
        const int linkSize = 131072;

        // Initialize compression context
        CompressionContext context = new(hashTableSize, linkSize, source.Length);

        // Lookahead
        context.Length -= 4;
        while (context.Length >= 0)
        {
            // Initialize compression loop context
            CompressionLoopContext loopContext = new(
                source,
                context.Length,
                context.CurrentPointer,
                maxBack,
                context.HashTable
            );

            // Process large loop if hash offset is valid
            if (loopContext.HashOffset >= loopContext.MinimumHashOffset)
            {
                ProcessLargeLoop(context, loopContext, source);
            }

            // Decide processing based on cost and length
            if (loopContext.Cost >= loopContext.Length || context.Length < 4)
            {
                ProcessCostlyLoop(context, loopContext);
            }
            else
            {
                ProcessMainLoop(context, loopContext, source, writer, quick);
            }
        }

        // Flush remaining bytes
        FlushLastBytes(context, source, writer);
    }

    /// <summary>
    /// Writes the appropriate header to the binary writer based on the source length.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    /// <param name="source">The source span.</param>
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

    /// <summary>
    /// Computes the hash value for a sequence of three bytes starting at the specified index.
    /// </summary>
    /// <param name="source">The source span.</param>
    /// <param name="index">The starting index.</param>
    /// <returns>The computed hash value.</returns>
    private static int Hash(ReadOnlySpan<byte> source, int index)
    {
        uint a = source[index];
        uint b = source[index + 1];
        uint c = source[index + 2];
        return (int)(((a << 8) | c) ^ (b << 4));
    }

    /// <summary>
    /// Calculates the length of the match between source and destination up to maxMatch bytes.
    /// </summary>
    /// <param name="source">The source span.</param>
    /// <param name="destination">The destination span.</param>
    /// <param name="maxMatch">The maximum match length.</param>
    /// <returns>The length of the match.</returns>
    private static int MatchLength(ReadOnlySpan<byte> source, ReadOnlySpan<byte> destination, int maxMatch)
    {
        var i = 0;
        var max = int.Min(maxMatch, Math.Min(source.Length, destination.Length));
        for (; i < max && source[i] == destination[i]; i++)
        {
            // Continue matching
        }

        return i;
    }

    /// <summary>
    /// Determines if a match is found and updates the loop context accordingly.
    /// </summary>
    /// <param name="context">The compression context.</param>
    /// <param name="loopContext">The compression loop context.</param>
    /// <param name="tempPointer">The temporary pointer.</param>
    /// <param name="source">The source span.</param>
    /// <returns><see langword="true"/> if a match is found; otherwise, <see langword="false"/>.</returns>
    private static bool IsMatched(
        CompressionContext context,
        CompressionLoopContext loopContext,
        int tempPointer,
        ReadOnlySpan<byte> source
    )
    {
        var tempLength = MatchLength(
            source[context.CurrentPointer..],
            source[tempPointer..],
            loopContext.MinimumLength
        );
        if (tempLength > loopContext.Length)
        {
            var tempOffset = context.CurrentPointer - 1 - tempPointer;
            var tempCost = tempOffset switch
            {
                < 1024 when tempLength <= 10 => 2,
                < 16384 when tempLength <= 67 => 3,
                _ => 4,
            };

            if (tempLength - tempCost + 4 > loopContext.Length - loopContext.Cost + 4)
            {
                loopContext.Length = tempLength;
                loopContext.Cost = tempCost;
                loopContext.Offset = tempOffset;
                if (loopContext.Length >= 1028)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Processes the large loop to find matches in the compression context.
    /// </summary>
    /// <param name="context">The compression context.</param>
    /// <param name="loopContext">The compression loop context.</param>
    /// <param name="source">The source span.</param>
    private static void ProcessLargeLoop(
        CompressionContext context,
        CompressionLoopContext loopContext,
        ReadOnlySpan<byte> source
    )
    {
        var chain = loopContext.HashOffset;
        while (chain >= loopContext.MinimumHashOffset)
        {
            var tempPtr = chain;
            if (
                context.CurrentPointer + loopContext.Length < source.Length
                && tempPtr + loopContext.Length < source.Length
                && source[context.CurrentPointer + loopContext.Length] == source[tempPtr + loopContext.Length]
                && IsMatched(context, loopContext, tempPtr, source)
            )
            {
                break;
            }

            chain = context.Link[chain & (context.Link.Length - 1)];
            if (chain < loopContext.MinimumHashOffset)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Processes the costly loop when no suitable match is found.
    /// </summary>
    /// <param name="context">The compression context.</param>
    /// <param name="loopContext">The compression loop context.</param>
    private static void ProcessCostlyLoop(CompressionContext context, CompressionLoopContext loopContext)
    {
        loopContext.HashOffset = context.CurrentPointer;
        context.Link[loopContext.HashOffset & (context.Link.Length - 1)] = context.HashTable[loopContext.HashValue];
        context.HashTable[loopContext.HashValue] = loopContext.HashOffset;

        ++context.Run;
        ++context.CurrentPointer;
        --context.Length;
    }

    /// <summary>
    /// Processes the literal block of data.
    /// </summary>
    /// <param name="context">The compression context.</param>
    /// <param name="source">The source span.</param>
    /// <param name="writer">The binary writer.</param>
    private static void ProcessLiteralBlockOfData(
        CompressionContext context,
        ReadOnlySpan<byte> source,
        BinaryWriter writer
    )
    {
        while (context.Run > 3)
        {
            var tempLength = int.Min(112, context.Run & ~3);
            context.Run -= tempLength;
            writer.Write((byte)(0xE0 + (tempLength >> 2) - 1));
            writer.Write(source.Slice(context.ReadPointer, tempLength).ToArray());
            context.ReadPointer += tempLength;
        }
    }

    /// <summary>
    /// Processes the forms based on the loop context and writes to the binary writer.
    /// </summary>
    /// <param name="context">The compression context.</param>
    /// <param name="loopContext">The compression loop context.</param>
    /// <param name="writer">The binary writer.</param>
    private static void ProcessForms(
        CompressionContext context,
        CompressionLoopContext loopContext,
        BinaryWriter writer
    )
    {
        if (loopContext.Cost == 2)
        {
            writer.Write(
                (byte)((((loopContext.Offset >> 8) << 5) + ((loopContext.Length - 3) << 2) + context.Run) & 0xFF)
            );

            writer.Write((byte)(loopContext.Offset & 0xFF));
        }
        else if (loopContext.Cost == 3)
        {
            writer.Write((byte)(0x80 + (loopContext.Length - 4)));
            writer.Write((byte)(((context.Run << 6) + (loopContext.Offset >> 8)) & 0xFF));
            writer.Write((byte)(loopContext.Offset & 0xFF));
        }
        else
        {
            writer.Write(
                (byte)(0xC0 + ((loopContext.Offset >> 16) << 4) + (((loopContext.Length - 5) >> 8) << 2) + context.Run)
            );

            writer.Write((byte)((loopContext.Offset >> 8) & 0xFF));
            writer.Write((byte)(loopContext.Offset & 0xFF));
            writer.Write((byte)((loopContext.Length - 5) & 0xFF));
        }
    }

    /// <summary>
    /// Calculates the next hash value and updates the compression context.
    /// </summary>
    /// <param name="context">The compression context.</param>
    /// <param name="loopContext">The compression loop context.</param>
    /// <param name="source">The source span.</param>
    /// <param name="quick">Indicates if the calculation is quick.</param>
    private static void CalculateNextHash(
        CompressionContext context,
        CompressionLoopContext loopContext,
        ReadOnlySpan<byte> source,
        bool quick
    )
    {
        if (quick)
        {
            loopContext.HashOffset = context.CurrentPointer;
            context.Link[loopContext.HashOffset & (context.Link.Length - 1)] = context.HashTable[loopContext.HashValue];
            context.HashTable[loopContext.HashValue] = loopContext.HashOffset;
            context.CurrentPointer += loopContext.Length;
        }
        else
        {
            for (var i = 0; i < loopContext.Length; ++i)
            {
                loopContext.HashValue = Hash(source, context.CurrentPointer);
                loopContext.HashOffset = context.CurrentPointer;
                context.Link[loopContext.HashOffset & (context.Link.Length - 1)] = context.HashTable[
                    loopContext.HashValue
                ];

                context.HashTable[loopContext.HashValue] = loopContext.HashOffset;
                ++context.CurrentPointer;
            }
        }
    }

    /// <summary>
    /// Processes the main loop of the compression algorithm.
    /// </summary>
    /// <param name="context">The compression context.</param>
    /// <param name="loopContext">The compression loop context.</param>
    /// <param name="source">The source span.</param>
    /// <param name="writer">The binary writer.</param>
    /// <param name="quick">Indicates if the calculation is quick.</param>
    private static void ProcessMainLoop(
        CompressionContext context,
        CompressionLoopContext loopContext,
        ReadOnlySpan<byte> source,
        BinaryWriter writer,
        bool quick
    )
    {
        ProcessLiteralBlockOfData(context, source, writer);
        ProcessForms(context, loopContext, writer);

        if (context.Run > 0)
        {
            writer.Write(source.Slice(context.ReadPointer, context.Run).ToArray());
            context.Run = 0;
        }

        CalculateNextHash(context, loopContext, source, quick);

        context.ReadPointer = context.CurrentPointer;
        context.Length -= loopContext.Length;
    }

    /// <summary>
    /// Flushes the last bytes to the binary writer.
    /// </summary>
    /// <param name="context">The compression context.</param>
    /// <param name="source">The source span.</param>
    /// <param name="writer">The binary writer.</param>
    private static void FlushLastBytes(CompressionContext context, ReadOnlySpan<byte> source, BinaryWriter writer)
    {
        context.Length += 4;
        context.Run += context.Length;
        while (context.Run > 3)
        {
            var tempLength = int.Min(112, context.Run & ~3);
            context.Run -= tempLength;
            writer.Write((byte)(0xE0 + (tempLength >> 2) - 1));
            writer.Write(source.Slice(context.ReadPointer, tempLength).ToArray());
            context.ReadPointer += tempLength;
        }

        writer.Write((byte)(0xFC + context.Run));
        if (context.Run > 0)
        {
            writer.Write(source.Slice(context.ReadPointer, context.Run).ToArray());
        }
    }

    /// <summary>
    /// Represents the compression context.
    /// </summary>
    private sealed class CompressionContext
    {
        /// <summary>
        /// Gets the hash table.
        /// </summary>
        public int[] HashTable { get; }

        /// <summary>
        /// Gets the link table.
        /// </summary>
        public int[] Link { get; }

        /// <summary>
        /// Gets or sets the run length.
        /// </summary>
        public int Run { get; set; }

        /// <summary>
        /// Gets or sets the current pointer.
        /// </summary>
        public int CurrentPointer { get; set; }

        /// <summary>
        /// Gets or sets the read pointer.
        /// </summary>
        public int ReadPointer { get; set; }

        /// <summary>
        /// Gets or sets the remaining length to process.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionContext"/> class.
        /// </summary>
        /// <param name="hashTableSize">Size of the hash table.</param>
        /// <param name="linkSize">Size of the link table.</param>
        /// <param name="sourceLength">Length of the source data.</param>
        public CompressionContext(int hashTableSize, int linkSize, int sourceLength)
        {
            HashTable = new int[hashTableSize];
            Link = new int[linkSize];

            Array.Fill(HashTable, -1);

            Length = sourceLength;
        }
    }

    /// <summary>
    /// Represents the compression loop context.
    /// </summary>
    private sealed class CompressionLoopContext
    {
        /// <summary>
        /// Gets or sets the current offset.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the length of the match.
        /// </summary>
        public int Length { get; set; } = 2;

        /// <summary>
        /// Gets or sets the cost of the match.
        /// </summary>
        public int Cost { get; set; } = 2;

        /// <summary>
        /// Gets or sets the minimum length for matching.
        /// </summary>
        public int MinimumLength { get; set; }

        /// <summary>
        /// Gets or sets the hash value.
        /// </summary>
        public int HashValue { get; set; }

        /// <summary>
        /// Gets or sets the hash offset.
        /// </summary>
        public int HashOffset { get; set; }

        /// <summary>
        /// Gets or sets the minimum hash offset.
        /// </summary>
        public int MinimumHashOffset { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionLoopContext"/> class.
        /// </summary>
        /// <param name="source">The source span.</param>
        /// <param name="length">The remaining length to process.</param>
        /// <param name="currentPointer">The current pointer.</param>
        /// <param name="maxBack">The maximum backward distance.</param>
        /// <param name="hashTable">The hash table.</param>
        public CompressionLoopContext(
            ReadOnlySpan<byte> source,
            int length,
            int currentPointer,
            int maxBack,
            int[] hashTable
        )
        {
            MinimumLength = int.Min(length, 1028);
            HashValue = Hash(source, currentPointer);
            HashOffset = hashTable[HashValue];
            MinimumHashOffset = int.Max(currentPointer - maxBack, 0);
        }
    }
}
