// -----------------------------------------------------------------------
// <copyright file="GlobalData.cs" company="Sage.Net">
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

namespace Sage.Net.Core.GameEngine.Common.Subsystems;

/// <summary>
/// The global data.
/// </summary>
public class GlobalData : SubsystemBase
{
    /// <summary>
    /// Gets or sets the viewport height scale.
    /// </summary>
    public float ViewportHeightScale { get; set; }

    /// <summary>
    /// Gets or sets the custom BIG file extension.
    /// </summary>
    /// <remarks>This is to support modding.</remarks>
    public string? ModBigCustomExtension { get; set; }

    /// <summary>
    /// Gets or sets the custom big files.
    /// </summary>
    /// <remarks>This is to support modding.</remarks>
    public string? ModBig { get; set; }

    /// <summary>
    /// Gets or sets the mod directory.
    /// </summary>
    /// <remarks>This is to support modding.</remarks>
    public string? ModDir { get; set; }

    /// <inheritdoc/>
    /// <exception cref="NotImplementedException">This is not implemented.</exception>
    public override void Initialize() => throw new NotImplementedException();

    /// <inheritdoc/>
    /// <exception cref="NotImplementedException">This is not implemented.</exception>
    public override void Reset() => throw new NotImplementedException();

    /// <inheritdoc/>
    /// <exception cref="NotImplementedException">This is not implemented.</exception>
    public override void UpdateBase() => throw new NotImplementedException();

    /// <summary>
    /// Parses a custom definition for modifying the game's behavior based on external data.
    /// </summary>
    public void ParseCustomDefinition()
    {
        if (AddonCompat.HasFullViewportDataFile())
        {
            ViewportHeightScale = 1F;
        }
    }
}
