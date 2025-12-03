// -----------------------------------------------------------------------
// <copyright file="Image.cs" company="Sage.Net">
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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Sage.Net.BaseTypes;
using Sage.Net.Core.GameEngine.Common.GameMemory;
using Sage.Net.Core.GameEngine.Common.Ini;

namespace Sage.Net.Core.GameEngine.Common.Pools;

/// <summary>
/// Image data.
/// </summary>
public class Image : IPooledObject
{
    /// <summary>
    /// Gets the field parsers.
    /// </summary>
    public static IReadOnlyList<FieldParse> FieldParsers => FieldParseTable;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the texture size.
    /// </summary>
    public Coord2D TextureSize { get; set; } = new(0, 0);

    /// <summary>
    /// Gets or sets the UV coordinates.
    /// </summary>
    public FRegion2D UvCoordinates { get; set; } = new(new FCoord2D(0, 0), new FCoord2D(1, 1));

    /// <summary>
    /// Gets or sets the size.
    /// </summary>
    public Coord2D Size { get; set; } = new(0, 0);

    /// <summary>
    /// Gets the width.
    /// </summary>
    public int Width => Size.X;

    /// <summary>
    /// Gets the height.
    /// </summary>
    public int Height => Size.Y;

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public ImageStatus Status { get; set; } = ImageStatus.None;

    /// <summary>
    /// Gets or sets the raw texture data.
    /// </summary>
    [SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "This is raw byte data."
    )]
    public byte[]? RawTextureData { get; set; }

    /// <summary>
    /// Gets the field parse table.
    /// </summary>
    protected static IReadOnlyList<FieldParse> FieldParseTable =>
        [
            new("Texture", null, IniParser.ParseString),
            new("TextureWidth", null, IniParser.ParseInt32),
            new("TextureHeight", null, IniParser.ParseInt32),
            new("Coords", null, ParseImageCoords),
            new("Status", null, ParseImageStatus),
        ];

    /// <summary>
    /// Parses the image coords.
    /// </summary>
    /// <param name="ini">The <see cref="IniReader"/> instance.</param>
    /// <param name="instance">The instance.</param>
    /// <param name="store">The store.</param>
    /// <param name="userData">The user data.</param>
    /// <remarks>
    /// <para>This parser expects <paramref name="instance"/> to be a <see cref="Image"/>.</para>
    /// <para>This parser does not use <paramref name="store"/> or <paramref name="userData"/>.</para>
    /// </remarks>
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "This is for the FieldParse delegate.")]
    public static void ParseImageCoords(
        [NotNull] IniReader ini,
        ref object? instance,
        ref object? store,
        object? userData
    )
    {
        var left = IniReader.ScanInt32(ini.GetNextToken());
        var top = IniReader.ScanInt32(ini.GetNextToken());
        var right = IniReader.ScanInt32(ini.GetNextToken());
        var bottom = IniReader.ScanInt32(ini.GetNextToken());

        if (instance is not Image theImage)
        {
            Debug.Fail($"{nameof(instance)} is not an {nameof(Image)} and cannot be parsed.");
            return;
        }

        FRegion2D uvCoords = new(new FCoord2D(left, top), new FCoord2D(right, bottom));
        Coord2D textureSize = theImage.TextureSize;
        if (textureSize.X != 0)
        {
            uvCoords = uvCoords with
            {
                Low = new FCoord2D(uvCoords.Low.X / textureSize.X, uvCoords.Low.Y),
                High = new FCoord2D(uvCoords.High.X / textureSize.X, uvCoords.High.Y),
            };
        }

        if (textureSize.Y != 0)
        {
            uvCoords = uvCoords with
            {
                Low = new FCoord2D(uvCoords.Low.X, uvCoords.Low.Y / textureSize.Y),
                High = new FCoord2D(uvCoords.High.X, uvCoords.High.Y / textureSize.Y),
            };
        }

        theImage.UvCoordinates = uvCoords;
    }

    /// <summary>
    /// Parses the image status.
    /// </summary>
    /// <param name="ini">The <see cref="IniReader"/> instance.</param>
    /// <param name="instance">The instance.</param>
    /// <param name="store">The store.</param>
    /// <param name="userData">The user data.</param>
    /// <remarks>
    /// <para>This parser expects <paramref name="instance"/> to be a <see cref="Image"/>.</para>
    /// <para>This parser expects <paramref name="store"/> to be a <see cref="int"/>.</para>
    /// <para>This parser expects <paramref name="userData"/> to be a <see cref="List{T}"/> of <see cref="string"/>s.</para>
    /// </remarks>
    public static void ParseImageStatus(
        [NotNull] IniReader ini,
        ref object? instance,
        ref object? store,
        object? userData
    )
    {
        IniParser.ParseBitString32(ini, ref instance, ref store, userData);
        var bits = (ImageStatus)(store ?? 0);
        if (!bits.HasFlag(ImageStatus.Rotated90Clockwise))
        {
            return;
        }

        if (instance is not Image theImage)
        {
            Debug.Fail($"{nameof(instance)} is not an {nameof(Image)} and cannot be parsed.");
            return;
        }

        Coord2D imageSize = new(theImage.Height, theImage.Width);
        theImage.Size = imageSize;
        instance = theImage;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        Name = string.Empty;
        FileName = string.Empty;
        TextureSize = new Coord2D(0, 0);
        UvCoordinates = new FRegion2D(new FCoord2D(0, 0), new FCoord2D(1, 1));
        Size = new Coord2D(0, 0);
        RawTextureData = null;
        Status = ImageStatus.None;
    }
}
