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

using System.Diagnostics.CodeAnalysis;

namespace Sage.Net.Extensions;

/// <summary><see cref="BinaryReader"/> extensions.</summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "This is a false positive.")]
public static class BinaryReaderExtensions
{
    /// <summary>Adds extensions to the <see cref="BinaryReader"/> class.</summary>
    extension(BinaryReader reader)
    {
        /// <summary>Tries to read a wide character.</summary>
        /// <param name="c">A <see cref="char"/> populated with the read wide character; <c>\0</c> otherwise.</param>
        /// <returns><see langword="true"/> if the character could be read; otherwise <see langword="false"/>.</returns>
        public bool TryReadWideChar(out char c)
        {
            c = '\0';
            if (reader.BaseStream.Position + sizeof(ushort) > reader.BaseStream.Length)
            {
                return false;
            }

            var u = reader.ReadUInt16();
            c = (char)u;
            return true;
        }

        /// <summary>Tries to read a word.</summary>
        /// <param name="word">A <see cref="IList{T}"/> of <see cref="char"/> that will be populated with the word characters.</param>
        /// <returns><see langword="true"/> if the <paramref name="word"/> could be populated; otherwise <see langword="false"/>.</returns>
        public bool TryReadWord(out IList<char> word)
        {
            word = new List<char>(128);
            if (!TryReadWideChar(reader, out var first))
            {
                return false;
            }

            if (first == ' ')
            {
                return true;
            }

            word.Add(first);
            var result = true;
            while (true)
            {
                if (!TryReadWideChar(reader, out var c))
                {
                    result = false;
                    break;
                }

                if (c == ' ')
                {
                    break;
                }

                word.Add(c);
            }

            return result;
        }
    }
}
