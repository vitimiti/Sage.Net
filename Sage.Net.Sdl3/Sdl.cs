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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sage.Net.Sdl3;

internal static unsafe partial class Sdl
{
    public const uint InitVideo = 0x0020;

    private const uint MessageBoxError = 0x0010;

    public static string GetError => Utf8StringMarshaller.ConvertToManaged(GetErrorUnsafe()) ?? "Unknown Error";

    [LibraryImport("SDL3", EntryPoint = "SDL_Init")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I4)]
    public static partial bool Init(uint flags);

    [LibraryImport("SDL3", EntryPoint = "SDL_Quit")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static partial void Quit();

    [LibraryImport("SDL3", EntryPoint = "SDL_PollEvent")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I4)]
    public static partial bool PollEvent(out SdlEvent sdlEvent);

    public static bool ShowErrorDialog(string title, string message) =>
        ShowSimpleMessageBox(MessageBoxError, title, message, nint.Zero);

    [NativeMarshalling(typeof(SafeHandleMarshaller<Surface>))]
    public sealed class Surface : SafeHandle
    {
        private NativeSurface* UnsafeHandle => (NativeSurface*)handle;

        public Surface()
            : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

        public Surface(bool ownsHandle)
            : base(invalidHandleValue: nint.Zero, ownsHandle: ownsHandle) => SetHandle(nint.Zero);

        public static Surface FromBmp(string file) => LoadBmp(file);

        public static bool Blit(Surface src, Surface dst) => BlitSurface(src, null, dst, null);

        public override bool IsInvalid => handle == nint.Zero;

        public int Width => UnsafeHandle->W;

        public int Height => UnsafeHandle->H;

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return true;
            }

            DestroySurface(handle);
            SetHandle(nint.Zero);
            return true;
        }
    }

    [NativeMarshalling(typeof(SafeHandleMarshaller<Window>))]
    public sealed class Window : SafeHandle
    {
        public const ulong Borderless = 0x0010;

        public Window()
            : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

        public override bool IsInvalid => handle == nint.Zero;

        public Surface Surface
        {
            get
            {
                var surfaceHandle = GetWindowSurface(this);
                Surface surface = new(ownsHandle: false);
                if (surfaceHandle == nint.Zero)
                {
                    return surface;
                }

                Marshal.InitHandle(surface, surfaceHandle);
                return surface;
            }
        }

        public static Window Create(string title, int width, int height, ulong flags) =>
            CreateWindow(title, width, height, flags);

        public bool UpdateSurface() => UpdateWindowSurface(this);

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return true;
            }

            DestroyWindow(handle);
            SetHandle(nint.Zero);
            return true;
        }
    }

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
        public uint Type;

        [FieldOffset(0)]
        public fixed byte Padding[128];
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_GetError")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial byte* GetErrorUnsafe();

    [LibraryImport("SDL3", EntryPoint = "SDL_LoadBMP", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial Surface LoadBmp(string file);

    [LibraryImport("SDL3", EntryPoint = "SDL_BlitSurface")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I4)]
    private static partial bool BlitSurface(Surface src, Rect* srcRect, Surface dst, Rect* dstRect);

    [LibraryImport("SDL3", EntryPoint = "SDL_DestroySurface")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial void DestroySurface(nint window);

    [LibraryImport("SDL3", EntryPoint = "SDL_CreateWindow", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial Window CreateWindow(string title, int width, int height, ulong flags);

    [LibraryImport("SDL3", EntryPoint = "SDL_GetWindowSurface", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial nint GetWindowSurface(Window window);

    [LibraryImport("SDL3", EntryPoint = "SDL_UpdateWindowSurface")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I4)]
    private static partial bool UpdateWindowSurface(Window window);

    [LibraryImport("SDL3", EntryPoint = "SDL_DestroyWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial void DestroyWindow(nint window);

    [LibraryImport("SDL3", EntryPoint = "SDL_ShowSimpleMessageBox", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I4)]
    private static partial bool ShowSimpleMessageBox(uint flags, string title, string message, nint window);

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
}
