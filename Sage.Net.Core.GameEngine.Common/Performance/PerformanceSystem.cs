// -----------------------------------------------------------------------
// <copyright file="PerformanceSystem.cs" company="Sage.Net">
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
#if PERF_TIMERS
using System.Collections.Concurrent;
#endif

namespace Sage.Net.Core.GameEngine.Common.Performance;

/// <summary>
/// Central coordinator for performance metrics collection.
/// When the <c>PERF_TIMERS</c> compilation symbol is defined, this system tracks
/// hierarchical timing information using <see cref="PerformanceMetric"/> and
/// <see cref="PerformanceScope"/>. It can also export aggregated metrics to CSV.
/// </summary>
public static class PerformanceSystem
{
#if PERF_TIMERS
    private static readonly ConcurrentBag<PerformanceMetric> _metrics = new();
    private static readonly long _overheadTicks;

    [ThreadStatic]
    private static Stack<PerformanceMetric>? _activeStack;
#endif

    /// <summary>
    /// Gets or sets a callback that provides the current frame index. Used for reporting.
    /// Defaults to a provider that returns <c>0</c>.
    /// </summary>
    public static Func<long> FrameProvider { get; set; } = () => 0;

    /// <summary>
    /// Gets or sets a value indicating whether runtime metric collection is enabled.
    /// When <c>false</c>, timing scopes become no-ops at runtime even if <c>PERF_TIMERS</c> is defined.
    /// </summary>
    public static bool EnableMetrics { get; set; } = true;

    static PerformanceSystem()
    {
#if PERF_TIMERS
        long start = Stopwatch.GetTimestamp();
        for (int i = 0; i < 100_000; i++)
        {
            _ = Stopwatch.GetTimestamp();
        }

        long end = Stopwatch.GetTimestamp();
        _overheadTicks = (end - start) / 100_000;
        if (_overheadTicks < 0)
        {
            _overheadTicks = 0;
        }
#endif
    }

#if PERF_TIMERS
    /// <summary>
    /// Resets all registered metrics to their initial state, clearing accumulated times and counts.
    /// </summary>
    public static void ResetAll()
    {
        foreach (PerformanceMetric metric in _metrics)
        {
            metric.Reset();
        }
    }
#endif

    /// <summary>
    /// Appends the current metrics to a CSV file. If the file is empty or does not exist,
    /// a header row is written first. Metrics are ordered by name and exported as Gross, Net and Count columns.
    /// When performance timers are disabled, this method writes a diagnostic message to the debug output.
    /// </summary>
    /// <param name="filePath">The destination CSV file path.</param>
    /// <param name="frameIndex">The frame index associated with this snapshot.</param>
    public static void DumpMetricsToCsv(string filePath, long frameIndex)
    {
#if PERF_TIMERS || DUMP_PERF_STATS
        var fileExists = File.Exists(filePath);

        using var writer = new StreamWriter(filePath, append: true);
        if (!fileExists || new FileInfo(filePath).Length == 0)
        {
            // Header
            var names = _metrics.OrderBy(m => m.Name).Select(m => m.Name).ToArray();
            writer.Write("Frame");
            foreach (var name in names)
            {
                writer.Write($",Gross:{name}");
            }

            foreach (var name in names)
            {
                writer.Write($",Net:{name}");
            }

            foreach (var name in names)
            {
                writer.Write($",Count:{name}");
            }

            writer.WriteLine();
        }

        writer.Write($"Frame{frameIndex:D8}");

        var sortedMetrics = _metrics.OrderBy(m => m.Name).ToList();

        // Gross
        foreach (PerformanceMetric m in sortedMetrics)
        {
            writer.Write($",{m.GrossTimeMs:F4}");
        }

        // Net
        foreach (PerformanceMetric m in sortedMetrics)
        {
            writer.Write($",{m.NetTimeMs:F4}");
        }

        // Count
        foreach (PerformanceMetric m in sortedMetrics)
        {
            writer.Write($",{m.CallCount}");
        }

        writer.WriteLine();
#else
        Debug.WriteLine("Performance metrics are disabled. Set PERF_TIMERS to enable.");
#endif
    }

#if PERF_TIMERS
    /// <summary>
    /// Registers a metric with the system so it can be aggregated and exported.
    /// Intended to be called by <see cref="PerformanceMetric"/> upon construction.
    /// </summary>
    internal static void Register(PerformanceMetric metric) => _metrics.Add(metric);

    /// <summary>
    /// Starts measuring the supplied metric and returns a scope that will finalize timing on dispose.
    /// Also maintains a per-thread stack to compute exclusive (net) time by subtracting child durations.
    /// </summary>
    /// <param name="metric">The metric to start measuring.</param>
    /// <returns>A <see cref="PerformanceScope"/> representing the active timing scope.</returns>
    internal static PerformanceScope StartMetric(PerformanceMetric metric)
    {
        if (!EnableMetrics)
        {
            return new PerformanceScope(null!); // No-op scope
        }

        Stack<PerformanceMetric> stack = _activeStack ??= new Stack<PerformanceMetric>();
        stack.Push(metric);

        return new PerformanceScope(metric);
    }

    /// <summary>
    /// Stops timing for the specified metric that was previously started, records the elapsed
    /// inclusive time, and adjusts the parent metric's exclusive time to exclude this child duration.
    /// </summary>
    /// <param name="metric">The metric being stopped.</param>
    /// <param name="startTime">The timestamp captured when the metric was started.</param>
    internal static void StopMetric(PerformanceMetric metric, long startTime)
    {
        var endTime = Stopwatch.GetTimestamp();
        var duration = endTime - startTime;

        Stack<PerformanceMetric>? stack = _activeStack;

        // Sanity check: The stopping metric should be the top of the stack
        if (stack == null || stack.Count == 0 || stack.Peek() != metric)
        {
            // In a robust engine, we might log an error here.
            // For now, we just recover gracefully.
            return;
        }

        _ = stack.Pop();

        // Register time to self
        // Net time starts as Gross time, and children subtract from it later
        metric.AddSample(duration, 0);

        // Subtract this duration (plus overhead) from the parent's NET time
        if (stack.Count == 0)
        {
            return;
        }

        PerformanceMetric parent = stack.Peek();
        parent.AdjustNetTime(duration + _overheadTicks);
    }
#endif
}
