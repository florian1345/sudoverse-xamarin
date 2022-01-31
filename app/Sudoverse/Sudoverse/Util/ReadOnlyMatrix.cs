namespace Sudoverse.Util
{
    public class ReadOnlyMatrix<T>
    {
        private T[,] matrix;

        public T this[int x, int y]
        {
            get => matrix[x, y];
        }

        public ReadOnlyMatrix(T[,] matrix)
        {
            this.matrix = matrix;
        }

        public int GetLength(int dimension) =>
            matrix.GetLength(dimension);
    }
}
