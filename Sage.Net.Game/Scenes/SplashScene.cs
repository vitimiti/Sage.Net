// -----------------------------------------------------------------------
// <copyright file="SplashScene.cs" company="Sage.Net">
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
using Sage.Net.Abstractions;
using Sage.Net.Io.Big;

namespace Sage.Net.Game.Scenes;

internal sealed class SplashScene(
    IServiceProvider services,
    string baseGamePath,
    GameOptions gameOptions,
    string? modBigFilesPath,
    string? modBigFilesExtension
) : IScene
{
    private readonly ISplashScreen _splashScreen = services.GetRequiredService<ISplashScreen>();
    private readonly ArchiveFileSystem _archiveFileSystem = new();

    private IScene? _nextScene;
    private bool _disposed;

    public bool QuitRequested => false;

    public IScene? NextScene => _splashScreen.InitializationIsComplete ? new EmptyScene(services) : null;

    public void Initialize()
    {
        _splashScreen.Initialize(baseGamePath, gameOptions);
        _ = Task.Run(PerformBackgroundInitialization);
    }

    public void Update(double deltaTime)
    {
        _splashScreen.Update();
        if (_splashScreen.InitializationIsComplete && _nextScene is null)
        {
            _nextScene = new EmptyScene(services);
        }
    }

    public void Draw() => _splashScreen.Draw();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _archiveFileSystem.Dispose();
        _splashScreen.Dispose();

        _disposed = true;
    }

    private void PerformBackgroundInitialization()
    {
        _archiveFileSystem.Initialize(services, baseGamePath, modBigFilesPath, modBigFilesExtension);

        _splashScreen.InitializationIsComplete = true;
    }
}
