// -----------------------------------------------------------------------
// <copyright file="SdlSplashScreen.cs" company="Sage.Net">
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
using Sage.Net.Abstractions;
using Sage.Net.Io.Extensions;
using Sage.Net.LoggerHelper;

namespace Sage.Net.Sdl3;

/// <inheritdoc/>
public sealed partial class SdlSplashScreen(ILogger<SdlSplashScreen> logger) : ISplashScreen
{
    private Sdl.Window? _window;
    private Sdl.Surface? _windowSurface;
    private Sdl.Surface? _bmp;

    /// <inheritdoc/>
    public bool InitializationIsComplete { get; set; }

    /// <summary>
    /// Initializes and sets up the splash screen using the provided base game path and game options.
    /// </summary>
    /// <param name="baseGamePath">
    /// The root directory of the game, used to locate the default splash screen bitmap if a mod-specific file is not provided.
    /// </param>
    /// <param name="options">
    /// A <see cref="GameOptions"/> object containing configuration settings for the splash screen,
    /// including the optional mod files path and splash screen bitmap file name.
    /// </param>
    public void Initialize(string baseGamePath, GameOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        using IDisposable? logContext = LogContext.BeginOperation(logger, nameof(Initialize));

        if (!Sdl.Init(Sdl.InitVideo))
        {
            throw new InvalidOperationException($"SDL initialization failed ({Sdl.GetError}).");
        }

        var bitmapPath = string.Empty;
        if (options.ModFilesPath is not null && options.ModSplashScreenBmpFileName is not null)
        {
            var resolvedModPath = options.ModFilesPath.ResolveTildePath()!;
            if (Directory.Exists(resolvedModPath))
            {
                var tempPath = Path.Combine(resolvedModPath, options.ModSplashScreenBmpFileName);
                if (File.Exists(tempPath))
                {
                    bitmapPath = tempPath;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(bitmapPath))
        {
            // This is the actual shipped splash screen bitmap.
            bitmapPath = Path.Combine(baseGamePath, "Install_Final.bmp");
        }

        Log.UsingBitmapFile(logger, bitmapPath);

        _bmp = Sdl.Surface.FromBmp(bitmapPath);
        if (_bmp.IsInvalid)
        {
            Log.FailedToLoadBitmapFile(logger, bitmapPath);
            return;
        }

        _window = Sdl.Window.Create($"{options.WindowTitle} - Loading", _bmp.Width, _bmp.Height, Sdl.Window.Borderless);
        if (_window.IsInvalid)
        {
            Log.FailedToCreateWindow(logger, Sdl.GetError);
            return;
        }

        _windowSurface = _window.Surface;
        if (_windowSurface.IsInvalid)
        {
            Log.FailedToGetWindowSurface(logger, Sdl.GetError);
            return;
        }

        _ = Sdl.Surface.Blit(_bmp, _windowSurface);
        _ = _window.UpdateSurface();
    }

    /// <summary>
    /// Processes pending SDL events in the event queue to ensure the application remains responsive
    /// and properly handles windowing, particularly on environments such as Linux with Wayland.
    /// </summary>
    public void Update()
    {
        while (Sdl.PollEvent(out _))
        {
            // Keep polling events
            // This is ESSENTIAL for systems like Linux on Wayland that require event
            // handling to show a window.
        }
    }

    /// <summary>
    /// Renders the splash screen by blitting the bitmap surface onto the window surface.
    /// </summary>
    /// <remarks>
    /// This method verifies the validity of the window, window surface, and bitmap surface before attempting
    /// the rendering operation. If any of these surfaces are invalid, the method exits without performing any rendering.
    /// </remarks>
    public void Draw()
    {
        if (_window?.IsInvalid == true || _windowSurface?.IsInvalid == true || _bmp?.IsInvalid == true)
        {
            return;
        }

        _ = _window!.UpdateSurface();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _bmp?.Dispose();
        _windowSurface?.Dispose();
        _window?.Dispose();

        // TODO: Move this to the main loop system (window system maybe?)
        Sdl.Quit();
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Debug, Message = "Using bitmap file: {BitmapPath}")]
        public static partial void UsingBitmapFile(ILogger logger, string bitmapPath);

        [LoggerMessage(LogLevel.Error, Message = "Failed to load bitmap file: {BitmapPath}")]
        public static partial void FailedToLoadBitmapFile(ILogger logger, string bitmapPath);

        [LoggerMessage(
            LogLevel.Error,
            Message = "Failed to create window ({Error}), the splash screen will not be displayed."
        )]
        public static partial void FailedToCreateWindow(ILogger logger, string error);

        [LoggerMessage(
            LogLevel.Error,
            Message = "Failed to get the window surface ({Error}), the splash screen will not be displayed."
        )]
        public static partial void FailedToGetWindowSurface(ILogger logger, string error);
    }
}
