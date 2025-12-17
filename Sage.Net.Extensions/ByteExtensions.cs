// -----------------------------------------------------------------------
// <copyright file="ByteExtensions.cs" company="Sage.Net">
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

/// <summary>Extension methods for <see cref="byte"/>.</summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "This is a false positive.")]
[SuppressMessage(
    "csharpsquid",
    "S1144: Unused private types or members should be removed",
    Justification = "This is a false positive."
)]
public static class ByteExtensions
{
    /// <summary>Extensions for <see langword="byte"/>.</summary>
    extension(byte value)
    {
        /// <summary>Determines whether the current byte is a space character.</summary>
        /// <returns><see langword="true"/> if the current byte is a space character; otherwise, <see langword="false"/>.</returns>
        public bool IsSpace() =>
            value is (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r' or (byte)'\v' or (byte)'\f';
    }
}
