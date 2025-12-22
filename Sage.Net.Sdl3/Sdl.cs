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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sage.Net.Sdl3;

internal static unsafe partial class Sdl
{
    public const uint InitVideo = 0x0020;

    private const uint MessageBoxError = 0x0010;

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
        public const ulong Fullscreen = 0x0001;
        public const ulong Borderless = 0x0010;
        public const ulong Resizable = 0x0020;
        public const ulong MouseCapture = 0x4000;

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

    [NativeMarshalling(typeof(SafeHandleMarshaller<GpuDevice>))]
    public sealed class GpuDevice : SafeHandle
    {
        public const uint ShaderFormatSpirv = 1 << 1;

        public GpuDevice()
            : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

        public override bool IsInvalid => handle == nint.Zero;

        public static GpuDevice Create(uint formatFlags, bool debugMode, string? name) =>
            CreateGpuDevice(formatFlags, debugMode, name);

        public bool ClaimWindow(Window window) => ClaimWindowForGpuDevice(this, window);

        public void UnclaimWindow(Window window) => ReleaseWindowFromGpuDevice(this, window);

        public GpuCommandBuffer AcquireCommandBuffer() => AcquireGpuCommandBuffer(this);

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return true;
            }

            DestroyGpuDevice(handle);
            SetHandle(nint.Zero);
            SetHandle(nint.Zero);
            return true;
        }
    }

    [NativeMarshalling(typeof(SafeHandleMarshaller<GpuCommandBuffer>))]
    public sealed class GpuCommandBuffer : SafeHandle
    {
        public GpuCommandBuffer()
            : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

        public override bool IsInvalid => handle == nint.Zero;

        public bool WaitAndAcquireSwapchainTexture(
            Window window,
            out GpuTexture swapchainTexture,
            out uint swapchainTextureWidth,
            out uint swapchainTextureHeight
        )
        {
            var textureHandle = nint.Zero;
            var result = WaitAndAcquireGpuSwapchainTexture(
                this,
                window,
                (nint)(&textureHandle),
                out swapchainTextureWidth,
                out swapchainTextureHeight
            );

            swapchainTexture = new GpuTexture();
            if (result && textureHandle != nint.Zero)
            {
                Marshal.InitHandle(swapchainTexture, textureHandle);
            }

            return result;
        }

        [SuppressMessage(
            "csharpsquid",
            "S3869:\"SafeHandle.DangerousGetHandle\" should not be called",
            Justification = "This is unsafe code."
        )]
        public GpuRenderPass BeginRenderPass(GpuTexture swapchainTexture)
        {
            var refAdded = false;
            swapchainTexture.DangerousAddRef(ref refAdded);

            try
            {
                GpuColorTargetInfo colorTargetInfo;
                NativeMemory.Clear(&colorTargetInfo, (nuint)sizeof(GpuColorTargetInfo));

                colorTargetInfo.Texture = swapchainTexture.DangerousGetHandle();
                colorTargetInfo.ClearColor = new FColor
                {
                    R = 0,
                    G = 0,
                    B = 0,
                    A = 1,
                };
                colorTargetInfo.LoadOp = GpuLoadOp.Clear;
                colorTargetInfo.StoreOp = GpuStoreOp.Store;

                return BeginGpuRenderPass(this, &colorTargetInfo, 1, null);
            }
            finally
            {
                if (refAdded)
                {
                    swapchainTexture.DangerousRelease();
                }
            }
        }

        public bool Submit() => SubmitGpuCommandBuffer(this);

        protected override bool ReleaseHandle() => true;
    }

    [NativeMarshalling(typeof(SafeHandleMarshaller<GpuTexture>))]
    public sealed class GpuTexture : SafeHandle
    {
        public GpuTexture()
            : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

        public override bool IsInvalid => handle == nint.Zero;

        protected override bool ReleaseHandle() => true;
    }

    [NativeMarshalling(typeof(SafeHandleMarshaller<GpuRenderPass>))]
    public sealed class GpuRenderPass : SafeHandle
    {
        public GpuRenderPass()
            : base(invalidHandleValue: nint.Zero, ownsHandle: true) => SetHandle(nint.Zero);

        public override bool IsInvalid => handle == nint.Zero;

        public void End() => EndGpuRenderPass(this);

        protected override bool ReleaseHandle() => true;
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
        public SdlEventType Type;

        [FieldOffset(0)]
        public fixed byte Padding[128];
    }

    public enum SdlEventType
    {
        Quit = 0x0100,
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

    [StructLayout(LayoutKind.Explicit, Size = 64)]
    private struct GpuColorTargetInfo
    {
        [FieldOffset(0)]
        public nint Texture;

        [FieldOffset(8)]
        public uint MipLevel;

        [FieldOffset(12)]
        public uint LayerOrDepthPlane;

        [FieldOffset(16)]
        public FColor ClearColor;

        [FieldOffset(32)]
        public GpuLoadOp LoadOp;

        [FieldOffset(36)]
        public GpuStoreOp StoreOp;

        [FieldOffset(40)]
        public nint ResolveTexture;

        [FieldOffset(48)]
        public uint ResolveMipLevel;

        [FieldOffset(52)]
        public uint ResolveLayer;

        [FieldOffset(56)]
        public byte Cycle;

        [FieldOffset(57)]
        public byte CycleResolveTexture;

        [FieldOffset(58)]
        public byte Padding1;

        [FieldOffset(59)]
        public byte Padding2;
    }

    [StructLayout(LayoutKind.Explicit, Size = 48)]
    private struct GpuDepthStencilTargetInfo
    {
        [FieldOffset(0)]
        public nint Texture;

        [FieldOffset(8)]
        public float ClearDepth;

        [FieldOffset(12)]
        public GpuLoadOp LoadOp;

        [FieldOffset(16)]
        public GpuStoreOp StoreOp;

        [FieldOffset(20)]
        public GpuLoadOp StencilLoadOp;

        [FieldOffset(24)]
        public GpuStoreOp StencilStoreOp;

        [FieldOffset(28)]
        public byte Cycle;

        [FieldOffset(29)]
        public byte ClearStencil;

        [FieldOffset(30)]
        public byte Padding1;

        [FieldOffset(31)]
        public byte Padding2;
    }

    private enum GpuLoadOp
    {
        Clear = 1,
    }

    private enum GpuStoreOp
    {
        Store,
    }
}
