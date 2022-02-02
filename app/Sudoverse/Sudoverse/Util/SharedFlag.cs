using System.Threading;

namespace Sudoverse.Util
{
    /// <summary>
    /// A thread-safe boolean flag.
    /// </summary>
    internal sealed class SharedFlag
    {
        private int flag;

        /// <summary>
        /// Creates a new shared flag with an initial state of <tt>false</tt>.
        /// </summary>
        public SharedFlag()
        {
            flag = 0;
        }

        /// <summary>
        /// Sets the flag to <tt>true</tt>.
        /// </summary>
        public void Set()
        {
            Interlocked.Exchange(ref flag, 1);
        }

        /// <summary>
        /// Resets the flag to <tt>false</tt>.
        /// </summary>
        public void Reset()
        {
            Interlocked.Exchange(ref flag, 0);
        }

        /// <summary>
        /// Reads the flag.
        /// </summary>
        /// <returns><tt>true</tt>, if and only if the flag is set, <tt>false</tt> otherwise.
        /// </returns>
        public bool IsSet() =>
            Interlocked.CompareExchange(ref flag, 1, 1) == 1;

        /// <summary>
        /// See <see cref="IsSet"/>.
        /// </summary>
        public static implicit operator bool(SharedFlag f) => f.IsSet();
    }
}
