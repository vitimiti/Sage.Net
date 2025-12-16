// -----------------------------------------------------------------------
// <copyright file="SurfaceHandle.cs" company="Sage.Net">
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

/// <summary>Represents a two-dimensional surface with properties and behaviors relevant for graphical or geometrical computations.</summary>
[NativeMarshalling(typeof(SafeHandleMarshaller<Surface>))]
public class Surface : SafeHandle
{
    /// <summary>Gets a value indicating whether the handle value is invalid.</summary>
    public override bool IsInvalid => handle == nint.Zero;

    /// <summary>Initializes a new instance of the <see cref="Surface"/> class.</summary>
    public Surface()
        : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

    /// <summary>Loads a BMP image from the specified file path.</summary>
    /// <param name="filePath">The path of the BMP file to load.</param>
    /// <returns>A new <see cref="Surface"/> object representing the loaded BMP image.</returns>
    public static unsafe Surface LoadBmp(string filePath)
    {
        Sdl.Surface* ptr = Sdl.LoadBmp(filePath);
        var surface = new Surface();
        if (ptr is null)
        {
            return surface;
        }

        Marshal.InitHandle(surface, (nint)ptr);
        return surface;
    }

    /// <inheritdoc/>
    /// <summary>Releases the unmanaged resources used by the <see cref="Surface" /> and optionally releases the managed resources.</summary>
    protected override bool ReleaseHandle()
    {
        if (handle == nint.Zero)
        {
            return true;
        }

        SetHandle(nint.Zero);
        Sdl.DestroySurface(handle);
        return true;
    }
}
