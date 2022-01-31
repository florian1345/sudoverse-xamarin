using System;

namespace Sudoverse.Util
{
    /// <summary>
    /// Offers various static utility methods.
    /// </summary>
    internal static class Utils
    {
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
    }
}
