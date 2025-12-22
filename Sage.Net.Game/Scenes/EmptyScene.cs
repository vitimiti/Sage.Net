// -----------------------------------------------------------------------
// <copyright file="EmptyScene.cs" company="Sage.Net">
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

namespace Sage.Net.Game.Scenes;

internal sealed class EmptyScene(IServiceProvider services) : IScene
{
    private readonly IGameWindow _gameWindow = services.GetRequiredService<IGameWindow>();

    private bool _disposed;

    public bool QuitRequested { get; private set; }

    public IScene? NextScene => null;

    public void Initialize() => _gameWindow.Initialize();

    public void Update(double deltaTime)
    {
        _gameWindow.Update();
        QuitRequested = _gameWindow.QuitRequested;
    }

    public void Draw() => _gameWindow.Draw();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _gameWindow.Dispose();

        _disposed = true;
    }
}
