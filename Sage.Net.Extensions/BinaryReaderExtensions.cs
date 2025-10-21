// -----------------------------------------------------------------------
// <copyright file="BinaryReaderExtensions.cs" company="Sage.Net">
// Copyright (c) Sage.Net. All rights reserved.
// Licensed under the MIT license.
// See LICENSE.md for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace Sage.Net.Extensions;

/// <summary>
/// Extension methods for <see cref="BinaryReader"/> to read big-endian and little-endian data.
/// </summary>
public static class BinaryReaderExtensions
{
    /// <summary>
    /// Reads a 16-bit unsigned integer in big-endian byte order.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <returns>The 16-bit unsigned integer read from the stream.</returns>
    /// <exception cref="EndOfStreamException">Thrown when attempting to read beyond the end of the stream.</exception>
    public static ushort ReadUInt16BigEndian([NotNull] this BinaryReader reader)
    {
        if (reader.BaseStream.Position + sizeof(ushort) > reader.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot read beyond the end of the stream.");
        }

        var bytes = reader.ReadBytes(sizeof(ushort));
        return BinaryPrimitives.ReadUInt16BigEndian(bytes);
    }

    /// <summary>
    /// Reads a 16-bit unsigned integer in little-endian byte order.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <returns>The 16-bit unsigned integer read from the stream.</returns>
    /// <exception cref="EndOfStreamException">Thrown when attempting to read beyond the end of the stream.</exception>
    public static ushort ReadUInt16LittleEndian([NotNull] this BinaryReader reader)
    {
        if (reader.BaseStream.Position + sizeof(ushort) > reader.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot read beyond the end of the stream.");
        }

        var bytes = reader.ReadBytes(sizeof(ushort));
        return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
    }

    /// <summary>
    /// Reads a 24-bit unsigned integer in big-endian byte order.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <returns>The 24-bit unsigned integer read from the stream.</returns>
    /// <exception cref="EndOfStreamException">Thrown when attempting to read beyond the end of the stream.</exception>
    /// <remarks>
    /// Since .NET does not have a built-in 24-bit integer type, this method emulates it using a 32-bit integer and zero padding the most significant byte.
    /// </remarks>
    public static uint ReadUInt24BigEndian([NotNull] this BinaryReader reader)
    {
        if (reader.BaseStream.Position + 3 > reader.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot read beyond the end of the stream.");
        }

        var bytes = reader.ReadBytes(3);
        return (uint)((bytes[0] << 16) | (bytes[1] << 8) | bytes[2]);
    }

    /// <summary>
    /// Reads a 24-bit unsigned integer in little-endian byte order.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <returns>The 24-bit unsigned integer read from the stream.</returns>
    /// <exception cref="EndOfStreamException">Thrown when attempting to read beyond the end of the stream.</exception>
    /// <remarks>
    /// Since .NET does not have a built-in 24-bit integer type, this method emulates it using a 32-bit integer and zero padding the most significant byte.
    /// </remarks>
    public static uint ReadUInt24LittleEndian([NotNull] this BinaryReader reader)
    {
        if (reader.BaseStream.Position + 3 > reader.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot read beyond the end of the stream.");
        }

        var bytes = reader.ReadBytes(3);
        return (uint)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16));
    }

    /// <summary>
    /// Reads a 32-bit unsigned integer in big-endian byte order.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <returns>The 32-bit unsigned integer read from the stream.</returns>
    /// <exception cref="EndOfStreamException">Thrown when attempting to read beyond the end of the stream.</exception>
    public static uint ReadUInt32BigEndian([NotNull] this BinaryReader reader)
    {
        if (reader.BaseStream.Position + sizeof(uint) > reader.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot read beyond the end of the stream.");
        }

        var bytes = reader.ReadBytes(sizeof(uint));
        return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    /// <summary>
    /// Reads a 32-bit unsigned integer in little-endian byte order.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <returns>The 32-bit unsigned integer read from the stream.</returns>
    /// <exception cref="EndOfStreamException">Thrown when attempting to read beyond the end of the stream.</exception>
    public static uint ReadUInt32LittleEndian([NotNull] this BinaryReader reader)
    {
        if (reader.BaseStream.Position + sizeof(uint) > reader.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot read beyond the end of the stream.");
        }

        var bytes = reader.ReadBytes(sizeof(uint));
        return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    }

    /// <summary>
    /// Writes a 16-bit unsigned integer in big-endian byte order.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    /// <param name="value">The 16-bit unsigned integer to write.</param>
    /// <exception cref="EndOfStreamException">Thrown when attempting to write beyond the end of the stream.</exception>
    public static void WriteUInt16BigEndian([NotNull] this BinaryWriter writer, ushort value)
    {
        if (writer.BaseStream.Position + sizeof(ushort) > writer.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot write beyond the end of the stream.");
        }

        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
        writer.Write(bytes);
    }

    /// <summary>
    /// Writes a 16-bit unsigned integer in little-endian byte order.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    /// <param name="value">The 16-bit unsigned integer to write.</param>
    /// <exception cref="EndOfStreamException">Thrown when attempting to write beyond the end of the stream.</exception>
    public static void WriteUInt16LittleEndian([NotNull] this BinaryWriter writer, ushort value)
    {
        if (writer.BaseStream.Position + sizeof(ushort) > writer.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot write beyond the end of the stream.");
        }

        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, value);
        writer.Write(bytes);
    }

    /// <summary>
    /// Writes a 24-bit unsigned integer in big-endian byte order.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    /// <param name="value">The 24-bit unsigned integer to write.</param>
    /// <exception cref="EndOfStreamException">Thrown when attempting to write beyond the end of the stream.</exception>
    /// <remarks>
    /// Since .NET does not have a built-in 24-bit integer type, this method emulates it using a 32-bit integer and ignoring the most significant byte.
    /// </remarks>
    public static void WriteUInt24BigEndian([NotNull] this BinaryWriter writer, uint value)
    {
        if (writer.BaseStream.Position + 3 > writer.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot write beyond the end of the stream.");
        }

        Span<byte> bytes = [(byte)(value >> 16), (byte)(value >> 8), (byte)value];
        writer.Write(bytes);
    }

    /// <summary>
    /// Writes a 24-bit unsigned integer in little-endian byte order.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    /// <param name="value">The 24-bit unsigned integer to write.</param>
    /// <exception cref="EndOfStreamException">Thrown when attempting to write beyond the end of the stream.</exception>
    /// <remarks>
    /// Since .NET does not have a built-in 24-bit integer type, this method emulates it using a 32-bit integer and ignoring the most significant byte.
    /// </remarks>
    public static void WriteUInt24LittleEndian([NotNull] this BinaryWriter writer, uint value)
    {
        if (writer.BaseStream.Position + 3 > writer.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot write beyond the end of the stream.");
        }

        Span<byte> bytes = [(byte)value, (byte)(value >> 8), (byte)(value >> 16)];
        writer.Write(bytes);
    }

    /// <summary>
    /// Writes a 32-bit unsigned integer in big-endian byte order.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    /// <param name="value">The 32-bit unsigned integer to write.</param>
    /// <exception cref="EndOfStreamException">Thrown when attempting to write beyond the end of the stream.</exception>
    public static void WriteUInt32BigEndian([NotNull] this BinaryWriter writer, uint value)
    {
        if (writer.BaseStream.Position + sizeof(uint) > writer.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot write beyond the end of the stream.");
        }

        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
        writer.Write(bytes);
    }

    /// <summary>
    /// Writes a 32-bit unsigned integer in little-endian byte order.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    /// <param name="value">The 32-bit unsigned integer to write.</param>
    /// <exception cref="EndOfStreamException">Thrown when attempting to write beyond the end of the stream.</exception>
    public static void WriteUInt32LittleEndian([NotNull] this BinaryWriter writer, uint value)
    {
        if (writer.BaseStream.Position + sizeof(uint) > writer.BaseStream.Length)
        {
            throw new EndOfStreamException("Cannot write beyond the end of the stream.");
        }

        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        writer.Write(bytes);
    }
}
