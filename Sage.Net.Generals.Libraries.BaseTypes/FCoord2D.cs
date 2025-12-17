// -----------------------------------------------------------------------
// <copyright file="FCoord2D.cs" company="Sage.Net">
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

/// <summary>Represents a two-dimensional coordinate.</summary>
/// <param name="X">The x-coordinate.</param>
/// <param name="Y">The y-coordinate.</param>
public record FCoord2D(float X, float Y)
{
    /// <summary>Gets the length of the vector.</summary>
    public float Length => float.Sqrt((X * X) + (Y * Y));

    /// <summary>Gets the normalized vector.</summary>
    public FCoord2D Normalized
    {
        get
        {
            var len = Length;
            return float.Abs(len) >= float.Epsilon ? new FCoord2D(X / len, Y / len) : this;
        }
    }

    /// <summary>Converts the vector to an angle.</summary>
    /// <returns>The angle in radians.</returns>
    public float ToAngle()
    {
        FCoord2D vector = new(X, Y);
        var distance = vector.Length;
        if (float.Abs(distance) < float.Epsilon)
        {
            return 0F;
        }

        FCoord2D direction = new(1F, 0F);
        var inverseDistance = 1F / distance;
        vector = vector with { X = vector.X * inverseDistance, Y = vector.Y * inverseDistance };

        var dot = (direction.X * vector.X) + (direction.Y * vector.Y);
        dot = float.Clamp(dot, -1F, 1F);

        var value = float.Acos(dot);
        var perpendicularZ = (direction.X * vector.Y) - (direction.Y * vector.X);
        if (perpendicularZ < 0F)
        {
            value = -value;
        }

        return value;
    }
}
