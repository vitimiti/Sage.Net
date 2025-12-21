// -----------------------------------------------------------------------
// <copyright file="BinaryWriterExtensions.cs" company="Sage.Net">
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

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace Sage.Net.Io.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="System.IO.BinaryWriter"/> class,
/// enabling additional functionality for working with binary data streams.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "This is a false positive.")]
[SuppressMessage(
    "csharpsquid",
    "S1144:Unused private types or members should be removed",
    Justification = "Extension methods."
)]
public static class BinaryWriterExtensions
{
    /// <summary>
    /// Extension methods for the <see cref="System.IO.BinaryWriter"/> class,
    /// </summary>
    extension(BinaryWriter writer)
    {
        /// <summary>
        /// Writes a 32-bit signed integer to the current stream in big-endian format.
        /// </summary>
        /// <param name="value">The 32-bit signed integer to write to the stream.</param>
        public void WriteInt32BigEndian(int value)
        {
            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, value);
            writer.Write(span);
        }

        /// <summary>
        /// Writes a 32-bit unsigned integer to the current stream in big-endian format.
        /// </summary>
        /// <param name="value">The 32-bit unsigned integer to write to the stream.</param>
        public void WriteUInt32BigEndian(uint value)
        {
            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(span, value);
            writer.Write(span);
        }
    }
}
