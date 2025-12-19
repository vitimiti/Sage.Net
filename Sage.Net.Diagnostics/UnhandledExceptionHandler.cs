// -----------------------------------------------------------------------
// <copyright file="UnhandledExceptionHandler.cs" company="Sage.Net">
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sage.Net.Diagnostics;

/// <summary>
/// Provides global hooks to capture unhandled exceptions, log them as critical errors,
/// and trigger diagnostic process dumps.
/// </summary>
public static partial class UnhandledExceptionHandler
{
    /// <summary>
    /// Installs the unhandled exception handler into the current <see cref="AppDomain"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve logging and dump services.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid cascading faults during error handling."
    )]
    public static void Install(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        ILogger? logger = serviceProvider
            .GetService<ILoggerFactory>()
            ?.CreateLogger("Sage.Net.Diagnostics.UnhandledException");

        AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
        {
            if (e.Exception is { } exception && logger is not null)
            {
                Log.LogFirstChanceException(logger, exception);
            }
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            try
            {
                var exception = (Exception)args.ExceptionObject;
                IDumpService? dumpService = serviceProvider.GetService<IDumpService>();
                if (logger is not null)
                {
                    Log.LogUnhandledException(logger, exception);
                }

                dumpService?.WriteDump("UnhandledException");

                ICrashDialogProvider? dialogProvider = serviceProvider.GetService<ICrashDialogProvider>();
                if (dialogProvider is null)
                {
                    return;
                }

                var path = dumpService?.DumpDirectoryPath ?? "***Unknown Dump Folder***";
                dialogProvider.ShowCrashDialog(exception, path);
            }
            catch
            {
                // Let the OS deal with any exception here!
            }
        };
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Trace, Message = "First-chance exception detected.")]
        public static partial void LogFirstChanceException(ILogger logger, Exception exception);

        [LoggerMessage(
            LogLevel.Critical,
            Message = "An unhandled exception occurred. The application will now terminate."
        )]
        public static partial void LogUnhandledException(ILogger logger, Exception exception);
    }
}
