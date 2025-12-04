// -----------------------------------------------------------------------
// <copyright file="Subsystem.cs" company="Sage.Net">
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
using Sage.Net.Core.GameEngine.Abstractions;

namespace Sage.Net.GeneralsMd.GameEngine.Common;

/// <summary>
/// The base class for all subsystems.
/// </summary>
public abstract class Subsystem : ISubsystem, IDisposable
{
#if DUMP_PERF_STATS
    private const float MinTimeThreshold = .0002F;

    private float _startTimeConsumed;
    private float _startDrawTimeConsumed;
#endif
    private bool _disposed;

    /// <summary>
    /// Gets or sets the name of the subsystem.
    /// </summary>
    public string? Name { get; set; }

#if DUMP_PERF_STATS
    /// <summary>
    /// Gets the total time consumed by the subsystem.
    /// </summary>
    protected static float TotalTime { get; private set; }

    /// <summary>
    /// Gets the time consumed by the subsystem during the last update.
    /// </summary>
    protected float UpdateTime { get; private set; }

    /// <summary>
    /// Gets the time consumed by the subsystem during the last draw.
    /// </summary>
    protected float DrawTime { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to dump the update.
    /// </summary>
    protected bool DoDumpUpdate { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to dump the draw.
    /// </summary>
    protected bool DoDumpDraw { get; private set; }
#endif

    /// <summary>
    /// Initialize the subsystem.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Called after the subsystem has been loaded.
    /// </summary>
    public virtual void PostProcessLoad() { }

    /// <summary>
    /// Reset the subsystem.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Update the subsystem.
    /// </summary>
    public void Update()
    {
#if DUMP_PERF_STATS
        var frequency = Stopwatch.Frequency;
        var startTime = Stopwatch.GetTimestamp();
        _startTimeConsumed = UpdateTime;
        UpdateCore();
        var endTime = Stopwatch.GetTimestamp();
        UpdateTime = (endTime - startTime) / (float)frequency;
        var subTime = UpdateTime - _startTimeConsumed;
        if (string.IsNullOrEmpty(Name))
        {
            return;
        }

        if (UpdateTime > MinTimeThreshold)
        {
            DoDumpUpdate = true;
        }

        if (UpdateTime > MinTimeThreshold / 10F)
        {
            UpdateTime -= subTime;
            TotalTime += UpdateTime;
        }
        else
        {
            UpdateTime = 0;
        }
#else
        UpdateCore();
#endif
    }

    /// <summary>
    /// Draw the subsystem.
    /// </summary>
    public void Draw()
    {
#if DUMP_PERF_STATS
        var frequency = Stopwatch.Frequency;
        var startTime = Stopwatch.GetTimestamp();
        _startDrawTimeConsumed = DrawTime;
        DrawCore();
        var endTime = Stopwatch.GetTimestamp();
        DrawTime = (endTime - startTime) / (float)frequency;
        var subTime = DrawTime - _startDrawTimeConsumed;
        if (string.IsNullOrEmpty(Name))
        {
            return;
        }

        if (DrawTime > MinTimeThreshold)
        {
            DoDumpDraw = true;
        }

        if (DrawTime > MinTimeThreshold / 10F)
        {
            DrawTime -= subTime;
            TotalTime += DrawTime;
        }
        else
        {
            DrawTime = 0;
        }
#else
        DrawCore();
#endif
    }

    /// <summary>
    /// The core update method.
    /// </summary>
    public abstract void UpdateCore();

    /// <summary>
    /// The core draw method.
    /// </summary>
    public virtual void DrawCore() => Debug.Fail($"{nameof(DrawCore)} should not be called from the base class.");

    /// <summary>
    /// Dispose of the <see cref="Subsystem"/> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

#if DUMP_PERF_STATS
    /// <summary>
    /// Clears the total time.
    /// </summary>
    protected static void ClearTotalTime() => TotalTime = 0;
#endif

    /// <summary>
    /// Dispose of the <see cref="Subsystem"/> class.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to dispose managed resources, <see langword="otherwise"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // TODO release managed resources here
        }

        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
