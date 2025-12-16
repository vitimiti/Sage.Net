// -----------------------------------------------------------------------
// <copyright file="Sdl.cs" company="Sage.Net">
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

using System.Runtime.InteropServices;

namespace Sage.Net.NativeHelpers.Sdl3.NativeImports;

internal static partial class Sdl
{
    private const string Sdl3 = "SDL3";

    private static readonly Dictionary<string, bool> Sdl3Names = new()
    {
        { "SDL3.dll", OperatingSystem.IsWindows() },
        { "libSDL3.so", OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD() },
        { "libSDL3.dylib", OperatingSystem.IsMacOS() },
    };

    static Sdl() =>
        NativeLibrary.SetDllImportResolver(
            typeof(Sdl).Assembly,
            (name, assembly, path) =>
                NativeLibrary.Load(
                    name switch
                    {
                        Sdl3 => Sdl3Names.FirstOrDefault(pair => pair.Value).Key,
                        _ => name,
                    },
                    assembly,
                    path
                )
        );
}
