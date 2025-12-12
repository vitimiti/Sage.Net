// -----------------------------------------------------------------------
// <copyright file="Color.cs" company="Sage.Net">
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

namespace Sage.Net.GameEngine.GameLogic;

/// <summary>Represents a color value stored as a packed 32-bit ARGB integer.</summary>
/// <param name="Value">The packed ARGB <see cref="int"/> value for this color.</param>
/// <remarks>The layout is <b>ARGB</b> with one byte (8 bits) per component: Alpha, Red, Green, Blue. Each component ranges from <c>0x00</c> to <c>0xFF</c> inclusive, where alpha <c>0x00</c> is fully transparent and <c>0xFF</c> is fully opaque.</remarks>
/// <example>
/// Packed representation (most significant byte first):
/// <code>
///   | ---------- Alpha (AA)
///   |  | ------- Red   (RR)
///   |  |  | ---- Green (GG)
///   |  |  |  | - Blue  (BB)
/// 0x|AA|RR|GG|BB
/// </code>
/// For example, opaque white is <c>0xFFFF_FFFF</c>, fully transparent white is <c>0x00FF_FFFF</c>.
/// </example>
public record Color(int Value)
{
    /// <summary>Gets the undefined color.</summary>
    /// <remarks>The undefined color is <c>0x00FF_FFFF</c> (ARGB), i.e., white with alpha <c>0x00</c> (fully transparent).</remarks>
    public static Color Undefined => new(0x00FF_FFFF);

    /// <summary>Gets the color components as bytes.</summary>
    /// <value>A tuple of (<c>Red</c>, <c>Green</c>, <c>Blue</c>, <c>Alpha</c>), each in the range <c>0..255</c>.</value>
    public (byte Red, byte Green, byte Blue, byte Alpha) Components
    {
        get
        {
            var alpha = (byte)(((Value & 0xFF00_0000) >> 24) & 0xFF);
            var red = (byte)(((Value & 0x00FF_0000) >> 16) & 0xFF);
            var green = (byte)(((Value & 0x0000_FF00) >> 8) & 0xFF);
            var blue = (byte)(Value & 0x0000_00FF & 0xFF);

            return (red, green, blue, alpha);
        }
    }

    /// <summary>Gets the color components normalized to single-precision floating-point values.</summary>
    /// <value>A tuple of (<c>Red</c>, <c>Green</c>, <c>Blue</c>, <c>Alpha</c>) where each component is in the range <c>0F..1F</c>.</value>
    public (float Red, float Green, float Blue, float Alpha) ComponentsSingle
    {
        get
        {
#pragma warning disable IDE0008 // Use explicit type instead of 'var'
            var (red, green, blue, alpha) = Components;
#pragma warning restore IDE0008 // Use explicit type instead of 'var'

            return (red / 255F, green / 255F, blue / 255F, alpha / 255F);
        }
    }

    /// <summary>Creates a new <see cref="Color"/> from components.</summary>
    /// <param name="red">Red component (0–255).</param>
    /// <param name="green">Green component (0–255).</param>
    /// <param name="blue">Blue component (0–255).</param>
    /// <param name="alpha">Alpha (opacity) component (0–255), where 0 is fully transparent and 255 is fully opaque.</param>
    /// <returns>A new <see cref="Color"/> with the specified components.</returns>
    /// <remarks>Components are packed into an ARGB 32-bit integer as <c>(alpha <![CDATA[<]]><![CDATA[<]]> 24) | (red <![CDATA[<]]><![CDATA[<]]> 16) | (green <![CDATA[<]]><![CDATA[<]]> 8) | blue</c>.</remarks>
    public static Color MakeColor(byte red, byte green, byte blue, byte alpha) =>
        new((alpha << 24) | (red << 16) | (green << 8) | blue);

    /// <summary>Darkens the color by the specified percentage.</summary>
    /// <param name="percent">An integer percentage (1–89) by which to darken the RGB channels.</param>
    /// <returns>A new <see cref="Color"/> with the darkened color. If <paramref name="percent"/> is ≤ 0 or ≥ 90, the original color is returned unchanged.</returns>
    /// <remarks>Darkening is applied independently to the Red, Green, and Blue components using integer arithmetic. The Alpha component is preserved.</remarks>
    public Color Darken(int percent = 10)
    {
        if (percent is >= 90 or <= 0)
        {
            return this;
        }

#pragma warning disable IDE0008 // Use explicit type instead of 'var'
        var (red, green, blue, alpha) = Components;
#pragma warning restore IDE0008 // Use explicit type instead of 'var'

        var r = red - (red * percent / 100);
        var g = green - (green * percent / 100);
        var b = blue - (blue * percent / 100);

        return MakeColor((byte)r, (byte)g, (byte)b, alpha);
    }
}
