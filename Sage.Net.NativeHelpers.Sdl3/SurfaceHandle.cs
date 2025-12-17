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
[NativeMarshalling(typeof(SafeHandleMarshaller<SurfaceHandle>))]
public class SurfaceHandle : SafeHandle
{
    /// <summary>Initializes a new instance of the <see cref="SurfaceHandle"/> class.</summary>
    public SurfaceHandle()
        : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

    internal unsafe SurfaceHandle(Sdl.Surface* ptr, bool ownsHandle = true)
        : base(invalidHandleValue: nint.Zero, ownsHandle) => Marshal.InitHandle(this, (nint)ptr);

    /// <summary>Gets a value indicating whether the handle value is invalid.</summary>
    public override bool IsInvalid => handle == nint.Zero;

    /// <summary>Gets the width of the surface, in pixels.</summary>
    public unsafe int Width => UnsafeHandle->W;

    /// <summary>Gets the height of the surface, in pixels.</summary>
    public unsafe int Height => UnsafeHandle->H;

    private unsafe Sdl.Surface* UnsafeHandle => (Sdl.Surface*)handle;

    /// <summary>Loads a BMP image from the specified file path.</summary>
    /// <param name="filePath">The path of the BMP file to load.</param>
    /// <returns>A new <see cref="SurfaceHandle"/> object representing the loaded BMP image.</returns>
    public static unsafe SurfaceHandle LoadBmp(string filePath)
    {
        Sdl.Surface* ptr = Sdl.LoadBmp(filePath);
        return ptr is null ? new SurfaceHandle() : new SurfaceHandle(ptr);
    }

    /// <summary>Copies a portion of one surface to another.</summary>
    /// <param name="src">The source surface.</param>
    /// <param name="srcRect">The source rectangle.</param>
    /// <param name="dst">The destination surface.</param>
    /// <param name="dstRect">The destination rectangle.</param>
    /// <returns>A value indicating whether the blit operation was successful.</returns>
    public static unsafe bool TryBlit(SurfaceHandle? src, Rectangle? srcRect, SurfaceHandle? dst, Rectangle? dstRect) =>
        Sdl.BlitSurface(src is null ? null : src.UnsafeHandle, srcRect, dst is null ? null : dst.UnsafeHandle, dstRect);

    /// <inheritdoc/>
    /// <summary>Releases the unmanaged resources used by the <see cref="SurfaceHandle" /> and optionally releases the managed resources.</summary>
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
