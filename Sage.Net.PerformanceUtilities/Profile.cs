// -----------------------------------------------------------------------
// <copyright file="Profile.cs" company="Sage.Net">
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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#if RTS_PROFILE
using System.Text.Json;
#endif

namespace Sage.Net.PerformanceUtilities;

/// <summary>Lightweight, per-thread sampling/profiling helpers for instrumenting scopes and exporting Chrome Trace events when <c>RTS_PROFILE</c> is defined. Use <see cref="Scope(string)"/> to profile a code region and <see cref="EndFrameAndCollect"/> to aggregate results.</summary>
[SuppressMessage(
    "Naming",
    "CA1724: Type names should not match namespaces",
    Justification = "This is a false positive."
)]
public static class Profile
{
#if RTS_PROFILE
    private const int TracePid = 1;
    private const string TraceCategory = "rts";

    // Cap per thread (count of B/E events). Tune as you like.
    private const int TraceEventsPerThreadCap = 200_000;
#endif
    private static readonly ConcurrentDictionary<int, ThreadState> AllThreads = new();

#if RTS_PROFILE
    private static readonly ThreadLocal<ThreadState> Tls = new(() =>
    {
        ThreadState ts = new()
        {
            ThreadId = Environment.CurrentManagedThreadId,
            ThreadName = Thread.CurrentThread.Name ?? string.Empty,
        };

        _ = AllThreads.TryAdd(ts.ThreadId, ts);
        return ts;
    });
#endif

#if RTS_PROFILE
    private static int _nextMarker = 1;

    /// <summary>Begins a named profiling scope associated with the provided <paramref name="marker"/>. Prefer using <see cref="Scope(string)"/> which manages the lifetime automatically.</summary>
    /// <param name="name">Human-friendly scope name. Nested scopes build a path with '/'.</param>
    /// <param name="marker">A unique marker that must match the corresponding <see cref="End(int)"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Begin(string name, int marker)
    {
        ThreadState ts = Tls.Value!;
        var now = Stopwatch.GetTimestamp();

        TraceBegin(ts, name, now);

        var path = ts.Stack.Count == 0 ? name : ts.Stack.Peek().Path + "/" + name;

        if (!ts.NodesByPath.TryGetValue(path, out FrameNode? node))
        {
            node = new FrameNode { Path = path };
            ts.NodesByPath.Add(path, node);
        }

        node.Marker = marker;
        node.Count++;
        node.StartTicks = now;
        node.ChildTicksAccum = 0;

        ts.Stack.Push(node);
    }
#endif

    /// <summary>Creates a disposable token that profiles the lifetime of the current scope.</summary>
    /// <param name="name">Human-friendly scope name.</param>
    /// <returns>A <see cref="ScopeToken"/> that ends the scope on <see cref="IDisposable.Dispose"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ScopeToken Scope(string name)
    {
#if RTS_PROFILE
        var marker = Interlocked.Increment(ref _nextMarker);
        Begin(name, marker);
        return new ScopeToken(marker);
#else
        return default;
#endif
    }

#if RTS_PROFILE
    /// <summary>Ends the profiling scope that was started with <see cref="Begin(string, int)"/>.</summary>
    /// <param name="marker">The marker obtained when the scope was started.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void End(int marker)
    {
        ThreadState ts = Tls.Value!;
        if (ts.Stack.Count == 0)
        {
            return;
        }

        FrameNode node = ts.Stack.Pop();

        if (node.Marker != marker)
        {
            Debug.Fail($"Profiler scope mismatch. Expected marker {node.Marker}, got {marker} for '{node.Path}'.");
            return;
        }

        var now = Stopwatch.GetTimestamp();
        TraceEnd(ts, now);

        var elapsed = now - node.StartTicks;

        node.InclusiveTicks += elapsed;
        node.ExclusiveTicks += elapsed - node.ChildTicksAccum;

        if (ts.Stack.Count > 0)
        {
            ts.Stack.Peek().ChildTicksAccum += elapsed;
        }
    }

    /// <summary>Sets the current managed thread's name for profiling and trace export purposes.</summary>
    /// <param name="name">The thread name to set.</param>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "We are completely swallowing the error, this is intentional."
    )]
    public static void SetThreadName(string name)
    {
        try
        {
            Thread.CurrentThread.Name = name;
        }
        catch
        {
            // Swallowed on purpose.
        }

        ThreadState ts = Tls.Value!;
        ts.ThreadName = name;
    }

    /// <summary>Writes collected trace events to a Chrome Trace JSON file compatible with chrome://tracing.</summary>
    /// <param name="filePath">Destination file path for the JSON trace.</param>
    public static void DumpChromeTrace(string filePath)
    {
        ThreadState[] threads = [.. AllThreads.Values];

        using FileStream fs = File.Create(filePath);
        using var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = false, SkipValidation = false });

        writer.WriteStartObject();
        writer.WritePropertyName("traceEvents");
        writer.WriteStartArray();

        foreach (ThreadState ts in threads)
        {
            writer.WriteStartObject();
            writer.WriteString("ph", "M");
            writer.WriteString("name", "thread_name");
            writer.WriteNumber("pid", TracePid);
            writer.WriteNumber("tid", ts.ThreadId);
            writer.WritePropertyName("args");
            writer.WriteStartObject();
            writer.WriteString(
                "name",
                string.IsNullOrWhiteSpace(ts.ThreadName) ? $"Thread {ts.ThreadId}" : ts.ThreadName
            );

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        foreach (ThreadState ts in threads)
        {
            foreach (TraceEvent ev in ts.TraceEvents.EnumerateChronological())
            {
                writer.WriteStartObject();
                writer.WriteString("cat", TraceCategory);

                // "E" events don't need a name. "B" events do.
                if (ev.Phase == 'B')
                {
                    writer.WriteString("name", ev.Name);
                }

                writer.WriteString("ph", ev.Phase == 'B' ? "B" : "E");
                writer.WriteNumber("ts", ev.TimestampUs);
                writer.WriteNumber("pid", TracePid);
                writer.WriteNumber("tid", ev.ThreadId);
                writer.WriteEndObject();
            }
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();
    }
#endif

    /// <summary>Aggregates profiling results across all threads for the current frame and resets per-node counters for the next frame.</summary>
    /// <returns>A snapshot containing per-path accumulated ticks and counts.</returns>
    public static FrameSnapshot EndFrameAndCollect()
    {
        FrameSnapshot snap = new();
        var frameStart = Stopwatch.GetTimestamp();
        foreach (ThreadState ts in AllThreads.Values)
        {
            foreach (FrameNode n in ts.NodesByPath.Values)
            {
                if (!snap.ByPath.TryGetValue(n.Path, out (long InclusiveTicks, long ExclusiveTicks, int Count) agg))
                {
                    agg = (0, 0, 0);
                }

                agg.InclusiveTicks += n.InclusiveTicks;
                agg.ExclusiveTicks += n.ExclusiveTicks;
                agg.Count += n.Count;

                snap.ByPath[n.Path] = agg;

                n.InclusiveTicks = 0;
                n.ExclusiveTicks = 0;
                n.Count = 0;
            }
        }

        snap.TotalFrameTicks = Stopwatch.GetTimestamp() - frameStart;
        return snap;
    }

#if RTS_PROFILE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long TicksToMicroseconds(long ticks) => ticks * 1_000_000L / Stopwatch.Frequency;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TraceBegin(ThreadState ts, string name, long timestampTicks) =>
        ts.TraceEvents.Add(new TraceEvent(TicksToMicroseconds(timestampTicks), ts.ThreadId, name, 'B'));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TraceEnd(ThreadState ts, long timestampTicks) =>
        ts.TraceEvents.Add(new TraceEvent(TicksToMicroseconds(timestampTicks), ts.ThreadId, string.Empty, 'E'));
#endif

#if RTS_PROFILE
    private readonly struct TraceEvent(long timestampUs, int threadId, string name, char phase)
    {
        public readonly long TimestampUs = timestampUs;
        public readonly int ThreadId = threadId;
        public readonly string Name = name;
        public readonly char Phase = phase; // 'B' or 'E'
    }

    private sealed class TraceRingBuffer
    {
        private readonly TraceEvent[] _buffer;
        private int _nextWriteIndex;
        private int _count;

        public TraceRingBuffer(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(capacity, 0);
            _buffer = new TraceEvent[capacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in TraceEvent ev)
        {
            _buffer[_nextWriteIndex] = ev;
            _nextWriteIndex++;
            if (_nextWriteIndex == _buffer.Length)
            {
                _nextWriteIndex = 0;
            }

            if (_count < _buffer.Length)
            {
                _count++;
            }
        }

        // Returns events in chronological order (oldest -> newest).
        public IEnumerable<TraceEvent> EnumerateChronological()
        {
            var capacity = _buffer.Length;
            var count = _count;
            if (count == 0)
            {
                yield break;
            }

            var start = _nextWriteIndex - count;
            if (start < 0)
            {
                start += capacity;
            }

            for (var i = 0; i < count; i++)
            {
                var idx = start + i;
                if (idx >= capacity)
                {
                    idx -= capacity;
                }

                yield return _buffer[idx];
            }
        }
    }
#endif

#if !RTS_PROFILE
    [SuppressMessage(
        "Performance",
        "CA1812: Avoid uninstantiated internal classes",
        Justification = "This is instantiated during RTS_PROFILE builds."
    )]
#endif
    private sealed class FrameNode
    {
        public string Path { get; init; } = string.Empty;

#if RTS_PROFILE
        public int Marker { get; set; }
#endif

        public long InclusiveTicks { get; set; }

        public long ExclusiveTicks { get; set; }

        public int Count { get; set; }

#if RTS_PROFILE
        public long StartTicks { get; set; }

        public long ChildTicksAccum { get; set; }
#endif
    }

#if !RTS_PROFILE
    [SuppressMessage(
        "Performance",
        "CA1812: Avoid uninstantiated internal classes",
        Justification = "This is instantiated during RTS_PROFILE builds."
    )]
#endif
    private sealed class ThreadState
    {
        public Stack<FrameNode> Stack { get; } = new();

        public Dictionary<string, FrameNode> NodesByPath { get; } = new(StringComparer.Ordinal);

#if RTS_PROFILE
        public int ThreadId { get; init; }

        public string ThreadName { get; set; } = string.Empty;
#endif

#if RTS_PROFILE
        public TraceRingBuffer TraceEvents { get; } = new(capacity: TraceEventsPerThreadCap);
#endif
    }
}
