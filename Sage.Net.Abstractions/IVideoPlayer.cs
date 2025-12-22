// -----------------------------------------------------------------------
// <copyright file="IVideoPlayer.cs" company="Sage.Net">
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

namespace Sage.Net.Abstractions;

/// <summary>
/// Represents a video player interface that provides extended functionalities
/// for video playback, including access to video dimensions and the current frame data.
/// </summary>
public interface IVideoPlayer : IMediaPlayer
{
    /// <summary>
    /// Gets the width of the video in pixels.
    /// </summary>
    /// <remarks>
    /// This property represents the horizontal resolution of the video currently loaded in the video player.
    /// The value is read-only and reflects the intrinsic width of the video content.
    /// </remarks>
    int Width { get; }

    /// <summary>
    /// Gets the height of the video in pixels.
    /// </summary>
    /// <remarks>
    /// This property represents the vertical resolution of the video currently loaded in the video player.
    /// The value is read-only and reflects the intrinsic height of the video content.
    /// </remarks>
    int Height { get; }

    /// <summary>
    /// Retrieves a handle to the current frame being displayed by the video player.
    /// This handle can be used for operations such as rendering or frame analysis.
    /// </summary>
    /// <returns>
    /// A native integer handle representing the current video frame.
    /// The returned handle is platform-specific and should be managed accordingly
    /// to ensure proper resource handling and avoid memory leaks.
    /// </returns>
    nint GetCurrentFrameHandle();
}
