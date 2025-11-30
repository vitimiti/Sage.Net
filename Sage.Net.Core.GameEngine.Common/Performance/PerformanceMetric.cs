// -----------------------------------------------------------------------
// <copyright file="PerformanceMetric.cs" company="Sage.Net">
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

using System.Diagnostics.CodeAnalysis;
#if PERF_TIMERS
using System.Diagnostics;
#endif

namespace Sage.Net.Core.GameEngine.Common.Performance;

/// <summary>
/// Represents a named performance metric that can be measured over time.
/// When <c>PERF_TIMERS</c> is defined, the metric accumulates inclusive (gross)
/// and exclusive (net) execution times and tracks the number of calls.
/// Use <see cref="Measure"/> to create a <see cref="PerformanceScope"/> that records a sample.
/// </summary>
#if !PERF_TIMERS
[SuppressMessage(
    "Performance",
    "CA1822:Mark members as static",
    Justification = "Required to maintain the API when PERF_TIMERS is not defined."
)]
[SuppressMessage(
    "csharpsquid",
    "S2325: Methods and properties that don't access instance data should be static",
    Justification = "Required to maintain the API when PERF_TIMERS is not defined."
)]
#endif
public class PerformanceMetric
{
#if PERF_TIMERS
    private long _grossTicks;
    private long _netTicks;
    private int _callCount;
#endif

    /// <summary>
    /// Gets the display name of this performance metric.
    /// </summary>
    public string Name { get; }

#if PERF_TIMERS
    /// <summary>
    /// Gets the accumulated inclusive time, in milliseconds, across all recorded samples.
    /// Inclusive time includes the time spent in child metrics.
    /// </summary>
    public double GrossTimeMs => (double)_grossTicks / Stopwatch.Frequency * 1000.0;

    /// <summary>
    /// Gets the accumulated exclusive time, in milliseconds, across all recorded samples.
    /// Exclusive time excludes time attributed to child metrics.
    /// </summary>
    public double NetTimeMs => (double)_netTicks / Stopwatch.Frequency * 1000.0;

    /// <summary>
    /// Gets the number of times this metric has been measured.
    /// </summary>
    public int CallCount => _callCount;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMetric"/> class.
    /// When <c>PERF_TIMERS</c> is defined, the metric is registered with the
    /// <see cref="PerformanceSystem"/> so it can be aggregated and reported.
    /// </summary>
    /// <param name="name">The display name of the metric.</param>
    public PerformanceMetric(string name)
    {
        Name = name;
#if PERF_TIMERS
        PerformanceSystem.Register(this);
#endif
    }

    /// <summary>
    /// Begins measuring this metric and returns a scope that records the elapsed time
    /// when disposed. Typical usage: <c>using (metric.Measure()) { /* work */ }</c>.
    /// When <c>PERF_TIMERS</c> is not defined, this returns a no-op scope.
    /// </summary>
    /// <returns>A <see cref="PerformanceScope"/> that records a timing sample for this metric.</returns>
    public PerformanceScope Measure() =>
#if PERF_TIMERS
        return PerfSystem.StartMetric(this);
#else
        new();
#endif

#if PERF_TIMERS
    /// <summary>
    /// Records a completed timing sample for this metric.
    /// </summary>
    /// <param name="grossTicks">The inclusive elapsed ticks for the sample.</param>
    /// <param name="netAdjustment">The total ticks attributed to child metrics to subtract from net time.</param>
    internal void AddSample(long grossTicks, long netAdjustment)
    {
        _ = Interlocked.Add(ref _grossTicks, grossTicks);
        _ = Interlocked.Add(ref _netTicks, grossTicks - netAdjustment);
        _ = Interlocked.Increment(ref _callCount);
    }

    /// <summary>
    /// Adjusts the net time by subtracting the specified child duration.
    /// </summary>
    /// <param name="childDuration">The number of ticks to subtract from the net total.</param>
    internal void AdjustNetTime(long childDuration) => Interlocked.Add(ref _netTicks, -childDuration);

    /// <summary>
    /// Resets all accumulated timing data and call count for this metric.
    /// </summary>
    public void Reset()
    {
        _ = Interlocked.Exchange(ref _grossTicks, 0);
        _ = Interlocked.Exchange(ref _netTicks, 0);
        _ = Interlocked.Exchange(ref _callCount, 0);
    }
#endif
}
