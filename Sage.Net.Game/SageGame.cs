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

namespace Sage.Net.Game;

internal sealed class SageGame : IDisposable
{
    private readonly ILogger<SageGame> _logger;
    private readonly IServiceProvider _services;
    private readonly GameOptions _gameOptions;
    private readonly string _baseGamePath;
    private readonly GameTime _gameTime = new();
    private readonly string? _modBigFilesPath;
    private readonly string? _modBigFilesExtension;
    private readonly bool _loadMods;

    public SageGame(IServiceProvider services)
    {
        _logger = services.GetRequiredService<ILogger<SageGame>>();
        _services = services;
        _gameOptions = services.GetRequiredService<IOptions<GameOptions>>().Value;

        _baseGamePath =
            _gameOptions.BaseGamePaths.Select(ResolvePath).FirstOrDefault(Directory.Exists)
            ?? Environment.CurrentDirectory;

        var rawModPath = _gameOptions.ModBigFilesPath;
        if (!string.IsNullOrWhiteSpace(rawModPath))
        {
            _modBigFilesPath = ResolvePath(rawModPath);
            _loadMods = Directory.Exists(_modBigFilesPath);

            _modBigFilesExtension = string.IsNullOrWhiteSpace(_gameOptions.ModBigFilesExtension)
                ? ".big"
                : _gameOptions.ModBigFilesExtension;
        }
    }

    public void Run()
    {
        Initialize();
        var counter = 0;
        while (true)
        {
            _gameTime.Update();

            Update(_gameTime.DeltaTime);
            Draw(_gameTime.DeltaTime);
            counter++;

            if (counter >= 60)
            {
                // TODO: implement a proper interface for the game loop management.
                break;
            }
        }
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    private static string ResolvePath(string path) =>
        path.StartsWith('~')
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                path.TrimStart('~', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            )
            : path;

    private void Initialize() => _gameTime.Start();

    private void Update(double deltaTime) => throw new NotImplementedException();

    private void Draw(double deltaTime) => throw new NotImplementedException();
}
