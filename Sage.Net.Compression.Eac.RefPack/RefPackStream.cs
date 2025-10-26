// -----------------------------------------------------------------------
// <copyright file="RefPackStream.cs" company="Sage.Net">
// Copyright (c) Sage.Net. All rights reserved.
// Licensed under the MIT license.
// See LICENSE.md for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using Sage.Net.Compression.Eac.RefPack.Internals;
using Sage.Net.Compression.Eac.RefPack.Options;
using Sage.Net.Extensions;

namespace Sage.Net.Compression.Eac.RefPack;

/// <summary>
/// Provides a stream for compressing and decompressing data using the RefPack algorithm.
/// </summary>
public sealed class RefPackStream : Stream
{
    private readonly Stream _baseStream;
    private readonly bool _leaveOpen;
    private readonly RefPackOptions _options;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefPackStream"/> class.
    /// </summary>
    /// <param name="baseStream">The underlying stream to read from or write to.</param>
    /// <param name="compressionMode">The compression mode (compress or decompress).</param>
    /// <param name="options">An optional action to configure <see cref="RefPackOptions"/>.</param>
    /// <param name="leaveOpen">Whether to leave the underlying stream open after disposing the <see cref="RefPackStream"/>.</param>
    /// <exception cref="ArgumentException">Thrown when the compression mode is not valid.</exception>
    /// <exception cref="ArgumentException">Thrown when the base stream is not writable or readable for the specified mode.</exception>
    /// <remarks>
    /// This constructor sets up the RefPack stream for either compression or decompression based on the specified mode.
    /// </remarks>
    public RefPackStream(
        [NotNull] Stream baseStream,
        CompressionMode compressionMode,
        Action<RefPackOptions>? options = null,
        bool leaveOpen = false
    )
    {
#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
        if (compressionMode is not (CompressionMode.Compress or CompressionMode.Decompress))
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
        {
            throw new ArgumentException(
                "The compression mode must be either CompressionMode.Compress or CompressionMode.Decompress.",
                nameof(compressionMode)
            );
        }

        if (compressionMode is CompressionMode.Compress && !baseStream.CanWrite)
        {
            throw new ArgumentException("The base stream must be writable for compression.", nameof(baseStream));
        }

        if (compressionMode is CompressionMode.Decompress && !baseStream.CanRead)
        {
            throw new ArgumentException("The base stream must be readable for decompression.", nameof(baseStream));
        }

        _options = new RefPackOptions();
        options?.Invoke(_options);

        _baseStream = baseStream;
        _leaveOpen = leaveOpen;

        CanRead = compressionMode is CompressionMode.Decompress && baseStream.CanRead;
        CanWrite = compressionMode is CompressionMode.Compress && baseStream.CanWrite;
    }

    /// <inheritdoc/>
    public override bool CanRead { get; }

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite { get; }

    /// <inheritdoc/>
    public override long Length => _baseStream.Length;

    /// <inheritdoc/>
    /// <remarks>
    /// This property does not support setting the position and will throw a <see cref="NotSupportedException"/> if attempted.
    /// </remarks>
    public override long Position
    {
        get => _baseStream.Position;
        set => throw new NotSupportedException($"The {nameof(RefPackStream)} does not support setting position.");
    }

    /// <summary>
    /// Retrieves the size of the decompressed data from a RefPack compressed stream.
    /// </summary>
    /// <param name="refPackStream">The RefPack compressed stream.</param>
    /// <returns>The size of the decompressed data.</returns>
    /// <remarks>
    /// This method reads the size information from the stream header and does not alter the stream's position.
    /// </remarks>
    public static int RetrieveDecompressedSize([NotNull] Stream refPackStream)
    {
        using BinaryReader reader = new(refPackStream, LegacyEncodings.Ansi1252, leaveOpen: true);
        return Decode.RetrieveDecompressedRefPackStreamSize(reader);
    }

    /// <summary>
    /// Flushes the underlying stream.
    /// </summary>
    /// <remarks>
    /// This method flushes any buffered data to the underlying stream.
    /// </remarks>
    /// <exception cref="NotSupportedException">Thrown when the stream is not writable.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the stream has been disposed.</exception>
    public override void Flush()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!CanWrite)
        {
            throw new NotSupportedException($"The {nameof(RefPackStream)} is not writable.");
        }

        _baseStream.Flush();
    }

    /// <summary>
    /// Reads decompressed data from the RefPack stream into the specified buffer.
    /// </summary>
    /// <param name="buffer">The buffer to read the decompressed data into.</param>
    /// <param name="offset">The zero-based byte offset in the buffer at which to begin storing the data read from the stream.</param>
    /// <param name="count">The maximum number of bytes to read.</param>
    /// <returns>The total number of bytes read into the buffer.</returns>
    /// <remarks>
    /// This method decompresses data from the underlying RefPack compressed stream and fills the provided buffer.
    /// </remarks>
    /// <exception cref="InvalidDataException">Thrown when the underlying stream is not a valid RefPack compressed stream.</exception>
    /// <exception cref="NotSupportedException">Thrown when the stream is not readable.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the stream has been disposed.</exception>
    public override int Read([NotNull] byte[] buffer, int offset, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length - offset, count);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!CanRead)
        {
            throw new NotSupportedException($"The {nameof(RefPackStream)} is not readable.");
        }

        using BinaryReader reader = new(_baseStream, LegacyEncodings.Ansi1252, leaveOpen: true);

        if (!Decode.IsValidRefPackStream(reader))
        {
            throw new InvalidDataException("The provided stream is not a valid RefPack compressed stream.");
        }

        List<byte> decompressed = Decode.Decompress(reader);
        var bytesToCopy = int.Min(count, decompressed.Count);
        decompressed.CopyTo(0, buffer, offset, bytesToCopy);
        return bytesToCopy;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This method does not support seeking and will always throw a <see cref="NotSupportedException"/>.
    /// </remarks>
    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException($"The {nameof(RefPackStream)} does not support seeking.");

    /// <inheritdoc/>
    /// <remarks>
    /// This method does not support setting the length and will always throw a <see cref="NotSupportedException"/>.
    /// </remarks>
    public override void SetLength(long value) =>
        throw new NotSupportedException($"The {nameof(RefPackStream)} does not support setting length.");

    /// <summary>
    /// Writes compressed data to the RefPack stream from the specified buffer.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to compress and write.</param>
    /// <param name="offset">The zero-based byte offset in the buffer at which to begin reading data.</param>
    /// <param name="count">The number of bytes to write.</param>
    /// <remarks>
    /// This method compresses data from the provided buffer and writes it to the underlying RefPack compressed stream.
    /// </remarks>
    /// <exception cref="NotSupportedException">Thrown when the stream is not writable.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offset or count are invalid.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the stream has been disposed.</exception>
    public override void Write([NotNull] byte[] buffer, int offset, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length - offset, count);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!CanWrite)
        {
            throw new NotSupportedException($"The {nameof(RefPackStream)} is not writable.");
        }

        using BinaryWriter writer = new(_baseStream, LegacyEncodings.Ansi1252, leaveOpen: true);

        ReadOnlySpan<byte> source = new Span<byte>(buffer, offset, count);
        Encode.Compress(writer, source, _options.QuickCompression);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing && !_leaveOpen)
        {
            _baseStream.Dispose();
        }

        _disposed = true;
        base.Dispose(disposing);
    }
}
