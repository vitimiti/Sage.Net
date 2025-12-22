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
using Sage.Net.Game.Scenes;
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

    private IScene? _scene;
    private bool _running = true;
    private bool _disposed;

    public SageGame(IServiceProvider services)
    {
        _logger = services.GetRequiredService<ILogger<SageGame>>();
        using IDisposable? logContext = LogContext.BeginOperation(_logger, nameof(SageGame));

        _services = services;
        _gameOptions = services.GetRequiredService<IOptions<GameOptions>>().Value;

        foreach (var path in _gameOptions.BaseGamePaths)
        {
            var resolved = path.ResolveTildePath()!;
            Log.BaseGamePathCheck(_logger, path, resolved, Directory.Exists(resolved));
        }

        _baseGamePath =
            _gameOptions.BaseGamePaths.Select(path => path.ResolveTildePath()!).FirstOrDefault(Directory.Exists)
            ?? Environment.CurrentDirectory;

        Log.BaseGamePath(_logger, _baseGamePath);

        var rawModPath = _gameOptions.ModFilesPath;
        if (string.IsNullOrWhiteSpace(rawModPath))
        {
            return;
        }

        _modBigFilesPath = rawModPath.ResolveTildePath()!;
        if (!Directory.Exists(_modBigFilesPath))
        {
            return;
        }

        Log.LoadingMods(_logger, _modBigFilesPath);
        _modBigFilesExtension = string.IsNullOrWhiteSpace(_gameOptions.ModBigFilesExtension)
            ? ".big"
            : _gameOptions.ModBigFilesExtension;

        Log.ModBigExtension(_logger, _modBigFilesExtension);
    }

    public void Run()
    {
        Initialize();
        while (_running)
        {
            _gameTime.Update();

            Update(_gameTime.DeltaTime);
            Draw();
        }

        Log.QuitRequested(_logger);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _scene?.Dispose();

        _disposed = true;
    }

    private void Initialize()
    {
        _gameTime.Start();
        _scene = new SplashScene(_services, _baseGamePath, _gameOptions, _modBigFilesPath, _modBigFilesExtension);
        _scene.Initialize();
    }

    private void Update(double deltaTime)
    {
        if (_scene is null)
        {
            return;
        }

        _scene.Update(deltaTime);
        if (_scene.NextScene is { } nextScene)
        {
            Log.SceneTransition(_logger, nextScene.GetType().Name);
            _scene.Dispose();
            _scene = nextScene;
            nextScene.Initialize();
        }

        _running = !_scene.QuitRequested;
    }

    private void Draw() => _scene?.Draw();

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Trace, Message = "Checking base game path: {Raw} -> {Resolved} (Exists: {Exists})")]
        public static partial void BaseGamePathCheck(ILogger logger, string raw, string resolved, bool exists);

        [LoggerMessage(LogLevel.Debug, Message = "Base game path: {Path}")]
        public static partial void BaseGamePath(ILogger logger, string path);

        [LoggerMessage(LogLevel.Information, Message = "Loading mods from: {Path}")]
        public static partial void LoadingMods(ILogger logger, string path);

        [LoggerMessage(LogLevel.Debug, Message = "Using mod BIG files extension: {Extension}")]
        public static partial void ModBigExtension(ILogger logger, string extension);

        [LoggerMessage(LogLevel.Debug, Message = "Transitioning to scene {SceneName}")]
        public static partial void SceneTransition(ILogger logger, string sceneName);

        [LoggerMessage(LogLevel.Debug, Message = "Quit requested, terminating game loop.")]
        public static partial void QuitRequested(ILogger logger);
    }
}
