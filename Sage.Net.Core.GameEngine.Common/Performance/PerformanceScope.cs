// -----------------------------------------------------------------------
// <copyright file="PerformanceScope.cs" company="Sage.Net">
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

#if PERF_TIMERS
using System.Diagnostics;
#endif

namespace Sage.Net.Core.GameEngine.Common.Performance;

/// <summary>
/// A stack-only, disposable scope used to measure execution time for a
/// <see cref="PerformanceMetric"/>.
/// When <c>PERF_TIMERS</c> is not defined, this scope is a no-op.
/// </summary>
/// <remarks>
/// Typical usage:
/// <code>
/// var metric = new PerformanceMetric("Update");
/// using (metric.Measure())
/// {
///     // Code to measure
/// }
/// </code>
/// </remarks>
public readonly ref struct PerformanceScope : IDisposable
{
#if PERF_TIMERS
    /// <summary>
    /// The metric being measured by this scope.
    /// </summary>
    private readonly PerformanceMetric _metric;

    /// <summary>
    /// The start timestamp captured via <see cref="System.Diagnostics.Stopwatch.GetTimestamp"/>.
    /// </summary>
    private readonly long _startTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceScope"/> struct and
    /// captures the start time for the provided <paramref name="metric"/>.
    /// </summary>
    /// <param name="metric">The performance metric to measure.</param>
    internal PerformanceScope(PerformanceMetric metric)
    {
        _metric = metric;
        _startTime = Stopwatch.GetTimestamp();
    }
#endif

    /// <summary>
    /// Completes the measurement and records the elapsed time to the associated metric.
    /// When <c>PERF_TIMERS</c> is not defined, this method does nothing.
    /// </summary>
    public void Dispose()
    {
#if PERF_TIMERS
        if (_metric is not null)
        {
            PerformanceSystem.StopMetric(_metric, _startTime);
        }
#endif
    }
}
