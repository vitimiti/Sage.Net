// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Sage.Net">
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sage.Net.Abstractions;
using Sage.Net.Diagnostics;
using Sage.Net.Game;
using Sage.Net.Sdl3;

var switchMappings = new Dictionary<string, string>
{
    { "-mod", "mod" },
    { "-log", "Logging:LogLevel:Sage.Net" },
    { "-id", "Sage:Game:GameId" },
    { "-splash", "Sage:Game:ModSplashScreenBmpFileName" },
    { "-modfiles", "Sage:Game:ModFilesPath" },
    { "-modbigextension", "Sage:Game:ModBigFilesExtension" },
};

IConfiguration argsConfig = new ConfigurationBuilder().AddCommandLine(args, switchMappings).Build();

var modName = argsConfig["mod"];

IConfigurationBuilder configBuilder = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("settings.json", optional: false, reloadOnChange: true);

#if DEBUG
configBuilder.AddJsonFile("settings.Debug.json", optional: true, reloadOnChange: true);
#endif

if (!string.IsNullOrEmpty(modName))
{
    configBuilder.AddJsonFile($"settings.{modName}.json", optional: true, reloadOnChange: true);
#if DEBUG
    configBuilder.AddJsonFile($"settings.{modName}.Debug.json", optional: true, reloadOnChange: true);
#endif
}

IConfigurationRoot configuration = configBuilder.AddCommandLine(args, switchMappings).Build();

var services = new ServiceCollection();

services.Configure<GameOptions>(configuration.GetSection("Sage:Game"));

services.AddLogging(builder =>
{
    _ = builder
        .AddConfiguration(configuration.GetSection("Logging"))
        .AddConsole()
        .AddSimpleConsole(options =>
        {
            options.SingleLine = false;
            options.IncludeScopes = true;
            options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
            options.UseUtcTimestamp = false;
        });
});

services.AddSageDiagnostics(configuration);

services.AddSingleton<ICrashDialogProvider, SdlCrashDialogProvider>();

services.AddSingleton<ISplashScreen, SdlSplashScreen>();

using ServiceProvider serviceProvider = services.BuildServiceProvider();

UnhandledExceptionHandler.Install(serviceProvider);

using SageGame game = new(serviceProvider);

game.Run();
