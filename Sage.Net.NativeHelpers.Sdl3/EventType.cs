// -----------------------------------------------------------------------
// <copyright file="EventType.cs" company="Sage.Net">
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

namespace Sage.Net.NativeHelpers.Sdl3;

/// <summary>Defines the types of events that can occur within the SDL environment.</summary>
[SuppressMessage("Design", "CA1028: Enum storage should be Int32", Justification = "This is used in interop.")]
public enum EventType : uint
{
    /// <summary>Indicates that the first event is in the queue.</summary>
    First,

    /// <summary>Indicates that the application should quit.</summary>
    Quit = 0x0100,

    /// <summary>Indicates that the window has been exposed and should be redrawn.</summary>
    WindowExposed = 0x0204,
}
