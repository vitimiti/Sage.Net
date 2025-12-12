// -----------------------------------------------------------------------
// <copyright file="Coord2D.cs" company="Sage.Net">
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

public record Coord2D(float X, float Y)
{
    public float Length => float.Sqrt((X * X) + (Y * Y));

    public Coord2D Normalized
    {
        get
        {
            var length = Length;
            return float.Abs(length) >= float.Epsilon ? new Coord2D(X / Length, Y / Length) : this;
        }
    }

    public float ToAngle()
    {
#if RTS_ZERO_HOUR
        var length = Length;
        if (float.Abs(length) < float.Epsilon)
        {
            return 0F;
        }

        var c = X / length;
        c = float.Clamp(c, -1F, 1F);

        return y < 0F ? -Trigonometry.ACos(c) : Trigonometry.ACos(c);
#else
        Coord2D vector = new(X, Y);
        var distance = vector.Length;
        if (float.Abs(distance) < float.Epsilon)
        {
            return 0F;
        }

        Coord2D direction = new(1F, 0F);
        var distanceInverted = 1F / distance;
        vector = new Coord2D(vector.X * distanceInverted, vector.Y * distanceInverted);

        var c = (direction.X * vector.X) + (direction.Y * vector.Y);
        c = float.Clamp(c, -1F, 1F);
        var value = Trigonometry.ACos(c);
        var perpendicularZ = (direction.X * vector.Y) - (direction.Y * vector.X);
        if (perpendicularZ < 0F)
        {
            value = -value;
        }

        return value;
#endif
    }
}
