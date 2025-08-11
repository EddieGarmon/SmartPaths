using System.Collections;

namespace SmartPaths;

/// <summary>A collection of weak references to objects. Weak references are purged by iteration/count
///     operations, not by add/remove operations.</summary>
/// <typeparam name="T">The type of object to hold weak references to.</typeparam>
/// <remarks>
///     <para>Since the collection holds weak references to the actual objects, the collection consists
///         of both living and dead references. Living references refer to objects that have not been
///         garbage collected, and may be used as normal references. Dead references refer to objects
///         that have been garbage collected.</para>
///     <para>Dead references do consume resources; each dead reference is a garbage collection handle.</para>
///     <para>Dead references may be cleaned up by a <see cref="Purge" /> operation. Some properties
///         and methods cause a purge as a side effect; the member documentation specifies whether a
///         purge takes place.</para>
/// </remarks>
internal sealed class WeakCollection<T> : IWeakCollection<T?>
    where T : class
{

    /// <summary>The actual collection of strongly-typed weak references.</summary>
    private readonly List<WeakReference<T>> _list = [];

    /// <summary>Gets the number of live and dead entries in the collection, without causing a purge. O(1).</summary>
    public int Count => _list.Count;

    /// <summary>Gets a sequence of live objects from the collection, causing a purge.</summary>
    public IEnumerable<T> LiveList {
        get {
            List<T> ret = new(_list.Count);
            ret.AddRange(UnsafeLiveList);
            return ret;
        }
    }

    /// <summary>Gets a value indicating whether the collection is read only. Always returns false.</summary>
    bool ICollection<T?>.IsReadOnly => false;

    /// <summary>Gets a sequence of live objects from the collection, causing a purge. The entire sequence
    ///     MUST always be enumerated!</summary>
    private IEnumerable<T> UnsafeLiveList {
        get {
            // This implementation uses logic similar to List<T>.RemoveAll, which always has O(n) time.
            //  Some other implementations seen in the wild have O(n*m) time, where m is the number of dead entries.
            //  As m approaches n (e.g., mass object extinctions), their running time approaches O(n^2).
            int writeIndex = 0;
            for (int readIndex = 0; readIndex != _list.Count; ++readIndex) {
                WeakReference<T> weakReference = _list[readIndex];
                if (weakReference.TryGetTarget(out T? target)) {
                    yield return target;

                    if (readIndex != writeIndex) {
                        _list[writeIndex] = _list[readIndex];
                    }

                    ++writeIndex;
                } else {
                    weakReference.SetTarget(null!);
                }
            }

            _list.RemoveRange(writeIndex, _list.Count - writeIndex);
        }
    }

    /// <summary>Adds a weak reference to an object to the collection. Does not cause a purge.</summary>
    /// <param name="item">The object to add a weak reference to.</param>
    public void Add(T? item) {
        if (item == null) {
            return;
        }
        _list.Add(new WeakReference<T>(item));
    }

    /// <summary>Empties the collection.</summary>
    public void Clear() {
        foreach (WeakReference<T> weakReference in _list) {
            weakReference.SetTarget(null!);
        }
        _list.Clear();
    }

    /// <summary>Frees all resources held by the collection.</summary>
    public void Dispose() {
        Clear();
    }

    /// <summary>Gets a sequence of live and dead objects from the collection. Does not cause a purge.</summary>
    /// <returns>The sequence of live and dead objects.</returns>
    public IEnumerator<T?> GetEnumerator() {
        return _list.Select(x => x.TryGetTarget(out T? target) ? target : null).GetEnumerator();
    }

    /// <summary>Removes all dead objects from the collection.</summary>
    public void Purge() {
        // We cannot simply use List<T>.RemoveAll, because the predicate "x => !x.IsAlive" is not stable.
        foreach (T? _ in UnsafeLiveList) {
            //do nothing
        }
    }

    /// <summary>Removes a weak reference to an object from the collection. Does not cause a purge.</summary>
    /// <param name="item">The object to remove a weak reference to.</param>
    /// <returns>True if the object was found and removed; false if the object was not found.</returns>
    public bool Remove(T? item) {
        if (item == null) {
            return false;
        }
        for (int i = 0; i != _list.Count; ++i) {
            WeakReference<T> weakReference = _list[i];
            if (weakReference.TryGetTarget(out T? target)) {
                if (target == item) {
                    _list.RemoveAt(i);
                    weakReference.SetTarget(null!);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString() {
        return "WeakCollection<" + typeof(T).Name + "> (" + Count + ")";
    }

    /// <summary>Determines whether the collection contains a specific value. Does not cause a purge.</summary>
    /// <param name="item">The object to locate.</param>
    /// <returns>True if the collection contains a specific value; false if it does not.</returns>
    bool ICollection<T?>.Contains(T? item) {
        return this.Where(x => x != null).Contains(item);
    }

    /// <summary>Copies all live and dead objects to an array. Does not cause a purge.</summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The index to begin writing into the array.</param>
    void ICollection<T?>.CopyTo(T?[] array, int arrayIndex) {
        ArgumentNullException.ThrowIfNull(array);
        if (arrayIndex < 0) {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Index can not be less than Zero.");
        }
        if (array.Length - arrayIndex < Count) {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Invalid offset (" + arrayIndex + ") for source length " + array.Length);
        }

        int i = arrayIndex;
        foreach (T? item in (ICollection<T?>)this) {
            array[i] = item;
            ++i;
        }
    }

    /// <summary>Gets a sequence of live and dead objects from the collection. Does not cause a purge.</summary>
    /// <returns>The sequence of live and dead objects.</returns>
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}