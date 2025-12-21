// -----------------------------------------------------------------------
// <copyright file="Profiler.cs" company="Sage.Net">
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
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sage.Net.Abstractions;

namespace Sage.Net.Diagnostics;

/// <summary>
/// Provides profiling functionality to track execution times and log long-running operations in applications.
/// </summary>
/// <remarks>
/// The Profiler is designed to integrate with the application logging pipeline and measure method-level
/// execution times. It uses `ILogger` for logging and can be configured using `ProfilerOptions`. Optionally,
/// the profiler can leverage a `GameTime` instance to track game-specific timing information.
/// </remarks>
public partial class Profiler
{
    private readonly ILogger<Profiler> _logger;
    private readonly ProfilerOptions _options;
    private readonly GameTime? _gameTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="Profiler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used to log profiling results.</param>
    /// <param name="options">The options used to configure the profiler.</param>
    /// <param name="gameTime">The game time instance used to track game-specific timing information.</param>
    /// <remarks>
    /// The profiler can be configured using the <see cref="ProfilerOptions"/> class.
    /// </remarks>
    public Profiler(ILogger<Profiler> logger, IOptions<ProfilerOptions> options, GameTime? gameTime = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _options = options.Value;
        _gameTime = gameTime;
    }

    /// <summary>
    /// Begins a profiling scope for tracking execution time and logging long-running operations.
    /// </summary>
    /// <param name="methodName">
    /// The name of the method initiating the profiling scope. Automatically set to the calling method's name if not provided.
    /// </param>
    /// <param name="filePath">
    /// The file path of the source file containing the calling method. Automatically set to the calling file's path if not provided.
    /// </param>
    /// <returns>
    /// An <see cref="IDisposable"/> that represents the profiling scope. Returns null if profiling is disabled via <see cref="ProfilerOptions"/>.
    /// </returns>
    public IDisposable? BeginScope([CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
    {
        if (!_options.Enabled)
        {
            return null;
        }

        var className = Path.GetFileNameWithoutExtension(filePath);
        return new ProfileScope(this, className, methodName);
    }

    private static partial class Log
    {
        [LoggerMessage(
            LogLevel.Debug,
            Message = "[Perf] {ClassName}.{MethodName} took {ElapsedMs:F4}ms (SimTime delta: {SimDelta:F4}s)"
        )]
        public static partial void PerformanceResult(
            ILogger logger,
            string className,
            string methodName,
            double elapsedMs,
            double simDelta
        );
    }

    private sealed class ProfileScope(Profiler parent, string className, string methodName) : IDisposable
    {
        private readonly long _startTicks = Stopwatch.GetTimestamp();
        private readonly double _startGameTime = parent._gameTime?.TotalTime ?? 0;

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            TimeSpan elapsed = Stopwatch.GetElapsedTime(_startTicks);
            var gameTimeElapsed = (parent._gameTime?.TotalTime ?? 0D) - _startGameTime;

            if (elapsed.TotalMilliseconds >= parent._options.LogThresholdMs)
            {
                Log.PerformanceResult(
                    parent._logger,
                    className,
                    methodName,
                    elapsed.TotalMilliseconds,
                    gameTimeElapsed
                );
            }

            _disposed = true;
        }
    }
}
