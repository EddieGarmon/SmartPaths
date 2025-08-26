namespace SmartPaths;

/// <summary>A collection of weak references to objects of type <typeparamref name="T" />. The
///     <see cref="ICollection{T}" /> implementations are understood to work on all the objects of the
///     collection (both dead and alive), and do not implicitly cause purges.</summary>
/// <typeparam name="T">The type of objects to hold weak references to.</typeparam>
internal interface IWeakCollection<T> : ICollection<T>, IDisposable
{

    /// <summary>Gets a sequence of live objects from the collection, causing a purge. The purge may occur
    ///     at the beginning of the enumeration, at the end of the enumeration, or progressively as the
    ///     sequence is enumerated.</summary>
    IEnumerable<T> LiveList { get; }

    /// <summary>Removes all dead objects from the collection.</summary>
    void Purge();

}