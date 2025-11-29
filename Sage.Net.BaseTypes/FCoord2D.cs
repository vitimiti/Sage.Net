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

namespace Sage.Net.BaseTypes;

/// <summary>
/// A 2D coordinate with <see cref="float"/> precision.
/// </summary>
/// <param name="X">The X coordinate.</param>
/// <param name="Y">The Y coordinate.</param>
public record FCoord2D(float X, float Y)
{
    /// <summary>
    /// Gets the length of the <see cref="FCoord2D"/>.
    /// </summary>
    public float Length => float.Sqrt(float.Pow(X, 2) + float.Pow(Y, 2));

    /// <summary>
    /// Returns a normalized <see cref="FCoord2D"/>.
    /// </summary>
    /// <returns>A new, normalized <see cref="FCoord2D"/>, or the zero coordinate.</returns>
    public FCoord2D Normalized()
    {
        var length = Length;
        return float.Abs(Length) >= float.Epsilon ? new FCoord2D(X / length, Y / length) : new FCoord2D(0, 0);
    }

    /// <summary>
    /// Converts the <see cref="FCoord2D"/> to an angle in radians.
    /// </summary>
    /// <returns>A <see cref="float"/> with the value of the <see cref="FCoord2D"/> angle in radians.</returns>
    public float ToAngle()
    {
        var length = Length;
        if (float.Abs(length) < float.Epsilon)
        {
            return 0;
        }

        var c = float.Clamp(X / length, -1F, 1F);
        return Y < 0F ? -float.Acos(c) : float.Acos(c);
    }
}
