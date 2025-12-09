// -----------------------------------------------------------------------
// <copyright file="StaticNameKey.cs" company="Sage.Net">
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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sage.Net.GameEngine.Common;

/// <summary>A static name key to manage <see cref="NameKeyType"/>s statically.</summary>
/// <param name="name">A <see cref="string"/> with the name of the <see cref="NameKeyType"/> to manage.</param>
public class StaticNameKey(string name)
{
    /// <summary>Gets the name of the <see cref="NameKeyType"/>.</summary>
    public string Name => name;

    /// <summary>Gets or sets the <see cref="NameKeyType"/> to manage.</summary>
    [SuppressMessage(
        "Maintainability",
        "CA1508:Avoid dead conditional code",
        Justification = "It is only dead conditional code if on Debug configuration."
    )]
    [SuppressMessage(
        "StyleCop.CSharp.LayoutRules",
        "SA1513:Closing brace should be followed by blank line",
        Justification = "This is a false positive."
    )]
    public NameKeyType Key
    {
        get
        {
            if (field != NameKeyType.Invalid)
            {
                return field;
            }

            Debug.Assert(
                NameKeyGenerator.TheNameKeyGenerator is not null,
                $"No {nameof(NameKeyGenerator.TheNameKeyGenerator)} yet."
            );

            if (NameKeyGenerator.TheNameKeyGenerator is not null)
            {
                field = NameKeyGenerator.TheNameKeyGenerator.NameToKey(Name);
            }

            return field;
        }
        set;
    } = NameKeyType.Invalid;
}
