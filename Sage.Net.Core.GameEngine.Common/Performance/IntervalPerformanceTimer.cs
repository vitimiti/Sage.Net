// -----------------------------------------------------------------------
// <copyright file="IntervalPerformanceTimer.cs" company="Sage.Net">
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
/// Measures and reports performance metrics for a specific code region across a range of frames.
/// </summary>
/// <remarks>
/// This timer accumulates high-resolution timing information between calls to <see cref="Start"/> and
/// <see cref="Stop"/>, and reports aggregated statistics (average time per call, per frame, calls per frame,
/// and an estimated maximum FPS) once the configured end frame is reached.
/// <para>
/// The implementation is compiled only when the <c>PERF_TIMERS</c> compilation symbol is defined; otherwise
/// all members are no-ops to avoid runtime overhead in non-performance builds.
/// </para>
/// </remarks>
#if !PERF_TIMERS
[SuppressMessage(
    "Performance",
    "CA1822:Mark members as static",
    Justification = "Required to maintain the API when PERF_TIMERS is not defined."
)]
#endif
public class IntervalPerformanceTimer
{
#if PERF_TIMERS
    private const int MetricsInterval = 150;

    private readonly string _identifier;
    private readonly long _startFrame;
    private readonly long _endFrame;
    private readonly bool _crashWithInfo;

    private long _startTime;
    private long _accumulatedTicks;
    private int _callCount;
    private long _lastFrameCaptured = -1;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="IntervalPerformanceTimer"/> class.
    /// </summary>
    /// <param name="identifier">A descriptive name used to label the reported metrics output.</param>
    /// <param name="startFrame">The first frame index (inclusive) for which timings will be recorded.</param>
    /// <param name="endFrame">
    /// The last frame index (inclusive) for which timings will be recorded; use <c>-1</c> to record until explicitly stopped
    /// without an automatic report.
    /// </param>
    /// <param name="crashWithInfo">
    /// When <see langword="true"/>, emits the final metrics via <see cref="System.Diagnostics.Debug.Fail(string?)"/>,
    /// otherwise writes to the console.
    /// </param>
    /// <remarks>
    /// Timing and reporting are active only when compiled with <c>PERF_TIMERS</c>; otherwise this instance is inert.
    /// </remarks>
    public IntervalPerformanceTimer(
        string identifier,
        long startFrame = 0,
        long endFrame = -1,
        bool crashWithInfo = true
    )
    {
#if PERF_TIMERS
        _identifier = identifier;
        _startFrame = startFrame;
        _endFrame = endFrame;
        _crashWithInfo = crashWithInfo;
#endif
    }

    /// <summary>
    /// Marks the beginning of a timed section for the current call.
    /// </summary>
    /// <remarks>
    /// The call is effective only if the current frame obtained from <see cref="PerformanceSystem.FrameProvider"/>
    /// lies within the configured <c>startFrame</c>/<c>endFrame</c> interval and when compiled with <c>PERF_TIMERS</c>.
    /// In other configurations this method does nothing.
    /// </remarks>
    public void Start()
    {
#if PERF_TIMERS
        var currentFrame = PerformanceSystem.FrameProvider();
        if (currentFrame >= _startFrame && (_endFrame == -1 || currentFrame <= _endFrame))
        {
            _startTime = Stopwatch.GetTimestamp();
        }
#endif
    }

    /// <summary>
    /// Marks the end of a timed section for the current call and accumulates elapsed time.
    /// </summary>
    /// <remarks>
    /// When the configured <c>endFrame</c> has been reached, this method triggers reporting of the collected metrics.
    /// Has effect only when compiled with <c>PERF_TIMERS</c>; otherwise it is a no-op.
    /// </remarks>
    public void Stop()
    {
#if PERF_TIMERS
        var currentFrame = PerformanceSystem.FrameProvider();

        if (currentFrame >= _startFrame && (_endFrame == -1 || currentFrame <= _endFrame))
        {
            var now = Stopwatch.GetTimestamp();
            _accumulatedTicks += now - _startTime;
            _callCount++;
            _lastFrameCaptured = currentFrame;
        }

        if (_endFrame > _startFrame + MetricsInterval)
        {
            // _endFrame = _startFrame + MetricsInterval; // (Simulated logic)
        }

        if (_endFrame > 0 && currentFrame >= _endFrame)
        {
            OutputInfo();
        }
#endif
    }

#if PERF_TIMERS
    private void OutputInfo()
    {
        var totalTimeMs = (double)_accumulatedTicks / Stopwatch.Frequency * 1000.0;
        var framesSpanned = _lastFrameCaptured - _startFrame + 1;
        if (framesSpanned < 1)
        {
            framesSpanned = 1;
        }

        var avgTimePerCall = _callCount > 0 ? totalTimeMs / _callCount : 0;
        var avgTimePerFrame = totalTimeMs / framesSpanned;
        var callsPerFrame = (double)_callCount / framesSpanned;
        var maxFps = avgTimePerFrame > 0 ? 1000.0 / avgTimePerFrame : 0;

        var message = $"""
            [{_identifier}]
            Average Time (per call):  {avgTimePerCall:F4} ms
            Average Time (per frame): {avgTimePerFrame:F4} ms
            Average calls per frame:  {callsPerFrame:F2}
            Number of calls:          {_callCount}
            Max possible FPS:         {maxFps:F4}
            """;

        if (_crashWithInfo)
        {
            Debug.Fail($"CRITICAL PERF INFO:\n{message}");
        }
        else
        {
            Console.WriteLine($"PERF INFO:\n{message}");
        }

        // Reset for next batch if intended to be reused like C++ ShowMetrics often implies
        ResetInternal();
    }
#endif

#if PERF_TIMERS
    private void ResetInternal()
    {
        _accumulatedTicks = 0;
        _callCount = 0;
    }
#endif
}
