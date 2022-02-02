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

        public ReadOnlySet(ISet<T> set)
        {
            this.set = set;
        }

        public int Count => set.Count;

        public bool Contains(T item) =>
            set.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) =>
            set.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() =>
            set.GetEnumerator();

        public bool IsProperSubsetOf(IEnumerable<T> other) =>
            set.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) =>
            set.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other) =>
            set.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) =>
            set.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other) =>
            set.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other) =>
            set.SetEquals(other);

        IEnumerator IEnumerable.GetEnumerator() =>
            set.GetEnumerator();
    }
}
