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

namespace Sage.Net.GameEngine.Common;

/// <summary>A bucket entry for the <see cref="NameKeyGenerator"/>.</summary>
/// <remarks>This is a pooled object.</remarks>
#if RTS_ZERO_HOUR
[MemoryPooled("NameKeyBucketPool", initialSize: 9_000, overflowSize: 1_024)]
#else
[MemoryPooled("NameKeyBucketPool", initialSize: 4_096, overflowSize: 32)]
#endif
public partial class Bucket : IPooledObject
{
    /// <summary>Gets or sets the next <see cref="Bucket"/> in the socket.</summary>
    public Bucket? NextInSocket { get; set; }

    /// <summary>Gets or sets the <see cref="Bucket"/>'s <see cref="NameKeyType"/>.</summary>
    public NameKeyType Key { get; set; } = NameKeyType.Invalid;

    /// <summary>Gets or sets the <see cref="Bucket"/>'s name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    /// <remarks>This resets all the properties in the <see cref="Bucket"/>.</remarks>
    public void OnAcquire() => Reset();

    /// <inheritdoc/>
    /// <remarks>This resets all the properties in the <see cref="Bucket"/>.</remarks>
    public void OnRelease() => Reset();

    private void Reset()
    {
        NextInSocket = null;
        Key = NameKeyType.Invalid;
        Name = string.Empty;
    }
}
