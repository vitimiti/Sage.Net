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

using System.Diagnostics.CodeAnalysis;

namespace Sage.Net.BaseTypes;

/// <summary>
/// Represents a color in RGB format.
/// </summary>
/// <param name="Red">The red component.</param>
/// <param name="Green">The green component.</param>
/// <param name="Blue">The blue component.</param>
public record RgbColor(float Red, float Green, float Blue)
{
    /// <summary>
    /// Creates a new <see cref="RgbColor"/> from an integer value.
    /// </summary>
    /// <param name="value">The <see cref="int"/> to create the <see cref="RgbColor"/> from.</param>
    /// <returns>A new <see cref="RgbColor"/> from the given <paramref name="value"/>.</returns>
    /// <remarks>The integer value is composed of the red, green and blue components in that order, with unused alpha component: <c>0x00RRGGBB</c>.</remarks>
    /// <seealso cref="ToInt32"/>
    /// <example>
    /// <code>
    /// var red = RgbColor.FromInt32(0x00FF0000);
    /// var green = RgbColor.FromInt32(0x0000FF00);
    /// var blue = RgbColor.FromInt32(0x000000FF);
    /// Debug.Assert(red == new RgbColor(1F, 0F, 0F));
    /// Debug.Assert(green == new RgbColor(0F, 1F, 0F));
    /// Debug.Assert(blue == new RgbColor(0F, 0F, 1F));
    /// </code>
    /// </example>
    public static RgbColor FromInt32(int value) =>
        new(((value >> 16) & 0xFF) / 255F, ((value >> 8) & 0xFF) / 255F, (value & 0xFF) / 255F);

    /// <summary>
    /// Converts the <see cref="RgbColor"/> to an integer value.
    /// </summary>
    /// <returns>A <see cref="int"/> that represents the <see cref="RgbColor"/>.</returns>
    /// <remarks>The integer value is composed of the red, green and blue components in that order, with unused alpha component: <c>0x00RRGGBB</c>.</remarks>
    /// <seealso cref="FromInt32"/>
    /// <example>
    /// <code>
    /// RgbColor red = new(1F, 0F, 0F);
    /// RgbColor green = new(1F, 0F, 0F);
    /// RgbColor blue = new(1F, 0F, 0F);
    /// Debug.Assert(red.ToInt32() == 0x00FF0000);
    /// Debug.Assert(green.ToInt32() == 0x0000FF00);
    /// Debug.Assert(blue.ToInt32() == 0x000000FF);
    /// </code>
    /// </example>
    public int ToInt32() =>
        (((int)(Red * 255) & 0xFF) << 16) | (((int)(Green * 255) & 0xFF) << 8) | ((int)(Blue * 255) & 0xFF);

    /// <summary>
    /// Explicit conversion operator to <see cref="int"/>.
    /// </summary>
    /// <param name="color">The <see cref="RgbColor"/> to convert to.</param>
    /// <returns>A new <see cref="int"/> representing the <see cref="RgbColor"/>.</returns>
    /// <seealso cref="FromInt32"/>
    public static explicit operator int([NotNull] RgbColor color) => color.ToInt32();

    /// <summary>
    /// Explicit conversion operator from <see cref="int"/>.
    /// </summary>
    /// <param name="value">The <see cref="int"/> to convert from.</param>
    /// <returns>A new <see cref="RgbColor"/> from the given <paramref name="value"/>.</returns>
    /// <seealso cref="ToInt32"/>
    public static explicit operator RgbColor(int value) => FromInt32(value);
}
