// -----------------------------------------------------------------------
// <copyright file="GameTime.cs" company="Sage.Net">
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

namespace Sage.Net.Abstractions;

/// <summary>
/// Represents a time-tracking mechanism for game applications.
/// </summary>
/// <remarks>
/// Provides functionality to measure the elapsed time and manage updates
/// for time-based operations during a game loop.
/// </remarks>
public class GameTime
{
    private readonly Stopwatch _stopwatch = new();
    private long _lastTicks;

    /// <summary>
    /// Gets the elapsed time since the last frame in seconds.
    /// </summary>
    /// <remarks>
    /// Use this for frame-independent movement: position += velocity * deltaTime.
    /// </remarks>
    public float DeltaTime { get; private set; }

    /// <summary>
    /// Gets the total time elapsed since the game started in seconds.
    /// </summary>
    public double TotalTime { get; private set; }

    /// <summary>
    /// Gets or sets the time scale. 1.0 is normal speed, 0.5 is half speed, 0.0 is paused.
    /// </summary>
    public float TimeScale { get; set; } = 1F;

    /// <summary>
    /// Gets a value indicating whether the game simulation is currently paused.
    /// </summary>
    public bool IsPaused => TimeScale <= 0F;

    /// <summary>
    /// Starts the game clock.
    /// </summary>
    public void Start()
    {
        _stopwatch.Start();
        _lastTicks = _stopwatch.ElapsedTicks;
    }

    /// <summary>
    /// Updates the timing values. This should be called exactly once at the start of every frame.
    /// </summary>
    public void Update()
    {
        var currentTicks = _stopwatch.ElapsedTicks;
        var elapsedTicks = currentTicks - _lastTicks;
        _lastTicks = currentTicks;

        // Convert ticks to raw seconds (unscaled)
        var rawDelta = (double)elapsedTicks / Stopwatch.Frequency;

        // Clamp delta time to avoid huge "jumps" after a hang or debugging breakpoint
        // 0.1s (10 FPS) is a common threshold for physics stability
        var clampedDelta = float.Min((float)rawDelta, .1F);

        // Apply scale
        DeltaTime = clampedDelta * TimeScale;
        TotalTime += DeltaTime;
    }

    /// <summary>
    /// Resets the clock to zero.
    /// </summary>
    public void Reset()
    {
        _stopwatch.Reset();
        _lastTicks = 0;
        TotalTime = 0;
        DeltaTime = 0;
    }
}
