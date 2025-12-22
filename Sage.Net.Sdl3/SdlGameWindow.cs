// -----------------------------------------------------------------------
// <copyright file="SdlGameWindow.cs" company="Sage.Net">
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sage.Net.Abstractions;

namespace Sage.Net.Sdl3;

/// <summary>
/// Represents a game window implemented using the SDL library.
/// </summary>
/// <remarks>
/// Provides functionality for initialization, updating, drawing, and disposal of the game window.
/// Implements the <see cref="IGameWindow"/> interface.
/// </remarks>
public sealed partial class SdlGameWindow : IGameWindow
{
    private readonly ILogger<SdlGameWindow> _logger;
    private readonly GameOptions _gameOptions;

    private Sdl.GpuDevice? _gpuDevice;
    private Sdl.Window? _window;
    private Sdl.GpuCommandBuffer? _commandBuffer;
    private Sdl.GpuTexture? _swapchainTexture;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SdlGameWindow"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="gameOptions">The game options.</param>
    public SdlGameWindow(ILogger<SdlGameWindow> logger, IOptions<GameOptions> gameOptions)
    {
        ArgumentNullException.ThrowIfNull(gameOptions);

        _logger = logger;
        _gameOptions = gameOptions.Value;
    }

    /// <inheritdoc/>
    public bool QuitRequested { get; private set; }

    /// <summary>
    /// Initializes the <see cref="SdlGameWindow"/> instance by configuring the GPU device,
    /// creating and claiming a window, and applying necessary settings based on the game options.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the GPU device fails to create, the window fails to create,
    /// or the GPU device fails to claim the window.
    /// </exception>
    public void Initialize()
    {
#if DEBUG
        const bool gpuDeviceDebugMode = true;
#else
        const bool gpuDeviceDebugMode = false;
#endif

        _gpuDevice = Sdl.GpuDevice.Create(Sdl.GpuDevice.ShaderFormatSpirv, gpuDeviceDebugMode, null);
        if (_gpuDevice.IsInvalid)
        {
            throw new InvalidOperationException($"Failed to create GPU device ({Sdl.CurrentError}).");
        }

        Log.CreatedGpuDevice(_logger);
        var flags = Sdl.Window.MouseCapture | Sdl.Window.Resizable;
        if (!_gameOptions.Windowed)
        {
            flags |= Sdl.Window.Fullscreen;
        }

#if DEBUG
        var windowTitle = $"{_gameOptions.WindowTitle} - Debug";
#else
        var windowTitle = _gameOptions.WindowTitle;
#endif
        _window = Sdl.Window.Create(windowTitle, _gameOptions.XResolution, _gameOptions.YResolution, flags);
        if (_window.IsInvalid)
        {
            throw new InvalidOperationException($"Failed to create window ({Sdl.CurrentError}).");
        }

        Log.CreatedWindow(_logger);
        if (!_gpuDevice.ClaimWindow(_window))
        {
            throw new InvalidOperationException($"Failed to claim window for GPU device ({Sdl.CurrentError}).");
        }

        Log.ClaimedWindow(_logger);
    }

    /// <summary>
    /// Processes SDL events and updates the state of the game window.
    /// </summary>
    /// <remarks>
    /// Handles window quit requests and prepares the rendering command buffer and swapchain texture for drawing operations.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when acquiring a command buffer or swapchain texture fails.
    /// </exception>
    public void Update()
    {
        while (Sdl.PollEvent(out Sdl.SdlEvent sdlEvent))
        {
            if (sdlEvent.Type is Sdl.SdlEventType.Quit)
            {
                QuitRequested = true;
            }
        }

        if (_gpuDevice?.IsInvalid == true || _window?.IsInvalid == true)
        {
            return;
        }

        _commandBuffer = _gpuDevice!.AcquireCommandBuffer();
        if (_commandBuffer.IsInvalid)
        {
            throw new InvalidOperationException($"Failed to acquire command buffer ({Sdl.CurrentError}).");
        }

        if (!_commandBuffer.WaitAndAcquireSwapchainTexture(_window!, out _swapchainTexture, out _, out _))
        {
            throw new InvalidOperationException($"Failed to acquire swapchain texture ({Sdl.CurrentError}).");
        }
    }

    /// <summary>
    /// Executes the rendering process for the game window.
    /// </summary>
    /// <remarks>
    /// This method performs rendering by initiating a render pass on the swapchain texture
    /// and submitting the associated command buffer. If the swapchain texture or command
    /// buffer is invalid, the rendering process is skipped.
    /// </remarks>
    public void Draw()
    {
        if (
            _swapchainTexture == null
            || _commandBuffer == null
            || _swapchainTexture.IsInvalid
            || _commandBuffer.IsInvalid
        )
        {
            return;
        }

        using Sdl.GpuRenderPass renderPass = _commandBuffer.BeginRenderPass(_swapchainTexture);
        if (!renderPass.IsInvalid)
        {
            renderPass.End();
        }

        _ = _commandBuffer.Submit();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _commandBuffer?.Dispose();

        if (_gpuDevice is { IsInvalid: false } && _window is { IsInvalid: false })
        {
            _gpuDevice.UnclaimWindow(_window);
        }

        _window?.Dispose();
        _gpuDevice?.Dispose();

        Sdl.Quit();

        _disposed = true;
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Debug, Message = "Created GPU device.")]
        public static partial void CreatedGpuDevice(ILogger logger);

        [LoggerMessage(LogLevel.Debug, Message = "Created window.")]
        public static partial void CreatedWindow(ILogger logger);

        [LoggerMessage(LogLevel.Debug, "Claimed window for GPU device.")]
        public static partial void ClaimedWindow(ILogger logger);
    }
}
