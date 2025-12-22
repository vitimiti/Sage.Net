// -----------------------------------------------------------------------
// <copyright file="SdlMethods.cs" company="Sage.Net">
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
    public static string CurrentError => Utf8StringMarshaller.ConvertToManaged(GetErrorUnsafe()) ?? "Unknown Error";

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

    [LibraryImport("SDL3", EntryPoint = "SDL_CreateGPUDevice", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial GpuDevice CreateGpuDevice(
        uint formatFlags,
        [MarshalAs(UnmanagedType.I4)] bool debugMode,
        string? name
    );

    [LibraryImport("SDL3", EntryPoint = "SDL_ClaimWindowForGPUDevice")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I4)]
    private static partial bool ClaimWindowForGpuDevice(GpuDevice device, Window window);

    [LibraryImport("SDL3", EntryPoint = "SDL_ReleaseWindowFromGPUDevice")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial void ReleaseWindowFromGpuDevice(GpuDevice device, Window window);

    [LibraryImport("SDL3", EntryPoint = "SDL_AcquireGPUCommandBuffer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial GpuCommandBuffer AcquireGpuCommandBuffer(GpuDevice device);

    [LibraryImport("SDL3", EntryPoint = "SDL_WaitAndAcquireGPUSwapchainTexture")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I4)]
    private static partial bool WaitAndAcquireGpuSwapchainTexture(
        GpuCommandBuffer commandBuffer,
        Window window,
        nint swapchainTexture,
        out uint swapchainTextureWidth,
        out uint swapchainTextureHeight
    );

    [LibraryImport("SDL3", EntryPoint = "SDL_BeginGPURenderPass")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial GpuRenderPass BeginGpuRenderPass(
        GpuCommandBuffer commandBuffer,
        GpuColorTargetInfo* colorTargetInfos,
        uint numColorTargets,
        GpuDepthStencilTargetInfo* depthStencilTargetInfo
    );

    [LibraryImport("SDL3", EntryPoint = "SDL_EndGPURenderPass")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial void EndGpuRenderPass(GpuRenderPass renderPass);

    [LibraryImport("SDL3", EntryPoint = "SDL_DestroyGPUDevice")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    private static partial void DestroyGpuDevice(nint device);

    [LibraryImport("SDL3", EntryPoint = "SDL_SubmitGPUCommandBuffer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    [return: MarshalAs(UnmanagedType.I4)]
    private static partial bool SubmitGpuCommandBuffer(GpuCommandBuffer commandBuffer);
}
