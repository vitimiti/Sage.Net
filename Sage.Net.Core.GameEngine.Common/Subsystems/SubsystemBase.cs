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

namespace Sage.Net.Core.GameEngine.Common.Subsystems;

/// <summary>
/// Base class for subsystems.
/// </summary>
public abstract class SubsystemBase
{
#if DUMP_PERF_STATS
    private static TimeSpan MinTimeThreshold => TimeSpan.FromMilliseconds(.2);
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="SubsystemBase"/> class.
    /// </summary>
    protected SubsystemBase() => GameEngineSystem.TheSubsystemList?.AddSubsystem(this);

#if DUMP_PERF_STATS
    /// <summary>
    /// Gets the total time.
    /// </summary>
    public static TimeSpan TotalTime => TimeConsumed;
#endif

    /// <summary>
    /// Gets or sets the name of the subsystem.
    /// </summary>
    public string? Name { get; set; }

#if DUMP_PERF_STATS
    /// <summary>
    /// Gets the update time.
    /// </summary>
    public TimeSpan UpdateTime => CurrentUpdateTime;

    /// <summary>
    /// Gets the draw time.
    /// </summary>
    public TimeSpan DrawTime => CurrentDrawTime;

    /// <summary>
    /// Gets a value indicating whether to dump the update process.
    /// </summary>
    public bool DoDumpUpdate => DumpUpdate;

    /// <summary>
    /// Gets a value indicating whether to dump the draw process.
    /// </summary>
    public bool DoDumpDraw => DumpDraw;

    /// <summary>
    /// Gets or sets the time consumed.
    /// </summary>
    protected static TimeSpan TimeConsumed { get; set; }

    /// <summary>
    /// Gets or sets the start time consumed.
    /// </summary>
    protected TimeSpan StartTimeConsumed { get; set; }

    /// <summary>
    /// Gets or sets the current update time.
    /// </summary>
    protected TimeSpan CurrentUpdateTime { get; set; }

    /// <summary>
    /// Gets or sets the start draw time consumed.
    /// </summary>
    protected TimeSpan StartDrawTimeConsumed { get; set; }

    /// <summary>
    /// Gets or sets the current draw time.
    /// </summary>
    protected TimeSpan CurrentDrawTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to dump update.
    /// </summary>
    protected bool DumpUpdate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to dump draw.
    /// </summary>
    protected bool DumpDraw { get; set; }
#endif

#if DUMP_PERF_STATS
    /// <summary>
    /// Clears the total time.
    /// </summary>
    public static void ClearTotalTime() => TimeConsumed = TimeSpan.Zero;
#endif

    /// <summary>
    /// Initializes the subsystem.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Post-process load.
    /// </summary>
    public virtual void PostProcessLoad() { }

    /// <summary>
    /// Reset the subsystem.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Update the subsystem.
    /// </summary>
    /// <remarks>This is the actual update code.</remarks>
    /// <seealso cref="Update"/>
    public abstract void UpdateBase();

    /// <summary>
    /// Draw the subsystem.
    /// </summary>
    /// <remarks>This is the actual draw code.</remarks>
    /// <seealso cref="Draw"/>
    public virtual void DrawBase() => Debug.Fail("Shouldn't call base class.");

    /// <summary>
    /// Update the subsystem.
    /// </summary>
    /// <remarks>This depends on the implementation of <see cref="UpdateBase"/>.</remarks>
    /// <seealso cref="UpdateBase"/>
    /// <note>This is a wrapper for <see cref="UpdateBase"/> unless <c>DUMP_PERF_STATS</c> has been defined, then it will retrieve performance stats around the call to <see cref="UpdateBase"/>.</note>
    public void Update()
    {
#if DUMP_PERF_STATS
        var frequency = Stopwatch.Frequency;
        var start = Stopwatch.GetTimestamp();
        StartTimeConsumed = TimeConsumed;
        UpdateBase();
        var end = Stopwatch.GetTimestamp();
        CurrentUpdateTime = new TimeSpan(end - start) / frequency;
        TimeSpan subTime = TimeConsumed - StartTimeConsumed;
        if (string.IsNullOrEmpty(Name))
        {
            return;
        }

        if (CurrentUpdateTime > MinTimeThreshold)
        {
            DumpUpdate = true;
        }

        if (CurrentUpdateTime > MinTimeThreshold / 10)
        {
            CurrentUpdateTime -= subTime;
            TimeConsumed += CurrentUpdateTime;
        }
        else
        {
            CurrentUpdateTime = TimeSpan.Zero;
        }
#else
        UpdateBase();
#endif
    }

    /// <summary>
    /// Draw the subsystem.
    /// </summary>
    /// <remarks>This depends on the implementation of <see cref="DrawBase"/>.</remarks>
    /// <seealso cref="DrawBase"/>
    /// <note>This is a wrapper for <see cref="DrawBase"/> unless <c>DUMP_PERF_STATS</c> has been defined, then it will retrieve performance stats around the call to <see cref="DrawBase"/>.</note>
    public void Draw()
    {
#if DUMP_PERF_STATS
        var frequency = Stopwatch.Frequency;
        var start = Stopwatch.GetTimestamp();
        StartDrawTimeConsumed = TimeConsumed;
        DrawBase();
        var end = Stopwatch.GetTimestamp();
        CurrentDrawTime = new TimeSpan(end - start) / frequency;
        TimeSpan subTime = TimeConsumed - StartDrawTimeConsumed;
        if (string.IsNullOrEmpty(Name))
        {
            return;
        }

        if (CurrentDrawTime > MinTimeThreshold)
        {
            DumpDraw = true;
        }

        if (CurrentDrawTime > MinTimeThreshold / 10)
        {
            CurrentDrawTime -= subTime;
            TimeConsumed += CurrentDrawTime;
        }
        else
        {
            CurrentDrawTime = TimeSpan.Zero;
        }
#else
        DrawBase();
#endif
    }
}
