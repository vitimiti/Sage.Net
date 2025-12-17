// -----------------------------------------------------------------------
// <copyright file="Rectangle.cs" company="Sage.Net">
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

using System.Runtime.InteropServices.Marshalling;
using Sage.Net.NativeHelpers.Sdl3.CustomMarshallers;

namespace Sage.Net.NativeHelpers.Sdl3;

/// <summary>Represents a rectangle with integer coordinates.</summary>
/// <param name="X">The x-coordinate of the upper-left corner of the rectangle.</param>
/// <param name="Y">The y-coordinate of the upper-left corner of the rectangle.</param>
/// <param name="Width">The width of the rectangle.</param>
/// <param name="Height">The height of the rectangle.</param>
[NativeMarshalling(typeof(RectangleMarshaller))]
public record Rectangle(int X, int Y, int Width, int Height);
