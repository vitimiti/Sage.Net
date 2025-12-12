// -----------------------------------------------------------------------
// <copyright file="OverridableClass.cs" company="Sage.Net">
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

/// <summary>A class that represents a linked list of overridable values.</summary>
[MemoryPooled("Overridable", initialSize: 32, overflowSize: 32)]
public partial class OverridableClass : IDisposable, IPooledObject
{
    private bool _isOverride;
    private bool _disposed;

    /// <summary>Gets or sets the next <see cref="OverridableClass"/> in the linked list.</summary>
    public OverridableClass? NextOverride { get; set; }

    /// <summary>Gets the final override value.</summary>
    public OverridableClass FinalOverride => NextOverride is not null ? NextOverride.FinalOverride : this;

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    /// <remarks>This resets all the properties in the <see cref="OverridableClass"/>.</remarks>
    public void OnAcquire() => Reset();

    /// <inheritdoc/>
    /// <remarks>This resets all the properties in the <see cref="OverridableClass"/>.</remarks>
    public void OnRelease() => Reset();

    /// <summary>Marks this instance as an override.</summary>
    public void MarkAsOverride() => _isOverride = true;

    /// <summary>Deletes all overrides in the linked list.</summary>
    /// <returns>The final <see cref="OverridableClass"/> in the linked list.</returns>
    public OverridableClass? DeleteOverrides()
    {
        if (_isOverride)
        {
            Delete();
            return null;
        }

        NextOverride = NextOverride?.DeleteOverrides();
        return this;
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    /// <param name="disposing"><see langword="true"/> to dispose managed objects; <see langword="false"/> otherwise.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            NextOverride?.Delete();
        }

        _disposed = true;
    }

    private void Reset()
    {
        NextOverride = null;
        _isOverride = false;
    }
}
