// -----------------------------------------------------------------------
// <copyright file="SageGame.cs" company="Sage.Net">
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sage.Net.Abstractions;
using Sage.Net.Io.Big;
using Sage.Net.Io.Extensions;
using Sage.Net.LoggerHelper;

namespace Sage.Net.Game;

internal sealed partial class SageGame : IDisposable
{
    private readonly ILogger<SageGame> _logger;
    private readonly IServiceProvider _services;
    private readonly GameOptions _gameOptions;
    private readonly string _baseGamePath;
    private readonly GameTime _gameTime = new();
    private readonly string? _modBigFilesPath;
    private readonly string? _modBigFilesExtension;
    private readonly bool _loadMods;
    private readonly ISplashScreen _splashScreen;
    private readonly ArchiveFileSystem _archiveFileSystem = new();

    private bool _running = true;
    private bool _disposed;

    public SageGame(IServiceProvider services)
    {
        _logger = services.GetRequiredService<ILogger<SageGame>>();
        using IDisposable? logContext = LogContext.BeginOperation(_logger, nameof(SageGame));

        _services = services;
        _gameOptions = services.GetRequiredService<IOptions<GameOptions>>().Value;
        _splashScreen = services.GetRequiredService<ISplashScreen>();

        foreach (var path in _gameOptions.BaseGamePaths)
        {
            var resolved = path.ResolveTildePath()!;
            Log.LogBaseGamePathCheck(_logger, path, resolved, Directory.Exists(resolved));
        }

        _baseGamePath =
            _gameOptions.BaseGamePaths.Select(path => path.ResolveTildePath()!).FirstOrDefault(Directory.Exists)
            ?? Environment.CurrentDirectory;

        Log.LogBaseGamePath(_logger, _baseGamePath);

        var rawModPath = _gameOptions.ModFilesPath;
        if (string.IsNullOrWhiteSpace(rawModPath))
        {
            return;
        }

        _modBigFilesPath = rawModPath.ResolveTildePath()!;
        _loadMods = Directory.Exists(_modBigFilesPath);
        if (!_loadMods)
        {
            return;
        }

        Log.LogLoadingMods(_logger, _modBigFilesPath);
        _modBigFilesExtension = string.IsNullOrWhiteSpace(_gameOptions.ModBigFilesExtension)
            ? ".big"
            : _gameOptions.ModBigFilesExtension;

        Log.LogModBigExtension(_logger, _modBigFilesExtension);
    }

    public void Run()
    {
        Initialize();
        while (_running)
        {
            _gameTime.Update();

            Update(_gameTime.DeltaTime);
            Draw(_gameTime.DeltaTime);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _splashScreen.Dispose();
        _archiveFileSystem.Dispose();

        _disposed = true;
    }

    private void Initialize()
    {
        _gameTime.Start();
        _splashScreen.Initialize(_baseGamePath, _gameOptions);

        // TODO: Run an actual initialization here.
        _ = Task.Run(PerformBackgroundInitialization);
    }

    private void Update(double deltaTime)
    {
        _splashScreen.Update();
        if (_splashScreen.InitializationIsComplete)
        {
            // TODO: Go into the actual game loop here.
            _running = false;
        }
    }

    private void Draw(double deltaTime) => _splashScreen.Draw();

    private void PerformBackgroundInitialization()
    {
        _archiveFileSystem.Initialize(_services, _baseGamePath, _modBigFilesPath, _modBigFilesExtension);

        _splashScreen.InitializationIsComplete = true;
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Trace, Message = "Checking base game path: {Raw} -> {Resolved} (Exists: {Exists})")]
        public static partial void LogBaseGamePathCheck(ILogger logger, string raw, string resolved, bool exists);

        [LoggerMessage(LogLevel.Debug, Message = "Base game path: {Path}")]
        public static partial void LogBaseGamePath(ILogger logger, string path);

        [LoggerMessage(LogLevel.Information, Message = "Loading mods from: {Path}")]
        public static partial void LogLoadingMods(ILogger logger, string path);

        [LoggerMessage(LogLevel.Debug, Message = "Using mod BIG files extension: {Extension}")]
        public static partial void LogModBigExtension(ILogger logger, string extension);
    }
}
