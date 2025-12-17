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

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Represents a base class for all subsystems.</summary>
public abstract class SubsystemBase : IDisposable
{
    private bool _disposed;

#if DUMP_PERF_STATS
    /// <summary>Gets the total time consumed by this subsystem.</summary>
    public static float TotalTime => MsConsumed;

    /// <summary>Gets the total time consumed by this subsystem.</summary>
    public float UpdateTime => CurrentUpdateTime;

    /// <summary>Gets the total time consumed by this subsystem.</summary>
    public float DrawTime => CurrentDrawTime;
#endif

    /// <summary>Gets or sets the name of the subsystem.</summary>
    public string Name { get; set; } = string.Empty;

#if DUMP_PERF_STATS
    /// <summary>Gets or sets the total time consumed by this subsystem.</summary>
    protected static float MsConsumed { get; set; }

    /// <summary>Gets or sets the time consumed by this subsystem since the last update.</summary>
    protected float StartTimeConsumed { get; set; }

    /// <summary>Gets or sets the current update time.</summary>
    protected float CurrentUpdateTime { get; set; }

    /// <summary>Gets or sets the time consumed by this subsystem since the last draw.</summary>
    protected float StartDrawTimeConsumed { get; set; }

    /// <summary>Gets or sets the current draw time.</summary>
    protected float CurrentDrawTime { get; set; }
#endif

    /// <summary>Initializes a new instance of the <see cref="SubsystemBase"/> class.</summary>
    /// <remarks>Adds the subsystem to the subsystem list.</remarks>
    protected SubsystemBase() => SubsystemList.TheSubsystemList?.AddSubsystem(this);

#if DUMP_PERF_STATS
    /// <summary>Clears the total time.</summary>
    public static void ClearTotalTime() => MsConsumed = 0;
#endif

    /// <summary>Initializes the subsystem.</summary>
    public abstract void Initialize();

    /// <summary>Prepares the subsystem for loading.</summary>
    public virtual void PostProcessLoad() { }

    /// <summary>Resets the subsystem.</summary>
    public abstract void Reset();

    /// <summary>Performs the actual subsystem update.</summary>
    public abstract void UpdateCore();

    /// <summary>Performs the actual subsystem drawing.</summary>
    public virtual void DrawCore() =>
        Debug.Fail($"Shouldn't be calling {nameof(SubsystemBase)}.{nameof(DrawCore)} directly.");

#if DUMP_PERF_STATS
    /// <summary>Updates the subsystem.</summary>
    public void Update()
    {
        var frequency = Stopwatch.Frequency;
        var startTime = Stopwatch.GetTimestamp();
        StartTimeConsumed = MsConsumed;
        UpdateCore();
        var endTime = Stopwatch.GetTimestamp();
        CurrentUpdateTime = (endTime - startTime) / (float)frequency;
        var subTime = MsConsumed - StartTimeConsumed;
        if (string.IsNullOrEmpty(Name))
        {
            return;
        }

        if (CurrentUpdateTime > .00001F)
        {
            CurrentUpdateTime -= subTime;
            MsConsumed += CurrentUpdateTime;
        }
        else
        {
            CurrentUpdateTime = 0;
        }
    }

    /// <summary>Draws the subsystem.</summary>
    public void Draw()
    {
        var frequency = Stopwatch.Frequency;
        var startTime = Stopwatch.GetTimestamp();
        DrawCore();
        var endTime = Stopwatch.GetTimestamp();
        CurrentDrawTime = (endTime - startTime) / (float)frequency;
        var subTime = MsConsumed - StartDrawTimeConsumed;
        if (string.IsNullOrEmpty(Name))
        {
            return;
        }

        if (CurrentDrawTime > .00001F)
        {
            CurrentDrawTime -= subTime;
            MsConsumed += CurrentDrawTime;
        }
        else
        {
            CurrentDrawTime = 0;
        }
    }
#else
    /// <summary>Updates the subsystem.</summary>
    public void Update() => UpdateCore();

    /// <summary>Draws the subsystem.</summary>
    public void Draw() => DrawCore();
#endif

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Disposes the subsystem.</summary>
    /// <param name="disposing">Indicates whether the method is called from <see cref="Dispose"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            SubsystemList.TheSubsystemList?.RemoveSubsystem(this);
        }

        _disposed = true;
    }
}
