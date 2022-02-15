using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoverse.Util
{
    /// <summary>
    /// Offers various static utility methods.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Creates a new array tht contains the given element <tt>count</tt> times.
        /// </summary>
        /// <typeparam name="T">The element type of the resulting array.</typeparam>
        /// <param name="element">The element to repeat.</param>
        /// <param name="count">The number of times to repeat the element.</param>
        /// <returns>A new array tht contains the given element <tt>count</tt> times.</returns>
        public static T[] ArrayRepeat<T>(T element, int count) =>
            Enumerable.Repeat(element, count).ToArray();

        /// <summary>
        /// Creates a new array that contains all elements in <tt>a</tt> followed by all elements
        /// in <tt>b</tt>.
        /// </summary>
        /// <typeparam name="T">The element type of the concatenated arrays.</typeparam>
        /// <param name="a">The first array to concatenate. Must not be <tt>null</tt>.</param>
        /// <param name="b">The second array to concatenate. Must not be <tt>null</tt>.</param>
        /// <returns>A new array containing all elements in <tt>a</tt> folowed by all elements in
        /// <tt>b</tt>.</returns>
        /// <exception cref="ArgumentNullException">If either <tt>a</tt> or <tt>b</tt> are
        /// <tt>null</tt>.</exception>
        public static T[] Concat<T>(this T[] a, T[] b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            if (b == null)
                throw new ArgumentNullException(nameof(b));

            T[] result = new T[a.Length + b.Length];
            Array.Copy(a, 0, result, 0, a.Length);
            Array.Copy(b, 0, result, a.Length, b.Length);
            return result;
        }

        /// <summary>
        /// Gets the element at the given index, or <tt>null</tt> of there is no such element.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="ts">The list to search.</param>
        /// <param name="index">The index at which to search.</param>
        /// <returns>The element at the given index, if it is present, and <tt>null</tt> otherwise.
        /// </returns>
        public static T GetOrNull<T>(this IList<T> ts, int index)
        where
            T: class
        {
            if (index < 0 || index >= ts.Count)
                return null;

            return ts[index];
        }
    }
}
