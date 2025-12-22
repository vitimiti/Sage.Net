// -----------------------------------------------------------------------
// <copyright file="IMediaPlayer.cs" company="Sage.Net">
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
/// Represents a media player interface that provides functionalities for controlling media playback
/// including play, pause, stop, and navigation within the media.
/// </summary>
public interface IMediaPlayer : IDisposable
{
    /// <summary>
    /// Gets or sets the volume level of the media playback.
    /// </summary>
    /// <remarks>
    /// The value is expected to be in the range of 0.0 (muted) to 1.0 (maximum volume).
    /// </remarks>
    float Volume { get; set; }

    /// <summary>
    /// Gets a value indicating whether the media is currently playing.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> if the media is actively playing; otherwise, <c>false</c>.
    /// </remarks>
    bool IsPlaying { get; }

    /// <summary>
    /// Gets or sets the current playback position within the media.
    /// </summary>
    /// <remarks>
    /// The value represents a time offset from the start of the media,
    /// and should typically be within the range of 0 to the media's total duration.
    /// </remarks>
    TimeSpan Position { get; set; }

    /// <summary>
    /// Gets the total duration of the media being played.
    /// </summary>
    /// <remarks>
    /// Represents the length of the media as a <see cref="TimeSpan"/>.
    /// This value is read-only and may return a value of zero if the duration is not known or unavailable.
    /// </remarks>
    TimeSpan Duration { get; }

    /// <summary>
    /// Begins playback of the media.
    /// If the media is paused, playback resumes from the current position.
    /// If the media is stopped, playback starts from the beginning.
    /// </summary>
    /// <remarks>
    /// This method transitions the media player's state to playing.
    /// If the media is already playing, calling this method has no effect.
    /// </remarks>
    void Play();

    /// <summary>
    /// Pauses the playback of the media.
    /// If the media is currently playing, playback will be halted and the current position will be retained.
    /// </summary>
    /// <remarks>
    /// This method transitions the media player's state to paused.
    /// If the media is already paused or stopped, calling this method has no effect.
    /// </remarks>
    void PausePlayback();

    /// <summary>
    /// Stops the playback of the media.
    /// If the media is currently playing or paused, playback will be halted and the position will reset to the beginning of the media.
    /// </summary>
    /// <remarks>
    /// This method transitions the media player's state to stopped.
    /// If the media is already stopped, calling this method has no effect.
    /// </remarks>
    void StopPlayback();

    /// <summary>
    /// Updates the current state of the media player.
    /// This method is typically called to perform time-based updates or handle media-related tasks.
    /// </summary>
    /// <param name="deltaTime">The elapsed time, in seconds, since the last update call.</param>
    void Update(double deltaTime);
}
