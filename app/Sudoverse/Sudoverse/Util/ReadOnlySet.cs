using System.Collections;
using System.Collections.Generic;

namespace Sudoverse.Util
{
    /// <summary>
    /// A read-only wrapper around an <see cref="ISet{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type of the set.</typeparam>
    public sealed class ReadOnlySet<T> : IEnumerable<T>
    {
        private ISet<T> set;

        /// <summary>
        /// Creates a new read-only wrapper around the given <tt>set</tt>.
        /// </summary>
        public ReadOnlySet(ISet<T> set)
        {
            this.set = set;
        }

        /// <summary>
        /// Gets the number of elements in the set.
        /// </summary>
        public int Count => set.Count;

        /// <summary>
        /// Determines whether this set contains the given <tt>item</tt>.
        /// </summary>
        public bool Contains(T item) =>
            set.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) =>
            set.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() =>
            set.GetEnumerator();

        /// <summary>
        /// Determines whether this set is a proper (strict) subset of a given <tt>other</tt>
        /// collection.
        /// </summary>
        public bool IsProperSubsetOf(IEnumerable<T> other) =>
            set.IsProperSubsetOf(other);

        /// <summary>
        /// Determines whether this set is a proper (strict) superset of a given <tt>other</tt>
        /// collection.
        /// </summary>
        public bool IsProperSupersetOf(IEnumerable<T> other) =>
            set.IsProperSupersetOf(other);

        /// <summary>
        /// Determines whether this set is a subset of a given <tt>other</tt> collection.
        /// </summary>
        public bool IsSubsetOf(IEnumerable<T> other) =>
            set.IsSubsetOf(other);

        /// <summary>
        /// Determines whether this set is a superset of a given <tt>other</tt> collection.
        /// </summary>
        public bool IsSupersetOf(IEnumerable<T> other) =>
            set.IsSupersetOf(other);

        /// <summary>
        /// Determines whether this set overlaps with a given <tt>other</tt> collection.
        /// </summary>
        public bool Overlaps(IEnumerable<T> other) =>
            set.Overlaps(other);

        /// <summary>
        /// Determines whether this set and a given <tt>other</tt> collection contain the same
        /// elements.
        /// </summary>
        public bool SetEquals(IEnumerable<T> other) =>
            set.SetEquals(other);

        IEnumerator IEnumerable.GetEnumerator() =>
            set.GetEnumerator();
    }
}
