// -----------------------------------------------------------------------
// <copyright file="TerrainLighting.cs" company="Sage.Net">
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

using Sage.Net.Generals.Libraries.BaseTypes;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Represents the lighting configuration for terrain, including ambient and diffuse lighting, as well as the position of the light source.</summary>
/// <param name="Ambient">The ambient light color represented as an <see cref="RgbColor"/> instance.</param>
/// <param name="Diffuse">The diffuse light color represented as an <see cref="RgbColor"/> instance.</param>
/// <param name="LightPosition">The position of the light source represented as a <see cref="Coord3D"/> instance.</param>
public record TerrainLighting(RgbColor Ambient, RgbColor Diffuse, Coord3D LightPosition);
