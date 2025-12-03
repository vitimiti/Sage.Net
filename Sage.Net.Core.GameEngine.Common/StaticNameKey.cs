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

namespace Sage.Net.Core.GameEngine.Common;

/// <summary>
/// A static name key.
/// </summary>
/// <param name="name">The name of the static key.</param>
public class StaticNameKey(string name)
{
    /// <summary>
    /// Explicitly converts a <see cref="StaticNameKey"/> to a <see cref="NameKeyType"/>.
    /// </summary>
    /// <param name="key">The <see cref="StaticNameKey"/> to convert.</param>
    /// <returns>A new <see cref="NameKeyType"/> corresponding to the <see cref="StaticNameKey"/>.</returns>
    public static explicit operator NameKeyType([NotNull] StaticNameKey key) => key.ToNameKeyType();

    /// <summary>
    /// Gets the name of the static key.
    /// </summary>
    [SuppressMessage(
        "StyleCop.CSharp.LayoutRules",
        "SA1513:Closing brace should be followed by blank line",
        Justification = "This is correct style."
    )]
    public NameKeyType Key
    {
        [SuppressMessage(
            "Maintainability",
            "CA1508:Avoid dead conditional code",
            Justification = "This is only dead code in DEBUG."
        )]
        [SuppressMessage(
            "ReSharper",
            "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract",
            Justification = "This is only dead code in DEBUG."
        )]
        get
        {
            if (field != NameKeyType.Invalid)
            {
                return field;
            }

            Debug.Assert(
                GlobalSingletons.TheNameKeyGenerator is not null,
                $"No {nameof(GlobalSingletons.TheNameKeyGenerator)} yet."
            );

            if (GlobalSingletons.TheNameKeyGenerator is not null)
            {
                field = GlobalSingletons.TheNameKeyGenerator.NameToKey(name);
            }

            return field;
        }
    } = NameKeyType.Invalid;

    /// <summary>
    /// Converts the static key to a <see cref="NameKeyType"/>.
    /// </summary>
    /// <returns>A new <see cref="NameKeyType"/> corresponding to the current <see cref="StaticNameKey"/>.</returns>
    public NameKeyType ToNameKeyType() => Key;
}
