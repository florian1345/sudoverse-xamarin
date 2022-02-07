namespace Sudoverse.Engine
{
    public interface ISudokuEngine
    {
        /// <summary>
        /// Returns 42. For checking that the engine is loaded correctly.
        /// </summary>
        int Test();

        /// <summary>
        /// Generates a new Sudoku of the given difficulty with the constraint represented by its
        /// ID. Check out the crate-level documentation of the <tt>engine</tt> crate for a list of
        /// valid IDs. Difficulty is measured on a scale from 1 to 5, both inclusive. The Sudoku is
        /// returned in JSON form, which can be parsed using
        /// <see cref="SudokuModel.Sudoku.ParseJson(string, SudokuModel.PencilmarkType)"/>.
        /// </summary>
        string Gen(int constraint, int difficulty);

        /// <summary>
        /// Checks whether the given Sudoku is correct. The Sudoku must be provided in JSON form,
        /// which can be obtained using <see cref="SudokuModel.Sudoku.ToJson()"/>. The result is
        /// returned as JSON as well, which can be parsed using
        /// <see cref="CheckResponse.ParseJson(string)"/>.
        /// </summary>
        string Check(string json);
    }
}
