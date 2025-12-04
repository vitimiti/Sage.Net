// -----------------------------------------------------------------------
// <copyright file="FileInformation.cs" company="Sage.Net">
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

namespace Sage.Net.GameEngine.Common.Io;

/// <summary>
/// Represents the file information.
/// </summary>
/// <param name="SizeHigh">The high 32 bits of the size.</param>
/// <param name="SizeLow">The low 32 bits of the size.</param>
/// <param name="TimestampHigh">The high 32 bits of the timestamp.</param>
/// <param name="TimestampLow">The low 32 bits of the timestamp.</param>
public record FileInformation(int SizeHigh, int SizeLow, int TimestampHigh, int TimestampLow)
{
    /// <summary>
    /// Gets the size of the file.
    /// </summary>
    public long Size => ((long)SizeHigh << 32) | (uint)SizeLow;

    /// <summary>
    /// Gets the timestamp of the file.
    /// </summary>
    public long Timestamp => ((long)TimestampHigh << 32) | (uint)TimestampLow;
}
