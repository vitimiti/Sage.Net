// -----------------------------------------------------------------------
// <copyright file="IPooledObject.cs" company="Sage.Net">
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

namespace Sage.Net.GameEngine.Common;

/// <summary>An interface to indicate an object is poolable.</summary>
public interface IPooledObject
{
    /// <summary>What to do when the object is being acquired from the pool.</summary>
    void OnAcquire();

    /// <summary>What to do when the object is being released to the pool.</summary>
    void OnRelease();
}
