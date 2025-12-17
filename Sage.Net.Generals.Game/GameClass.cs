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
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Sage.Net.Core.GameEngine.Common;
using Sage.Net.Generals.GameEngine.Common;
using Sage.Net.Generals.GameEngine.GameEngineDevice.SdlDevice;
using Sage.Net.NativeHelpers.Sdl3;

namespace Sage.Net.Generals.Game;

internal sealed class GameClass(ILogger logger) : IDisposable
{
    private const int DefaultDisplayWidth = 800;
    private const int DefaultDisplayHeight = 600;

    private static int _handlersInstalled; // 0: not installed, 1: installed

    private WindowHandle? _window;
    private SurfaceHandle? _loadScreenBitmap;
    private bool _running = true;
    private bool _paintSplash = true;
    private bool _disposed;

    public void Run()
    {
        Initialize();
        Update();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _loadScreenBitmap?.Dispose();
        _window?.Dispose();
        SdlSubsystems.QuitAll();

        FramePacer.TheFramePacer = null;

        GameEngine.Common.GameEngine.TheGameEngine?.Dispose();
        GameEngine.Common.GameEngine.TheGameEngine = null;

        VersionHelper.TheVersion = null;

#if RTS_ENABLE_CRASHDUMP
        MiniDumper.ShutdownMiniDumper();
#endif

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

    private static string? GetMetadata(string key)
    {
        var asm = Assembly.GetExecutingAssembly();
        return asm.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => string.Equals(a.Key, key, StringComparison.OrdinalIgnoreCase))
            ?.Value?.Trim();
    }

    private static int GetMetadataInt(string key) =>
        int.TryParse(GetMetadata(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;

    private static long GetMetadataLong(string key) =>
        long.TryParse(GetMetadata(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;

    private static (int Major, int Minor, int BuildNumber, int LocalBuildNumber) GetVersionNumbers()
    {
        var asm = Assembly.GetExecutingAssembly();

        // Prefer FileVersion for the "BuildNumber" if you want it to change per build.
        var fileVer = asm.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version; // "A.B.C.D"
        if (Version.TryParse(fileVer, out Version? fv))
        {
            return (fv.Major, fv.Minor, fv.Build, GetMetadataInt("LocalBuildNumber"));
        }

        // Fallback: AssemblyVersion
        Version av = asm.GetName().Version ?? new Version(0, 0, 0, 0);
        return (av.Major, av.Minor, av.Build, GetMetadataInt("LocalBuildNumber"));
    }

    private static (string User, string Location, string BuildTime, string BuildDate) GetBuildInfo()
    {
        var user = GetMetadata("BuildUser") ?? string.Empty;
        var loc = GetMetadata("BuildLocation") ?? string.Empty;

        // Deterministic build time from Git commit time
        var unix = GetMetadataLong("GitCommitUnixTime");
        if (unix <= 0)
        {
            return (user, loc, string.Empty, string.Empty);
        }

        DateTime dt = DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime;
        var date = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var time = dt.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        return (user, loc, time, date);
    }

    private void InitializeAppSdl(bool runWindowed)
    {
        if (!SdlSubsystems.TryInitVideo())
        {
            throw new InvalidOperationException($"Unable to initialize SDL video subsystem ({SdlError.LastMessage}).");
        }

        if (_loadScreenBitmap!.IsInvalid)
        {
            return;
        }

        _window = WindowHandle.CreateSplashWindow("Generals Game", _loadScreenBitmap!.Width, _loadScreenBitmap!.Height);
        if (_window.IsInvalid)
        {
            throw new InvalidOperationException($"Unable to create splash window ({SdlError.LastMessage}).");
        }

        if (runWindowed)
        {
            _ = _window.TrySetSize(DefaultDisplayWidth, DefaultDisplayHeight);
        }

        SdlEvents.OnQuit += (_, _) => _running = false;

        SdlEvents.OnWindowExposed += (_, _) =>
        {
            if (_paintSplash)
            {
                PaintBitmap1To1();
            }
        };

        if (!runWindowed)
        {
            _paintSplash = false;
        }
    }

    private void Update()
    {
        while (_running)
        {
            SdlEvents.PollEvents();
        }
    }

    private void PaintBitmap1To1()
    {
        if (_window is null || _loadScreenBitmap is null)
        {
            return;
        }

        if (_window.IsInvalid || _loadScreenBitmap.IsInvalid)
        {
            return;
        }

        SurfaceHandle windowSurface = _window!.Surface;
        if (windowSurface.IsInvalid)
        {
            return;
        }

        if (!_window.TryShow())
        {
            return;
        }

        if (!SurfaceHandle.TryBlit(_loadScreenBitmap, null, windowSurface, null))
        {
            return;
        }

        _ = _window!.TryUpdateSurface();
    }

    private void Initialize()
    {
        InstallUnhandledExceptionHandlers(logger);

        // TODO: Allow users to change the screen bitmap
        _loadScreenBitmap = SurfaceHandle.LoadBmp("Install_Final.bmp");

        CommandLine.ParseCommandLineForStartup(logger);

        InitializeAppSdl(GlobalData.TheGlobalData!.Windowed);

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
        PaintBitmap1To1();

        VersionHelper.TheVersion = new VersionHelper();
        (int Major, int Minor, int BuildNumber, int LocalBuildNumber) versionNumbers = GetVersionNumbers();
        (string User, string Location, string BuildTime, string BuildDate) buildInfo = GetBuildInfo();
        VersionHelper.TheVersion.SetVersion(versionNumbers, buildInfo);

        FramePacer.TheFramePacer = new FramePacer { FramesPerSecondLimitEnabled = true };
        GameEngine.Common.GameEngine.TheGameEngine = CreateGameEngine();
        GameEngine.Common.GameEngine.TheGameEngine.Initialize();
    }

    private SdlGameEngine CreateGameEngine() => new(logger);
}
