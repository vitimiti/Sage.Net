// -----------------------------------------------------------------------
// <copyright file="BinaryReaderExtensions.cs" company="Sage.Net">
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
using System.Text;

namespace Sage.Net.Io.Extensions;

/// <summary>
/// Provides extension methods for reading primitive types from a <see cref="BinaryReader"/>.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "This is a false positive.")]
[SuppressMessage(
    "csharpsquid",
    "S1144:Unused private types or members should be removed",
    Justification = "Extension methods."
)]
public static class BinaryReaderExtensions
{
    /// <summary>
    /// Extension methods for reading primitive types from a <see cref="BinaryReader"/>.
    /// </summary>
    extension(BinaryReader reader)
    {
        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream in big-endian format.
        /// </summary>
        /// <returns>A 4-byte unsigned integer read from the stream in big-endian order.</returns>
        public uint ReadUInt32BigEndian() => BinaryPrimitives.ReadUInt32BigEndian(reader.ReadBytes(4));

        /// <summary>
        /// Reads a null-terminated string from the current stream.
        /// </summary>
        /// <returns>The string read from the stream, up to but not including the null terminator.
        /// If the stream ends before a null terminator is encountered, the string read until that point is returned.
        /// </returns>
        public string ReadNullTerminatedString()
        {
            StringBuilder sb = new();
            char c;
            while ((c = (char)reader.ReadByte()) != '\0')
            {
                _ = sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
