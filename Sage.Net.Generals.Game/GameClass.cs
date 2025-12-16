// -----------------------------------------------------------------------
// <copyright file="GameClass.cs" company="Sage.Net">
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
using Microsoft.Extensions.Logging;
using Sage.Net.Core.GameEngine.Common;
using Sage.Net.Generals.GameEngine;
using Sage.Net.NativeHelpers.Sdl3;

namespace Sage.Net.Generals.Game;

internal sealed class GameClass(ILogger logger) : IDisposable
{
    private static int _handlersInstalled; // 0: not installed, 1: installed

    private Surface? _loadScreenBitmap;
    private bool _disposed;

    public void Run() => Initialize();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _loadScreenBitmap?.Dispose();
        throw new Exception("Not implemented");

        _disposed = true;
    }

    [SuppressMessage(
        "csharpsquid",
        "S1172: Unused method parameters should be removed.",
        Justification = "This is a false positive."
    )]
    private static void InstallUnhandledExceptionHandlers(ILogger logger)
    {
        if (Interlocked.Exchange(ref _handlersInstalled, 1) != 0)
        {
            return;
        }

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
#if RTS_DEBUG
            var ex = e.ExceptionObject as Exception;
            StackDump.DumpExceptionInfo(logger, "unhandled", ex);
#endif

#if RTS_ENABLE_CRASHDUMP
            MiniDumper.Instance?.TriggerMiniDumpsForException("unhandled");
            MiniDumper.ShutdownMiniDumper();
#endif
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
#if RTS_DEBUG
            StackDump.DumpExceptionInfo(logger, "unobserved-task", e.Exception);
#endif

#if RTS_ENABLE_CRASHDUMP
            MiniDumper.Instance?.TriggerMiniDumpsForException("unobserved-task");
#endif
            e.SetObserved();
        };
    }

    private void Initialize()
    {
        InstallUnhandledExceptionHandlers(logger);

        // TODO: Allow users to change the screen bitmap
        _loadScreenBitmap = Surface.LoadBmp("Install_Final.bmp");

#if RTS_ENABLE_CRASHDUMP
        MiniDumper.InitMiniDumper(
            Path.Combine(
                Environment.GetFolderPath(
                    OperatingSystem.IsWindows()
                        ? Environment.SpecialFolder.LocalApplicationData
                        : Environment.SpecialFolder.ApplicationData
                ),
                "GeneralsGame"
            ),
            "GeneralsGame"
        );
#endif
    }
}
