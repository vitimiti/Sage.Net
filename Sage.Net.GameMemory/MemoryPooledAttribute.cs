// -----------------------------------------------------------------------
// <copyright file="MemoryPooledAttribute.cs" company="Sage.Net">
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

namespace Sage.Net.GameMemory;

/// <summary>An attribute to indicate how a memory pooled object will be generated.</summary>
/// <param name="poolName">A <see cref="string"/> with the pool name.</param>
/// <param name="initialSize">An optional <see cref="int"/> with the pool's initial size. It defaults to <c>64</c>.</param>
/// <param name="overflowSize">An optional <see cref="int"/> with the pool's overflow size. It defaults to <c>16</c>.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class MemoryPooledAttribute(string poolName, int initialSize = 64, int overflowSize = 16) : Attribute
{
    /// <summary>Gets the pool name.</summary>
    public string PoolName { get; } = poolName;

    /// <summary>Gets the initial pool size.</summary>
    public int InitialSize { get; } = initialSize;

    /// <summary>Gets the overflow size.</summary>
    public int OverflowSize { get; } = overflowSize;
}
