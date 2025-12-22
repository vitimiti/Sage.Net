// -----------------------------------------------------------------------
// <copyright file="IVideoDecoder.cs" company="Sage.Net">
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
/// Represents an interface for decoding video files, providing access to video frame dimensions,
/// frame rate, and methods for opening and decoding video content.
/// </summary>
public interface IVideoDecoder
{
    /// <summary>
    /// Gets the width of the video frames in pixels.
    /// </summary>
    /// <remarks>
    /// This property provides the horizontal resolution of the video frames being decoded.
    /// The value represents the number of pixels across the width of each frame.
    /// </remarks>
    int Width { get; }

    /// <summary>
    /// Gets the height of the video frames in pixels.
    /// </summary>
    /// <remarks>
    /// This property provides the vertical resolution of the video frames being decoded.
    /// The value represents the number of pixels along the height of each frame.
    /// </remarks>
    int Height { get; }

    /// <summary>
    /// Gets the frame rate of the video in frames per second (FPS).
    /// </summary>
    /// <remarks>
    /// This property represents the number of frames displayed per second in the video being decoded.
    /// It provides insight into the smoothness of video playback, with higher values indicating
    /// a smoother viewing experience.
    /// </remarks>
    double FrameRate { get; }

    /// <summary>
    /// Opens the specified video file for decoding.
    /// </summary>
    /// <param name="filePath">The path to the video file to be opened.</param>
    /// <returns>True if the video file is successfully opened; otherwise, false.</returns>
    bool Open(string filePath);

    /// <summary>
    /// Decodes the next frame of the video and retrieves the frame data along with associated audio data.
    /// </summary>
    /// <param name="rgbaBuffer">A buffer that receives the decoded video frame in RGBA format.</param>
    /// <param name="audioBufferHandle">A handle to the buffer containing the decoded audio data.</param>
    /// <param name="audioSize">The size of the decoded audio data in bytes.</param>
    /// <returns>True if the decoding of the next frame is successful; otherwise, false.</returns>
    bool DecodeNextFrame(out ReadOnlySpan<byte> rgbaBuffer, out nint audioBufferHandle, out int audioSize);
}
