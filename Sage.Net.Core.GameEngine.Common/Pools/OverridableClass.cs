// -----------------------------------------------------------------------
// <copyright file="Overridable.cs" company="Sage.Net">
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

using Sage.Net.Core.GameEngine.Common.GameMemory;

namespace Sage.Net.Core.GameEngine.Common.Pools;

/// <summary>
/// Represents an object that can participate in an override chain and be reused via a pool.
/// </summary>
/// <remarks>
/// Instances can form a singly linked list of overrides using <see cref="NextOverride"/>.
/// The deepest override in the chain can be obtained via <see cref="FinalOverride"/>.
/// The class integrates with <see cref="GameMemory.GamePool{Overridable}"/> to return instances
/// to a pool when disposed.
/// </remarks>
public class OverridableClass : IPooledObject, IDisposable
{
    private bool _isOverride;
    private bool _disposed;

    /// <summary>
    /// Gets or sets the next override in the chain.
    /// </summary>
    public OverridableClass? NextOverride { get; set; }

    /// <summary>
    /// Gets the final override in the chain (the deepest non-null successor),
    /// or the current instance when there are no overrides.
    /// </summary>
    public OverridableClass? FinalOverride => NextOverride is not null ? NextOverride.FinalOverride : this;

    /// <summary>
    /// Resets this instance to its initial state so it can be returned to and reused from the pool.
    /// </summary>
    public void Reset()
    {
        _isOverride = false;
        NextOverride = null;
    }

    /// <summary>
    /// Marks this instance as an override node in the chain.
    /// </summary>
    public void MarkAsOverride() => _isOverride = true;

    /// <summary>
    /// Deletes override nodes in the chain according to the override semantics.
    /// </summary>
    /// <returns>
    /// Returns <see langword="null"/> if this node is an override (it will be disposed),
    /// otherwise returns <see cref="this"/> with its <see cref="NextOverride"/> updated to
    /// reflect removed overrides.
    /// </returns>
    public OverridableClass? DeleteOverrides()
    {
        if (_isOverride)
        {
            // We must Dispose (which handles the recursion) to ensure
            // the children are also returned to the pool, matching C++ deleteInstance(this) behavior.
            Dispose();
            return null;
        }

        NextOverride = NextOverride?.DeleteOverrides();
        return this;
    }

    /// <summary>
    /// Releases this instance and its override chain, returning them to the pool.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Core dispose pattern implementation.
    /// </summary>
    /// <param name="disposing">
    /// When <see langword="true"/>, managed resources (the override chain) are disposed and
    /// the instance is returned to the pool.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            NextOverride?.Dispose();
            NextOverride = null;

            GamePool<OverridableClass>.Return(this);
        }

        _disposed = true;
    }
}
