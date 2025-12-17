// -----------------------------------------------------------------------
// <copyright file="SdlSubsystems.cs" company="Sage.Net">
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

using Sage.Net.NativeHelpers.Sdl3.NativeImports;

namespace Sage.Net.NativeHelpers.Sdl3;

/// <summary>Provides access to SDL subsystem initialization functionality.</summary>
public static class SdlSubsystems
{
    /// <summary>Initializes the video subsystem.</summary>
    /// <returns><see langword="true"/> if the video subsystem was successfully initialized; otherwise, <see langword="false"/>.</returns>
    public static bool TryInitVideo() => Sdl.Init(Sdl.InitFlags.Video);

    /// <summary>Quits all initialized subsystems.</summary>
    public static void QuitAll() => Sdl.Quit();
}
