// -----------------------------------------------------------------------
// <copyright file="RgbColor.cs" company="Sage.Net">
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

namespace Sage.Net.Generals.Libraries.BaseTypes;

/// <summary>Represents a color in the RGB color space.</summary>
/// <param name="Red">The red component of the color.</param>
/// <param name="Green">The green component of the color.</param>
/// <param name="Blue">The blue component of the color.</param>
public record RgbColor(float Red, float Green, float Blue)
{
    /// <summary>Converts a 32-bit integer representation of a color into an <see cref="RgbColor"/> instance.</summary>
    /// <param name="color">A 32-bit integer representing the color, where the red, green, and blue components are packed into the higher, middle, and lower 8 bits, respectively.</param>
    /// <returns>An <see cref="RgbColor"/> instance representing the color defined by the input integer.</returns>
    public static RgbColor FromInt32(int color) =>
        new(((color >> 16) & 0x00FF) / 255F, ((color >> 8) & 0x00FF) / 255F, (color & 0x00FF) / 255F);

    /// <summary>Converts the current <see cref="RgbColor"/> instance into a 32-bit integer representation.</summary>
    /// <returns>A 32-bit integer where the red, green, and blue components are packed into the higher, middle, and lower 8 bits, respectively.</returns>
    public int ToInt32() => ((int)(Red * 255) << 16) | ((int)(Green * 255) << 8) | (int)(Blue * 255);

    /// <summary>Explicitly converts an <see cref="RgbColor"/> instance to its 32-bit integer representation.</summary>
    /// <param name="color">The <see cref="RgbColor"/> instance to convert.</param>
    /// <returns>A 32-bit integer where the red, green, and blue components of the color are packed into the higher, middle, and lower 8 bits, respectively.</returns>
    public static explicit operator int(RgbColor color)
    {
        ArgumentNullException.ThrowIfNull(color);

        return color.ToInt32();
    }

    /// <summary>Explicitly converts a 32-bit integer representation of a color to an <see cref="RgbColor"/> instance.</summary>
    /// <param name="color">A 32-bit integer where the red, green, and blue components are packed into the higher, middle, and lower 8 bits, respectively.</param>
    /// <returns>An <see cref="RgbColor"/> instance representing the color defined by the input integer.</returns>
    public static explicit operator RgbColor(int color) => FromInt32(color);
}
