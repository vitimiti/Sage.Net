// -----------------------------------------------------------------------
// <copyright file="Structs.cs" company="Sage.Net">
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
    [StructLayout(LayoutKind.Explicit)]
    public struct Event
    {
        [FieldOffset(0)]
        public EventType Type;

        [FieldOffset(0)]
        private unsafe fixed byte _padding[128];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int X;
        public int Y;
        public int W;
        public int H;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Surface
    {
        public SurfaceFlags Flags;
        public PixelFormat Format;
        public int W;
        public int H;
        public int Pitch;
        public unsafe void* Pixels;
        public int RefCount;
        public unsafe void* Reserved;
    }
}
