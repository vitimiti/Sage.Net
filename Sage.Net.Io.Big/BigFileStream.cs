// -----------------------------------------------------------------------
// <copyright file="BigFileStream.cs" company="Sage.Net">
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

namespace Sage.Net.Io.Big;

/// <summary>
/// Represents a stream for reading a portion of a larger stream, commonly
/// used when accessing a segment of data from a BIG archive file.
/// </summary>
/// <remarks>
/// This stream operates on a parent stream, restricting the access
/// to a specific range defined by a start offset and length. The data
/// within this range can be read sequentially or accessed via seeking.
/// Note that this stream is read-only and does not support writing
/// or modifying the underlying data.
/// </remarks>
/// <threadsafety>
/// Thread-safe for concurrent read operations due to internal locking when
/// accessing the parent stream.
/// </threadsafety>
/// <exception cref="NotSupportedException">
/// Thrown by <see cref="SetLength"/> and <see cref="Write"/> methods as this
/// stream does not support writing or resizing.
/// </exception>
/// <param name="parent">
/// The underlying parent stream providing the data.
/// </param>
/// <param name="startOffset">
/// The starting byte offset in the parent stream where the readable portion begins.
/// </param>
/// <param name="length">
/// The total number of bytes available for reading within this stream.
/// </param>
public sealed class BigFileStream(FileStream parent, long startOffset, long length) : Stream
{
    private long _position;

    /// <summary>
    /// Gets a value indicating whether the current stream supports reading.
    /// </summary>
    public override bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether the current stream supports seeking.
    /// </summary>
    public override bool CanSeek => true;

    /// <summary>
    /// Gets a value indicating whether the current stream supports writing.
    /// </summary>
    public override bool CanWrite => false;

    /// <summary>
    /// Gets the total number of bytes in the stream.
    /// </summary>
    public override long Length => length;

    /// <summary>
    /// Gets or sets the current position within the stream.
    /// </summary>
    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    /// <summary>
    /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <param name="buffer">The array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
    /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
    /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero if the end of the stream has been reached.</returns>
    public override int Read(byte[] buffer, int offset, int count)
    {
        var remaining = length - _position;
        if (remaining <= 0)
        {
            return 0;
        }

        if (count > remaining)
        {
            count = (int)remaining;
        }

        var read = RandomAccess.Read(parent.SafeFileHandle!, buffer.AsSpan(offset, count), startOffset + _position);
        _position += read;
        return read;
    }

    /// <summary>
    /// Sets the position within the stream to a new value specified by an offset and origin.
    /// </summary>
    /// <param name="offset">The byte offset relative to the origin parameter. Positive values move forward in the stream, and negative values move backward.</param>
    /// <param name="origin">A value of type <see cref="SeekOrigin"/> which acts as a reference point for the offset. It can be one of: Begin, Current, or End.</param>
    /// <returns>The new position within the stream, constrained to be within the bounds of the stream.</returns>
    public override long Seek(long offset, SeekOrigin origin)
    {
        var newPos = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => length + offset,
            _ => _position,
        };

        _position = long.Clamp(newPos, 0, length);
        return _position;
    }

    /// <summary>
    /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
    /// </summary>
    /// <remarks>
    /// As this stream is read-only, this method does not perform any operations but is overridden to fulfill the abstract class requirement.
    /// </remarks>
    public override void Flush() { }

    /// <summary>
    /// Sets the length of the stream to the specified value. This operation
    /// is not supported for this stream and will always throw a <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="value">The desired length of the stream, in bytes. This parameter is not used as the operation is unsupported.</param>
    /// <exception cref="NotSupportedException">Thrown unconditionally as the stream does not support setting its length.</exception>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <summary>
    /// Writes a sequence of bytes to the current stream and advances the position within the stream by the number of bytes written.
    /// </summary>
    /// <param name="buffer">The array of bytes to be written to the current stream.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin reading bytes to write to the stream.</param>
    /// <param name="count">The number of bytes to write to the stream.</param>
    /// <exception cref="NotSupportedException">Thrown unconditionally as the stream does not support writing.</exception>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
