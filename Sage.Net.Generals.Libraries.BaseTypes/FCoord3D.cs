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

namespace Sage.Net.Generals.Libraries.BaseTypes;

/// <summary>Represents a three-dimensional coordinate.</summary>
/// <param name="X">The x-coordinate.</param>
/// <param name="Y">The y-coordinate.</param>
/// <param name="Z">The z-coordinate.</param>
public record FCoord3D(float X, float Y, float Z)
{
    /// <summary>Gets the zero vector.</summary>
    public static FCoord3D Zero => new(0F, 0F, 0F);

    /// <summary>Gets the length of the vector.</summary>
    public float Length => float.Sqrt((X * X) + (Y * Y) + (Z * Z));

    /// <summary>Gets the squared length of the vector.</summary>
    public float LengthSqr => (X * X) + (Y * Y) + (Z * Z);

    /// <summary>Gets the normalized vector.</summary>
    public FCoord3D Normalized
    {
        get
        {
            var length = Length;
            return float.Abs(length) >= float.Epsilon ? new FCoord3D(X / length, Y / length, Z / length) : this;
        }
    }

    /// <summary>Scales the vector.</summary>
    /// <param name="scalar">The scalar to scale by.</param>
    /// <returns>The scaled vector.</returns>
    public FCoord3D Scale(float scalar) => Multiply(scalar);

    /// <summary>Adds two vectors together.</summary>
    /// <param name="other">The other vector to add.</param>
    /// <returns>The sum of the two vectors.</returns>
    public FCoord3D Add(FCoord3D other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new FCoord3D(X + other.X, Y + other.Y, Z + other.Z);
    }

    /// <summary>Subtracts one vector from another.</summary>
    /// <param name="other">The other vector to subtract.</param>
    /// <returns>The difference between the two vectors.</returns>
    public FCoord3D Subtract(FCoord3D other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new FCoord3D(X - other.X, Y - other.Y, Z - other.Z);
    }

    /// <summary>Multiplies two vectors together.</summary>
    /// <param name="other">The other vector to multiply.</param>
    /// <returns>The product of the two vectors.</returns>
    public FCoord3D Multiply(FCoord3D other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new FCoord3D(
            (Y * other.Z) - (Z * other.Y),
            (Z * other.X) - (X * other.Z),
            (X * other.Y) - (Y * other.X)
        );
    }

    /// <summary>Multiplies a vector by a scalar.</summary>
    /// <param name="scalar">The scalar to multiply by.</param>
    /// <returns>The product of the vector and the scalar.</returns>
    public FCoord3D Multiply(float scalar) => new(X * scalar, Y * scalar, Z * scalar);

    /// <summary>Calculates the dot product of two vectors.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The dot product of the two vectors.</returns>
    public static FCoord3D CrossProduct(FCoord3D left, FCoord3D right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Multiply(right);
    }

    /// <summary>Adds two three-dimensional coordinates using the + operator.</summary>
    /// <param name="left">The first coordinate.</param>
    /// <param name="right">The second coordinate.</param>
    /// <returns>The sum of the two coordinates.</returns>
    public static FCoord3D operator +(FCoord3D left, FCoord3D right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Add(right);
    }

    /// <summary>Subtracts one three-dimensional coordinate from another using the - operator.</summary>
    /// <param name="left">The first coordinate.</param>
    /// <param name="right">The second coordinate.</param>
    /// <returns>The difference between the two coordinates.</returns>
    public static FCoord3D operator -(FCoord3D left, FCoord3D right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Subtract(right);
    }

    /// <summary>Multiplies two three-dimensional coordinates using the * operator.</summary>
    /// <param name="left">The first coordinate.</param>
    /// <param name="right">The second coordinate.</param>
    /// <returns>The product of the two coordinates.</returns>
    public static FCoord3D operator *(FCoord3D left, FCoord3D right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Multiply(right);
    }

    /// <summary>Multiplies a three-dimensional coordinate by a scalar using the * operator.</summary>
    /// <param name="left">The coordinate to multiply.</param>
    /// <param name="right">The scalar to multiply by.</param>
    /// <returns>The product of the coordinate and the scalar.</returns>
    public static FCoord3D operator *(FCoord3D left, float right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Multiply(right);
    }
}
