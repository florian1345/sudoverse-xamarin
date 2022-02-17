namespace Sudoverse.Util
{
    /// <summary>
    /// A wrapper around a two-dimensional array that is read-only.
    /// </summary>
    /// <typeparam name="T">The element type of the matrix.</typeparam>
    public class ReadOnlyMatrix<T>
    {
        private T[,] matrix;

        /// <summary>
        /// Gets the element at the given <tt>column</tt> and <tt>row</tt>.
        /// </summary>
        public T this[int column, int row]
        {
            get => matrix[column, row];
        }

        /// <summary>
        /// Wraps the given <tt>matrix</tt> as a read only matrix.
        /// </summary>
        public ReadOnlyMatrix(T[,] matrix)
        {
            this.matrix = matrix;
        }

        /// <summary>
        /// Gets the number of elements along the given <tt>dimension</tt>.
        /// </summary>
        public int GetLength(int dimension) =>
            matrix.GetLength(dimension);
    }
}
