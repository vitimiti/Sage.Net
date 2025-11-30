// -----------------------------------------------------------------------
// <copyright file="XferOptions.cs" company="Sage.Net">
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

namespace Sage.Net.Core.GameEngine.Common.Transfer;

/// <summary>
/// Transfer options.
/// </summary>
[Flags]
[SuppressMessage(
    "Design",
    "CA1028:Enum Storage should be Int32",
    Justification = "Required due to maximum integer allowed being 0xFFFF_FFFF."
)]
[SuppressMessage(
    "Usage",
    "CA2217:Do not mark enums with FlagsAttribute",
    Justification = "Required for runtime options support."
)]
[SuppressMessage(
    "csharpsquid",
    "S4070: Non-flags enums should not be marked with \"FlagsAttribute\"",
    Justification = "Required for runtime options support."
)]
public enum XferOptions : uint
{
    /// <summary>
    /// No options.
    /// </summary>
    None = 0,

    /// <summary>
    /// Processing flag.
    /// </summary>
    Processing = 1 << 0,

    /// <summary>
    /// All options.
    /// </summary>
    All = 0xFFFF_FFFF,
}
