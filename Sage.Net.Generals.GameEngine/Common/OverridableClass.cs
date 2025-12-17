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

using Sage.Net.Core.GameEngine.Common;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Represents a class that can be overridden by other instances.</summary>
[MemoryPooled("Overridable", initialSize: 32, overflowSize: 32)]
public partial class OverridableClass : IPooledObject, IDisposable
{
    private bool _isOverride;
    private bool _disposed;

    /// <summary>Gets or sets the next override in the linked list.</summary>
    public OverridableClass? NextOverride { get; set; }

    /// <summary>Gets the final override in the linked list.</summary>
    public OverridableClass? FinalOverride => NextOverride is not null ? NextOverride.FinalOverride : this;

    /// <inheritdoc/>
    /// <remarks>This method resets the state of the object.</remarks>
    public void OnAcquire() => Reset();

    /// <inheritdoc/>
    /// <remarks>This method resets the state of the object.</remarks>
    public void OnRelease() => Reset();

    /// <summary>Marks this instance as an override in the chain.</summary>
    /// <remarks>When an instance is marked as an override, <see cref="DeleteOverrides"/> will delete it (and any subsequent overrides) and return <see langword="null"/> to unlink it from the chain.</remarks>
    public void MarkAsOverride() => _isOverride = true;

    /// <summary>Deletes overrides starting from this node according to its role in the chain.</summary>
    /// <returns><see langword="null"/> if this instance is an override (i.e. previously marked via <see cref="MarkAsOverride"/>), effectively removing it from the chain; otherwise returns <see langword="this"/> after updating <see cref="NextOverride"/> to exclude deleted nodes.</returns>
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

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting resources.</summary>
    /// <remarks>Calls <see cref="Dispose(bool)"/> with <see langword="true"/> and suppresses finalization.</remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases the unmanaged resources used by this instance and, optionally, releases the managed resources.</summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    /// <remarks>When <paramref name="disposing"/> is <see langword="true"/>, this method will also delete any linked overrides by invoking <see cref="NextOverride"/>.<see cref="Delete"/> if present, and mark the instance as disposed.</remarks>
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
