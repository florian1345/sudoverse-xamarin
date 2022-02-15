using Newtonsoft.Json.Linq;

using Sudoverse.Util;

using System;

namespace Sudoverse.SudokuModel
{
    /// <summary>
    /// A data container which contains a full Sudoku grid.
    /// </summary>
    public sealed class SudokuGrid
    {
        private int[] cells;

        /// <summary>
        /// The size (width and height) of the Sudoku grid.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the digit in this grid at the given column and row. Both coordinates must be in
        /// the range [0, <see cref="Size"/>].
        /// </summary>
        public int this[int column, int row] => cells[row * Size + column];

        private SudokuGrid(int[] cells, int size)
        {
            if (cells.Length != size * size)
                throw new ArgumentException("Cell array has invalid length.");

            Size = size;
            this.cells = cells;
        }
        
        /// <summary>
        /// Loads a Sudoku grid from a JSON token.
        /// </summary>
        /// <exception cref="ParseJsonException">If the JSON did not encode a valid Sudoku grid.
        /// </exception>
        public static SudokuGrid FromJson(JToken token)
        {
            if (!(token is JObject jobject))
                throw new ParseJsonException(token.Type, JTokenType.Object);

            var size = jobject.GetField<JValue>("block_width").ToInt()
                * jobject.GetField<JValue>("block_height").ToInt();
            var cellsArray = jobject.GetField<JArray>("cells");
            var cells = new int[size * size];

            if (cellsArray.Count != cells.Length)
                throw new ParseJsonException(cellsArray.Count, cells.Length);

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = cellsArray[i].ToInt();
            }

            return new SudokuGrid(cells, size);
        }
    }
}
