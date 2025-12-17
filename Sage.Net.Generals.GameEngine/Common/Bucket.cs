// -----------------------------------------------------------------------
// <copyright file="Bucket.cs" company="Sage.Net">
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

using Sage.Net.Core.GameEngine.Common;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Represents a memory-pooled object used in the game engine. The <c>Bucket</c> class is designed to be reused through a pooling mechanism to enhance performance by reducing object allocation overhead.</summary>
/// <remarks>This class is decorated with the <see cref="MemoryPooledAttribute"/> to define its pooling behavior. When acquired from the pool, the <see cref="OnAcquire"/> method is invoked to reset state, and when released back, the <see cref="OnRelease"/> method ensures that the object is reset to its default state.</remarks>
/// <seealso cref="IPooledObject"/>
/// <seealso cref="MemoryPooledAttribute"/>
[MemoryPooled("NameKeyBucketPool", initialSize: 4096, overflowSize: 32)]
public partial class Bucket : IPooledObject
{
    /// <summary>Gets or sets the next bucket in the socket.</summary>
    public Bucket? NextInSocket { get; set; }

    /// <summary>Gets or sets the key.</summary>
    public NameKeyType Key { get; set; } = NameKeyType.Invalid;

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public void OnAcquire() => Reset();

    /// <inheritdoc/>
    public void OnRelease() => Reset();

    private void Reset()
    {
        NextInSocket = null;
        Key = NameKeyType.Invalid;
        Name = string.Empty;
    }
}
