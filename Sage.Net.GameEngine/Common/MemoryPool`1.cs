// -----------------------------------------------------------------------
// <copyright file="MemoryPool`1.cs" company="Sage.Net">
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

/// <summary>A class to manage memory pools.</summary>
/// <typeparam name="T">The type of the object that is being pooled.</typeparam>
public class MemoryPool<T>
    where T : class, new()
{
    private readonly Stack<T> _freeObjects;
    private readonly int _overflowCount;
    private readonly Lock _lock = new();

    private int _totalAllocated;

    /// <summary>Initializes a new instance of the <see cref="MemoryPool{T}"/> class.</summary>
    /// <param name="name">A <see cref="string"/> with the name of the pool.</param>
    /// <param name="initialCount">An <see cref="int"/> with the initial count of objects in the pool.</param>
    /// <param name="overflowCount">An <see cref="int"/> with the overflow count of objects for the pool.</param>
    public MemoryPool(string name, int initialCount, int overflowCount)
    {
        Name = name;
        _overflowCount = overflowCount;
        _freeObjects = new Stack<T>(initialCount);

        ExpandPool(initialCount);
    }

    /// <summary>Gets the pool name.</summary>
    public string Name { get; }

    /// <summary>Gets the stats of the current <see cref="MemoryPool{T}"/> state.</summary>
    public string Stats
    {
        get
        {
            lock (_lock)
            {
                return $"Pool: {Name} | Total: {_totalAllocated} | Free: {_freeObjects.Count} | Used: {_totalAllocated - _freeObjects.Count}";
            }
        }
    }

    /// <summary>Allocates a new object from the pool.</summary>
    /// <returns>An existing instance of the given <typeparamref name="T"/> object, or an existing one if there is any left in the pool.</returns>
    /// <remarks>This will call <see cref="IPooledObject.OnAcquire"/> if the <typeparamref name="T"/> object is a <see cref="IPooledObject"/>.</remarks>
    public T Allocate()
    {
        lock (_lock)
        {
            if (_freeObjects.Count == 0)
            {
                ExpandPool(_overflowCount > 0 ? _overflowCount : 16);
            }

            T item = _freeObjects.Pop();
            if (item is IPooledObject pooled)
            {
                pooled.OnAcquire();
            }

            return item;
        }
    }

    /// <summary>Frees an existing object back into the pool.</summary>
    /// <param name="item">The <typeparamref name="T"/> object to free.</param>
    /// <remarks>This will call <see cref="IPooledObject.OnRelease"/> if the <typeparamref name="T"/> object is a <see cref="IPooledObject"/>. This simply returns the object to the pool for later reuse.</remarks>
    public void Free(T? item)
    {
        switch (item)
        {
            case null:
                return;
            case IPooledObject pooled:
                pooled.OnRelease();
                break;
            default:
                break;
        }

        lock (_lock)
        {
            _freeObjects.Push(item);
        }
    }

    private void ExpandPool(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _freeObjects.Push(new T());
        }

        _totalAllocated += count;
    }
}
