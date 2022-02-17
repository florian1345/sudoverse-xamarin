namespace Sudoverse.Util
{
    /// <summary>
    /// A set of integers less than 32.
    /// </summary>
    public sealed class SmallIntSet
    {
        /// <summary>
        /// An integer that stores the bits representing the set.
        /// </summary>
        public int Data { get; private set; }

        /// <summary>
        /// Creates a new small integer set containing no integers.
        /// </summary>
        public SmallIntSet()
            : this(0) { }

        /// <summary>
        /// Creates a new small integer set from the given data.
        /// </summary>
        /// <param name="data">An integer that stores the bits representing the set. Thsi can be
        /// obtained from another set by accessing <see cref="Data"/>.</param>
        public SmallIntSet(int data)
        {
            Data = data;
        }

        /// <summary>
        /// Determines whether this set contains the given value.
        /// </summary>
        /// <param name="value">The integer value to check. Must be in the range [0, 32[, otherwise
        /// this method is undefined.</param>
        public bool Contains(int value) => (Data & (1 << value)) > 0;

        /// <summary>
        /// Inserts the given value into this set.
        /// </summary>
        /// <param name="value">The integer value to add. Must be in the range [0, 32[, otherwise
        /// this method is undefined.</param>
        public void Add(int value)
        {
            Data |= 1 << value;
        }

        /// <summary>
        /// Removes the given value from this set.
        /// </summary>
        /// <param name="value">The integer value to remove. Must be in the range [0, 32[,
        /// otherwise this method is undefined.</param>
        public void Remove(int value)
        {
            Data &= ~(1 << value);
        }

        /// <summary>
        /// Removes the given value from this set if it is present, and inserts it if it is not.
        /// </summary>
        /// <param name="value">The integer value to toggle. Must be in the range [0, 32[,
        /// otherwise this method is undefined.</param>
        public void Toggle(int value)
        {
            Data ^= 1 << value;
        }

        /// <summary>
        /// Removes all elements from this set.
        /// </summary>
        public void Clear()
        {
            Data = 0;
        }
    }
}
