// -----------------------------------------------------------------------
// <copyright file="WindowHandle.cs" company="Sage.Net">
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
using System.Runtime.InteropServices.Marshalling;
using Sage.Net.NativeHelpers.Sdl3.NativeImports;

namespace Sage.Net.NativeHelpers.Sdl3;

/// <summary>Represents a window that can be used to draw graphics.</summary>
[NativeMarshalling(typeof(SafeHandleMarshaller<WindowHandle>))]
public class WindowHandle : SafeHandle
{
    /// <summary>Initializes a new instance of the <see cref="WindowHandle"/> class.</summary>
    public WindowHandle()
        : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

    /// <summary>Gets a value indicating whether the current handle is invalid.</summary>
    public override bool IsInvalid => handle == nint.Zero;

    /// <summary>Gets the window surface.</summary>
    public unsafe SurfaceHandle Surface => new(Sdl.GetWindowSurface(this), ownsHandle: false);

    /// <summary>Creates a new splash window.</summary>
    /// <param name="windowTitle">The window title.</param>
    /// <param name="w">The width of the window, in pixels.</param>
    /// <param name="h">The height of the window, in pixels.</param>
    /// <returns>A new <see cref="WindowHandle"/> object representing the created window.</returns>
    public static WindowHandle CreateSplashWindow(string windowTitle, int w, int h) =>
        Sdl.CreateWindow($"{windowTitle} (Loading...)", w, h, Sdl.WindowFlags.Hidden | Sdl.WindowFlags.Borderless);

    /// <summary>Attempts to display the window associated with this instance.</summary>
    /// <returns>A boolean value indicating whether the window was successfully shown.</returns>
    public bool TryShow() => Sdl.ShowWindow(this);

    /// <summary>Attempts to update the window surface associated with this instance.</summary>
    /// <returns>A boolean value indicating whether the surface was successfully updated.</returns>
    public bool TryUpdateSurface() => Sdl.UpdateWindowSurface(this);

    /// <summary>Attempts to set the size of the window associated with this instance.</summary>
    /// <param name="w">The width of the window, in pixels.</param>
    /// <param name="h">The height of the window, in pixels.</param>
    /// <returns>A boolean value indicating whether the size was successfully set.</returns>
    public bool TrySetSize(int w, int h) => Sdl.SetWindowSize(this, w, h);

    /// <inheritdoc/>
    /// <summary>Releases the unmanaged resources used by the <see cref="WindowHandle" /> and optionally releases the managed resources.</summary>
    protected override bool ReleaseHandle()
    {
        if (handle == nint.Zero)
        {
            return true;
        }

        Sdl.DestroyWindow(handle);
        SetHandle(nint.Zero);
        return true;
    }
}
