// -----------------------------------------------------------------------
// <copyright file="StackDump.cs" company="Sage.Net">
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Sage.Net.Generals.GameEngine;

/// <summary>Provides functionality for capturing and logging unhandled exceptions.</summary>
public static partial class StackDump
{
    /// <summary>Gets the last captured exception dump.</summary>
    public static string LastErrorDump { get; private set; } = string.Empty;

    /// <summary>Captures and logs an unhandled exception dump.</summary>
    /// <param name="logger">The logger to use for logging.</param>
    /// <param name="reason">The reason or context for the exception dump.</param>
    /// <param name="ex">The exception to dump.</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
    /// <remarks>This method is idempotent.</remarks>
    public static void DumpExceptionInfo(ILogger logger, string reason, Exception? ex)
    {
        ArgumentNullException.ThrowIfNull(logger);
        reason = string.IsNullOrWhiteSpace(reason) ? "unknown" : reason;
        StringBuilder sb = new(4096);

        _ = sb.AppendLine()
            .AppendLine("********** EXCEPTION DUMP ****************")
            .AppendLine(CultureInfo.InvariantCulture, $"UTC: {DateTimeOffset.UtcNow:o}")
            .AppendLine(CultureInfo.InvariantCulture, $"Reason: {reason}")
            .AppendLine(CultureInfo.InvariantCulture, $"PID: {Environment.ProcessId}")
            .AppendLine(CultureInfo.InvariantCulture, $"OS: {RuntimeInformation.OSDescription}")
            .AppendLine(CultureInfo.InvariantCulture, $"Framework: {RuntimeInformation.FrameworkDescription}")
            .AppendLine(CultureInfo.InvariantCulture, $"Arch: {RuntimeInformation.ProcessArchitecture}")
            .AppendLine();

        _ = ex is null
            ? sb.AppendLine("Exception: <null>")
            : sb.AppendLine("Exception:")
                .AppendLine(CultureInfo.InvariantCulture, $"  Type: {ex.GetType().FullName}")
                .AppendLine(CultureInfo.InvariantCulture, $"  Message: {ex.Message}")
                .AppendLine(CultureInfo.InvariantCulture, $"  HResult: 0x{ex.HResult:X8}")
                .AppendLine(CultureInfo.InvariantCulture, $"  Description: {Describe(ex)}")
                .AppendLine()
                .AppendLine(ex.ToString())
                .AppendLine()
                .AppendLine("Caller stack (DumpExceptionInfo call site):")
                .AppendLine(Environment.StackTrace)
                .AppendLine()
                .AppendLine("Process/Thread:")
                .AppendLine(
                    CultureInfo.InvariantCulture,
                    $"  ProcessName: {Safe(() => Process.GetCurrentProcess().ProcessName) ?? "<unknown>"}"
                )
                .AppendLine(CultureInfo.InvariantCulture, $"  ThreadId: {Environment.CurrentManagedThreadId}")
                .AppendLine("********** END EXCEPTION DUMP ****************")
                .AppendLine();

        LastErrorDump = sb.ToString();

        Log.UnhandledDump(logger, reason, ex?.HResult ?? 0, ex?.GetType().FullName ?? "<null>");

        Log.ExceptionDump(logger, LastErrorDump);
    }

    private static partial class Log
    {
        [LoggerMessage(
            LogLevel.Error,
            Message = "Unhandled exception dump captured. Reason={Reason} HResult=0x{HResult:X8} ExceptionType={ExceptionType}"
        )]
        public static partial void UnhandledDump(ILogger logger, string reason, int hresult, string exceptionType);

        [LoggerMessage(LogLevel.Error, Message = "{ExceptionDump}")]
        public static partial void ExceptionDump(ILogger logger, string exceptionDump);
    }

    private static string Describe(Exception ex) =>
        ex switch
        {
            NullReferenceException => "Attempted to dereference a null object reference.",
            IndexOutOfRangeException => "Attempted to access an array/list element that is out of bounds.",
            DivideByZeroException => "Attempted to divide by zero.",
            AccessViolationException =>
                "A fatal memory access violation occurred (often indicates native/interp issues).",
            OutOfMemoryException => "The runtime could not allocate more memory.",
            StackOverflowException => "The call stack overflowed (deep or infinite recursion).",
            _ => $"Unhandled exception ({ex.GetType().FullName ?? "<null>"}).",
        };

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid crash paths here."
    )]
    private static string? Safe(Func<string> f)
    {
        try
        {
            return f();
        }
        catch
        {
            return null;
        }
    }
}
