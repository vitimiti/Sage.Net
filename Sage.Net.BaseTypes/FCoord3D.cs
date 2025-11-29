// -----------------------------------------------------------------------
// <copyright file="FCoord3D.cs" company="Sage.Net">
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
/// A 3D coordinate with <see cref="float"/> precision.
/// </summary>
/// <param name="X">The X coordinate.</param>
/// <param name="Y">The Y coordinate.</param>
/// <param name="Z">The Z coordinate.</param>
public record FCoord3D(float X, float Y, float Z)
{
    /// <summary>
    /// Gets the zero coordinate.
    /// </summary>
    public static FCoord3D Zero => new(0, 0, 0);

    /// <summary>
    /// Gets the length of the <see cref="FCoord3D"/>.
    /// </summary>
    public float Length => float.Sqrt(LengthSqr);

    /// <summary>
    /// Gets the squared length of the <see cref="FCoord3D"/>.
    /// </summary>
    public float LengthSqr => float.Pow(X, 2) + float.Pow(Y, 2) + float.Pow(Z, 2);

    /// <summary>
    /// Gets a normalized <see cref="FCoord3D"/>.
    /// </summary>
    public FCoord3D Normalized
    {
        get
        {
            var length = Length;
            return float.Abs(length) >= float.Epsilon
                ? new FCoord3D(X / length, Y / length, Z / length)
                : new FCoord3D(0, 0, 0);
        }
    }

    /// <summary>
    /// Calculates the cross product of two vectors.
    /// </summary>
    /// <param name="left">The <see cref="FCoord3D"/> on the left side of the cross-product operation.</param>
    /// <param name="right">The <see cref="FCoord3D"/> on the right side of the cross-product operation.</param>
    /// <returns>A new <see cref="FCoord3D"/> resulting from the cross-product operation between the two <see cref="FCoord3D"/> instances.</returns>
    /// <seealso cref="CrossProduct"/>
    public static FCoord3D CrossProduct([NotNull] FCoord3D left, [NotNull] FCoord3D right) =>
        new(
            (left.Y * right.Z) - (left.Z * right.Y),
            (left.Z * right.X) - (left.X * right.Z),
            (left.X * right.Y) - (left.Y * right.X)
        );

    /// <summary>
    /// Scales the <see cref="FCoord3D"/>.
    /// </summary>
    /// <param name="scale">A <see cref="float"/> with the scale to apply to the coordinate.</param>
    /// <returns>A new <see cref="FCoord3D"/> with the given <paramref name="scale"/>.</returns>
    public FCoord3D Scaled(float scale) => new(X * scale, Y * scale, Z * scale);

    /// <summary>
    /// Adds two <see cref="FCoord3D"/> instances.
    /// </summary>
    /// <param name="other">The other <see cref="FCoord3D"/> to apply the addition operation with.</param>
    /// <returns>A new <see cref="FCoord3D"/> resulting from the addition of the two <see cref="FCoord3D"/> instances.</returns>
    public FCoord3D Add([NotNull] FCoord3D other) => new(X + other.X, Y + other.Y, Z + other.Z);

    /// <summary>
    /// Subtracts two <see cref="FCoord3D"/> instances.
    /// </summary>
    /// <param name="other">The other <see cref="FCoord3D"/> to apply the subtraction operation with.</param>
    /// <returns>A new <see cref="FCoord3D"/> resulting from the subtraction of the two <see cref="FCoord3D"/> instances.</returns>
    public FCoord3D Subtract([NotNull] FCoord3D other) => new(X - other.X, Y - other.Y, Z - other.Z);

    /// <summary>
    /// Multiply two <see cref="FCoord3D"/> instances.
    /// </summary>
    /// <param name="other">The other <see cref="FCoord3D"/> to apply the multiplication operation with.</param>
    /// <returns>A new <see cref="FCoord3D"/> resulting from the multiplication of the two <see cref="FCoord3D"/> instances.</returns>
    /// <remarks>This is a cross-product operation and can be achieved using <see cref="CrossProduct"/>.</remarks>
    /// <seealso cref="CrossProduct"/>
    public FCoord3D Multiply([NotNull] FCoord3D other) => CrossProduct(this, other);

    /// <summary>
    /// The addition operator.
    /// </summary>
    /// <param name="left">The left sided <see cref="FCoord3D"/>.</param>
    /// <param name="right">The right sided <see cref="FCoord3D"/>.</param>
    /// <returns>A new <see cref="FCoord3D"/> resulting from the addition of <paramref name="left"/> and <paramref name="right"/>.</returns>
    /// <seealso cref="Add"/>
    public static FCoord3D operator +([NotNull] FCoord3D left, FCoord3D right) => left.Add(right);

    /// <summary>
    /// The subtraction operator.
    /// </summary>
    /// <param name="left">The left sided <see cref="FCoord3D"/>.</param>
    /// <param name="right">The right sided <see cref="FCoord3D"/>.</param>
    /// <returns>A new <see cref="FCoord3D"/> resulting from the subtraction of <paramref name="left"/> and <paramref name="right"/>.</returns>
    /// <seealso cref="Subtract"/>
    public static FCoord3D operator -([NotNull] FCoord3D left, FCoord3D right) => left.Subtract(right);

    /// <summary>
    /// The multiplication operator.
    /// </summary>
    /// <param name="left">The left sided <see cref="FCoord3D"/>.</param>
    /// <param name="right">The right sided <see cref="FCoord3D"/>.</param>
    /// <returns>A new <see cref="FCoord3D"/> resulting from the multiplication of <paramref name="left"/> and <paramref name="right"/>.</returns>
    /// <seealso cref="Multiply"/>
    public static FCoord3D operator *([NotNull] FCoord3D left, FCoord3D right) => left.Multiply(right);
}
