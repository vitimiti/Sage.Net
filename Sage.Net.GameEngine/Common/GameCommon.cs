// -----------------------------------------------------------------------
// <copyright file="GameCommon.cs" company="Sage.Net">
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

using Sage.Net.WwVegas.WwLib;

namespace Sage.Net.GameEngine.Common;

/// <summary>Common game constants and helper methods used across the engine.</summary>
public static class GameCommon
{
    /// <summary>Base target frames per second for rendering.</summary>
    public const int BaseFps = 30;

    /// <summary>Number of simulation logic updates per second.</summary>
    public const int LogicFramesPerSecond = WwCommon.SyncPerSecond;

    /// <summary>Number of milliseconds in one second.</summary>
    public const int MillisecondsPerSecond = 1000;

    /// <summary>Gets the number of logic frames that occur in one millisecond.</summary>
    public static float LogicFramesPerMillisecond => LogicFramesPerSecond / (float)MillisecondsPerSecond;

    /// <summary>Gets the number of milliseconds that elapse during a single logic frame.</summary>
    public static float MillisecondsPerLogicFrame => MillisecondsPerSecond / (float)LogicFramesPerSecond;

    /// <summary>Gets the number of seconds that elapse during a single logic frame.</summary>
    public static float SecondsPerLogicFrame => 1F / LogicFramesPerSecond;

    /// <summary>Converts an angular velocity specified in degrees per second to radians per logic frame.</summary>
    /// <param name="degreesPerSecond">Angular speed in degrees per second.</param>
    /// <returns>Angular displacement in radians for one logic frame.</returns>
    public static float ConvertAngularVelocityInDegreesPerSecondToRadiansPerFrame(float degreesPerSecond)
    {
        const float radsPerDegree = (float)Math.PI / 180F;
        return degreesPerSecond * (SecondsPerLogicFrame * radsPerDegree);
    }
}
