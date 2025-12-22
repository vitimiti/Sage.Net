// -----------------------------------------------------------------------
// <copyright file="SdlSafeHandles.cs" company="Sage.Net">
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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sage.Net.Sdl3;

internal static unsafe partial class Sdl
{
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
}
