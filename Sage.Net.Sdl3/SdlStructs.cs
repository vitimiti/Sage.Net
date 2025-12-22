// -----------------------------------------------------------------------
// <copyright file="SdlStructs.cs" company="Sage.Net">
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

namespace Sage.Net.Sdl3;

internal static unsafe partial class Sdl
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int X;
        public int Y;
        public int W;
        public int H;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SdlEvent
    {
        [FieldOffset(0)]
        public SdlEventType Type;

        [FieldOffset(0)]
        public fixed byte Padding[128];
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSurface
    {
        public uint Flags;
        public uint Format;
        public int W;
        public int H;
        public int Pitch;
        public nint Pixels;
        public int RefCount;
        public nint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FColor
    {
        public float R;
        public float G;
        public float B;
        public float A;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GpuColorTargetInfo
    {
        public nint Texture;
        public uint MipLevel;
        public uint LayerOrDepthPlane;
        public FColor ClearColor;
        public GpuLoadOp LoadOp;
        public GpuStoreOp StoreOp;
        public nint ResolveTexture;
        public uint ResolveMipLevel;
        public uint ResolveLayer;
        public byte Cycle;
        public byte CycleResolveTexture;
        public byte Padding1;
        public byte Padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GpuDepthStencilTargetInfo
    {
        public nint Texture;
        public float ClearDepth;
        public GpuLoadOp LoadOp;
        public GpuStoreOp StoreOp;
        public GpuLoadOp StencilLoadOp;
        public GpuStoreOp StencilStoreOp;
        public byte Cycle;
        public byte ClearStencil;
        public byte Padding1;
        public byte Padding2;
    }
}
