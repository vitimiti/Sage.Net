// -----------------------------------------------------------------------
// <copyright file="SdlEvents.cs" company="Sage.Net">
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

/// <summary>Provides access to SDL event-handling functionality.</summary>
public static class SdlEvents
{
    /// <summary>An event that is triggered when the application receives a quit signal. This can occur, for example, when the user closes the application window or initiates a quit command.</summary>
    public static event EventHandler<EventArgs>? OnQuit;

    /// <summary>An event that is triggered when the window has been exposed and should be redrawn.</summary>
    public static event EventHandler<EventArgs>? OnWindowExposed;

    private static Sdl.Event _evt;

    /// <summary>Polls for pending events and dispatches them to the appropriate event handlers.</summary>
    public static void PollEvents()
    {
        while (Sdl.PollEvent(out _evt))
        {
            if (_evt.Type is EventType.Quit)
            {
                OnQuit?.Invoke(null, EventArgs.Empty);
            }

            if (_evt.Type is EventType.WindowExposed)
            {
                OnWindowExposed?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}
