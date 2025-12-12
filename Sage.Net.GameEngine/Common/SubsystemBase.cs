// -----------------------------------------------------------------------
// <copyright file="SubsystemBase.cs" company="Sage.Net">
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

namespace Sage.Net.GameEngine.Common;

/// <summary>The base class for all subsystems.</summary>
public abstract class SubsystemBase : IDisposable
{
#if DUMP_PERF_STATS && RTS_ZERO_HOUR
    private const float MinTimeThreshold = .0002F;
#endif

    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="SubsystemBase"/> class.</summary>
    /// <remarks>This will add the initialized subsystem to the subsystem list.</remarks>
    protected SubsystemBase() => SubsystemsList.TheSubsystemsList?.AddSubsystem(this);

#if DUMP_PERF_STATS
    /// <summary>Gets or sets the total time spent in an action.</summary>
    public static float TotalTime { get; protected set; }
#endif

    /// <summary>Gets or sets the name of the subsystem.</summary>
    public string Name { get; set; } = string.Empty;

#if DUMP_PERF_STATS
    /// <summary>Gets or sets the time spent updating the subsystem.</summary>
    public float UpdateTime { get; protected set; }

    /// <summary>Gets or sets the time spent drawing the subsystem.</summary>
    public float DrawTime { get; protected set; }

#if RTS_ZERO_HOUR
    /// <summary>Gets or sets a value indicating whether to dump the update time.</summary>
    public bool DoDumpUpdate { get; protected set; }

    /// <summary>Gets or sets a value indicating whether to dump the draw time.</summary>
    public bool DoDumpDraw { get; protected set; }
#endif

    /// <summary>Gets or sets a value indicating the consumed start time for updating.</summary>
    protected float StartUpdateTimeConsumed { get; set; }

    /// <summary>Gets or sets a value indicating the consumed start time for drawing.</summary>
    protected float StartDrawTimeConsumed { get; set; }

    /// <summary>Clears the total time.</summary>
    public static void ClearTotalTime() => TotalTime = 0;
#endif

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Initializes the subsystem.</summary>
    public abstract void Initialize();

    /// <summary>Pre-processes the load of the subsystem.</summary>
    /// <remarks>This is, by default, a no-op.</remarks>
    public virtual void PostProcessLoad() { }

    /// <summary>Resets the subsystem.</summary>
    public abstract void Reset();

    /// <summary>Updates the subsystem.</summary>
    public abstract void UpdateCore();

    /// <summary>Draws the subsystem.</summary>
    /// <remarks>This will, during debug builds, crash if it isn't overriden by the base class.</remarks>
    public virtual void DrawCore() => Debug.Fail($"Shouldn't call {nameof(DrawCore)} on base class.");

#if DUMP_PERF_STATS
    /// <summary>Updates the subsystem.</summary>
    /// <remarks>Performance statistics will be collected during this action. Disable <c>DUMP_PERF_STATS</c> to improve speed.</remarks>
    public void Update()
    {
        var frequency = Stopwatch.Frequency;
        var startTime = Stopwatch.GetTimestamp();
        StartUpdateTimeConsumed = TotalTime;
        UpdateCore();
        var endTime = Stopwatch.GetTimestamp();
        UpdateTime = (endTime - startTime) / (float)frequency;
        var subTime = TotalTime - StartUpdateTimeConsumed;
        if (string.IsNullOrEmpty(Name))
        {
            return;
        }

#if RTS_ZERO_HOUR
        if (UpdateTime > MinTimeThreshold)
        {
            DoDumpUpdate = true;
        }

        if (UpdateTime > MinTimeThreshold / 10F)
#else
        if (UpdateTime > .00001F)
#endif
        {
            UpdateTime -= subTime;
            TotalTime += UpdateTime;
        }
        else
        {
            UpdateTime = 0;
        }
    }

    /// <summary>Draws the subsystem.</summary>
    /// <remarks>Performance statistics will be collected during this action. Disable <c>DUMP_PERF_STATS</c> to improve speed.</remarks>
    public void Draw()
    {
        var frequency = Stopwatch.Frequency;
        var startTime = Stopwatch.GetTimestamp();
        StartDrawTimeConsumed = TotalTime;
        DrawCore();
        var endTime = Stopwatch.GetTimestamp();
        DrawTime = (endTime - startTime) / (float)frequency;
        var subTime = TotalTime - StartDrawTimeConsumed;
        if (string.IsNullOrEmpty(Name))
        {
            return;
        }

#if RTS_ZERO_HOUR
        if (DrawTime > MinTimeThreshold)
        {
            DoDumpDraw = true;
        }

        if (DrawTime > MinTimeThreshold / 10F)
#else
        if (DrawTime > .00001F)
#endif
        {
            DrawTime -= subTime;
            TotalTime += DrawTime;
        }
        else
        {
            DrawTime = 0;
        }
    }
#else
    /// <summary>Updates the subsystem.</summary>
    public void Update() => UpdateCore();

    /// <summary>Draws the subsystem.</summary>
    public void Draw() => DrawCore();
#endif

    /// <summary>Disposes the managed data of the <see cref="SubsystemBase"/> instance.</summary>
    /// <param name="disposing"><see langword="true"/> to dispose managed data; otherwise <see langword="false"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            SubsystemsList.TheSubsystemsList?.RemoveSubsystem(this);
        }

        _disposed = true;
    }
}
