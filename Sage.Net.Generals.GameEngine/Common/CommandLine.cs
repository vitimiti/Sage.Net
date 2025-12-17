// -----------------------------------------------------------------------
// <copyright file="CommandLine.cs" company="Sage.Net">
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

using Microsoft.Extensions.Logging;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Provides functionality for parsing command-line arguments.</summary>
public static partial class CommandLine
{
    private static readonly CommandLineParameter[] ParamsForStartup =
    [
        new("-win", ParseWin),
        new("-fullscreen", ParseNoWin),
    ];

    /// <summary>Parses command-line arguments for startup.</summary>
    /// <param name="logger">The logger to use.</param>
    /// <remarks>This method is idempotent.</remarks>
    public static void ParseCommandLineForStartup(ILogger logger)
    {
        CreateGlobalData(logger);
        if (GlobalData.TheGlobalData!.CommandLineData.HasParsedCommandLineForStartup)
        {
            return;
        }

        GlobalData.TheGlobalData!.CommandLineData.HasParsedCommandLineForStartup = true;
        ParseCommandLine(logger, ParamsForStartup);
    }

    private static void CreateGlobalData(ILogger logger)
    {
        if (GlobalData.TheGlobalData is null)
        {
            GlobalData.TheWritableGlobalData = new GlobalData(logger);
        }
    }

    private static void ParseCommandLine(ILogger logger, ReadOnlySpan<CommandLineParameter> paramsForStartup)
    {
        var args = Environment.GetCommandLineArgs();

#if DEBUG
        LogCommandLineArgs(logger, args);
#endif

        var argIndex = 1;
        while (argIndex < args.Length)
        {
            if (TryConsumeParam(args, argIndex, paramsForStartup, out var consumed))
            {
                argIndex += consumed;
            }
            else
            {
                argIndex++;
            }
        }
    }

#if DEBUG
    private static void LogCommandLineArgs(ILogger logger, string[] args)
    {
        var tail = args.Length > 1 ? string.Join(' ', args, 1, args.Length - 1) : string.Empty;
        Log.Args(logger, $"Command-line args: {tail}");
    }
#endif

    private static bool ArgEquals(string arg, string name) =>
        arg.Length == name.Length && arg.Equals(name, StringComparison.OrdinalIgnoreCase);

    private static bool TryConsumeParam(
        string[] args,
        int argIndex,
        ReadOnlySpan<CommandLineParameter> parameters,
        out int consumed
    )
    {
        consumed = 0;
        var current = args[argIndex];

        foreach (CommandLineParameter p in parameters)
        {
            if (!ArgEquals(current, p.Name))
            {
                continue;
            }

            consumed = p.ArgsParser(args.AsSpan(argIndex));
            if (consumed <= 0)
            {
                consumed = 1;
            }

            return true;
        }

        return false;
    }

    private static int ParseWin(ReadOnlySpan<string> args)
    {
        GlobalData.TheWritableGlobalData!.Windowed = true;
        return 1;
    }

    private static int ParseNoWin(ReadOnlySpan<string> args)
    {
        GlobalData.TheWritableGlobalData!.Windowed = false;
        return 1;
    }

    private sealed record CommandLineParameter(string Name, Func<ReadOnlySpan<string>, int> ArgsParser);

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Debug, "Command-line args: {Args}")]
        public static partial void Args(ILogger logger, string args);
    }
}
