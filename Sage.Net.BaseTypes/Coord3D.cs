// -----------------------------------------------------------------------
// <copyright file="Coord3D.cs" company="Sage.Net">
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

namespace Sage.Net.BaseTypes;

/// <summary>A 3D coordinate.</summary>
/// <param name="X">A <see cref="float"/> representing the X coordinate.</param>
/// <param name="Y">A <see cref="float"/> representing the Y coordinate.</param>
/// <param name="Z">A <see cref="float"/> representing the Z coordinate.</param>
public record Coord3D(float X, float Y, float Z)
{
    /// <summary>Gets the zero <see cref="Coord3D"/>.</summary>
    public static Coord3D Zero => new(0, 0, 0);

    /// <summary>Gets the length of the coordinate.</summary>
    public float Length => float.Sqrt((X * X) + (Y * Y) + (Z * Z));

    /// <summary>Gets the squared length of the coordinate.</summary>
    public float LengthSqr => (X * X) + (Y * Y) + (Z * Z);

    /// <summary>Gets the normalized coordinate.</summary>
    public Coord3D Normalized
    {
        get
        {
            var length = Length;
            return float.Abs(length) >= float.Epsilon ? new Coord3D(X / length, Y / length, Z / length) : this;
        }
    }

    /// <summary>Calculates the cross-product of two coordinates.</summary>
    /// <param name="left">The left <see cref="Coord3D"/>.</param>
    /// <param name="right">The right <see cref="Coord3D"/>.</param>
    /// <returns>A new <see cref="Coord3D"/> representing the cross-product.</returns>
    public static Coord3D CrossProduct(Coord3D left, Coord3D right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return new Coord3D(
            (left.Y * right.Z) - (left.Z * right.Y),
            (left.Z * right.X) - (left.X * right.Z),
            (left.X * right.Y) - (left.Y * right.X)
        );
    }

    /// <summary>Adds two coordinates together.</summary>
    /// <param name="other">The other <see cref="Coord3D"/>.</param>
    /// <returns>A new <see cref="Coord3D"/> representing the sum of the two coordinates.</returns>
    public Coord3D Add(Coord3D other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new Coord3D(X + other.X, Y + other.Y, Z + other.Z);
    }

    /// <summary>Subtracts two coordinates from each other.</summary>
    /// <param name="other">The other <see cref="Coord3D"/>.</param>
    /// <returns>A new <see cref="Coord3D"/> representing the difference of the two coordinates.</returns>
    public Coord3D Subtract(Coord3D other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new Coord3D(X - other.X, Y - other.Y, Z - other.Z);
    }

    /// <summary>Scales the coordinate by a given factor.</summary>
    /// <param name="scale">The scale factor.</param>
    /// <returns>A new <see cref="Coord3D"/> representing the scaled coordinate.</returns>
    public Coord3D Scaled(float scale) => new(X * scale, Y * scale, Z * scale);

    /// <summary>Performs an addition operation on two coordinates.</summary>
    /// <param name="left">The left-handed <see cref="Coord3D"/>.</param>
    /// <param name="right">The right-handed <see cref="Coord3D"/>.</param>
    /// <returns>A new <see cref="Coord3D"/> representing the sum of the two coordinates.</returns>
    public static Coord3D operator +(Coord3D left, Coord3D right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Add(right);
    }

    /// <summary>Performs a subtraction operation on two coordinates.</summary>
    /// <param name="left">The left-handed <see cref="Coord3D"/>.</param>
    /// <param name="right">The right-handed <see cref="Coord3D"/>.</param>
    /// <returns>A new <see cref="Coord3D"/> representing the difference of the two coordinates.</returns>
    public static Coord3D operator -(Coord3D left, Coord3D right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Subtract(right);
    }
}
