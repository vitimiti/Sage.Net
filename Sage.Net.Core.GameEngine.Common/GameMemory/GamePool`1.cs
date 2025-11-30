// -----------------------------------------------------------------------
// <copyright file="GamePool`1.cs" company="Sage.Net">
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

namespace Sage.Net.Core.GameEngine.Common.GameMemory;

/// <summary>
/// A GC friendly object pool.
/// </summary>
/// <typeparam name="T">The type of the object to pool.</typeparam>
public static class GamePool<T>
    where T : class, IPooledObject, new()
{
    private static readonly Stack<T> Items = new(128);

    /// <summary>
    /// Rents an object from the pool.
    /// </summary>
    /// <returns>Either the existing instance within the pool; or a newly allocated one if none exists.</returns>
    public static T Rent() => Items.Count > 0 ? Items.Pop() : new T();

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    /// <param name="item">The instance to return to the pool.</param>
    public static void Return(T? item)
    {
        if (item is null)
        {
            return;
        }

        item.Reset();
        Items.Push(item);
    }
}
