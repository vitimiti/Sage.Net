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

namespace Sage.Net.Generals.GameEngine.GameClient;

/// <summary>Represents an ARGB color packed into a single 32-bit signed <see cref="int"/>.</summary>
/// <remarks>
/// The <see cref="Value"/> packs channels in ARGB order using 8 bits per channel (<c>0xAARR_GGBB</c>):
/// <code>
/// 31                24 23                16 15                 8 7                 0
/// +-------------------+-------------------+---------------------+--------------------+
/// |      Alpha (A)    |       Red (R)     |      Green (G)      |      Blue (B)      |
/// +-------------------+-------------------+---------------------+--------------------+
/// </code>
/// In other words:
/// <list type="bullet">
/// <item><description>Bits 31..24 store the alpha component.</description></item>
/// <item><description>Bits 23..16 store the red component.</description></item>
/// <item><description>Bits 15..8 store the green component.</description></item>
/// <item><description>Bits 7..0 store the blue component.</description></item>
/// </list>
/// Channel accessors shift and mask <see cref="Value"/> accordingly, and <see cref="Make(byte, byte, byte, byte)"/> constructs a packed value as <c>(alpha <![CDATA[<]]><![CDATA[<]]> 24) | (red <![CDATA[<]]><![CDATA[<]]> 16) | (green <![CDATA[<]]><![CDATA[<]]> 8) | blue</c>.
/// </remarks>
public record Color(int Value)
{
    /// <summary>Gets the red channel (0..255) from <see cref="Value"/>.</summary>
    public byte Red => (byte)((Value >> 16) & 0x00FF);

    /// <summary>Gets the green channel (0..255) from <see cref="Value"/>.</summary>
    public byte Green => (byte)((Value >> 8) & 0x00FF);

    /// <summary>Gets the blue channel (0..255) from <see cref="Value"/>.</summary>
    public byte Blue => (byte)(Value & 0x00FF);

    /// <summary>Gets the alpha channel (0..255) from <see cref="Value"/>.</summary>
    public byte Alpha => (byte)((Value >> 24) & 0x00FF);

    /// <summary>Gets the red channel normalized to 0..1.</summary>
    public float FRed => Red / 255F;

    /// <summary>Gets the green channel normalized to 0..1.</summary>
    public float FGreen => Green / 255F;

    /// <summary>Gets the blue channel normalized to 0..1.</summary>
    public float FBlue => Blue / 255F;

    /// <summary>Gets the alpha channel normalized to 0..1.</summary>
    public float FAlpha => Alpha / 255F;

    /// <summary>Creates a <see cref="Color"/> from 8-bit ARGB channels.</summary>
    /// <param name="red">Red channel (0..255).</param>
    /// <param name="green">Green channel (0..255).</param>
    /// <param name="blue">Blue channel (0..255).</param>
    /// <param name="alpha">Alpha channel (0..255).</param>
    /// <returns>A new <see cref="Color"/> with channels packed into <see cref="Value"/>.</returns>
    public static Color Make(byte red, byte green, byte blue, byte alpha) =>
        new((alpha << 24) | (red << 16) | (green << 8) | blue);

    /// <summary>Darkens the specified color by a percentage of its RGB channels, preserving alpha.</summary>
    /// <param name="color">The input color.</param>
    /// <param name="percent">The percentage by which to darken each RGB component. Values less than or equal to 0, or greater than or equal to 90, return the input color unchanged.</param>
    /// <returns>The darkened color.</returns>
    public static Color Darken(Color color, int percent)
    {
        ArgumentNullException.ThrowIfNull(color);

        if (percent is >= 90 or <= 0)
        {
            return color;
        }

        var r = color.Red;
        var g = color.Green;
        var b = color.Blue;

        r -= (byte)(r * percent / 100);
        g -= (byte)(g * percent / 100);
        b -= (byte)(b * percent / 100);

        return Make(r, g, b, color.Alpha);
    }
}
