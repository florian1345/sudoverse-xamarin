using Sudoverse.SudokuModel;

namespace Sudoverse.Engine
{
    /// <summary>
    /// Wraps a <see cref="ISudokuEngine"/> and gives higher level functions.
    /// </summary>
    public sealed class EngineWrapper
    {
        private ISudokuEngine engine;

        /// <summary>
        /// Creates a new engine wrapper around the given low-level <tt>engine</tt>.
        /// </summary>
        public EngineWrapper(ISudokuEngine engine)
        {
            this.engine = engine;
        }

        /// <summary>
        /// Returns 42. For checking that the engine is loaded correctly.
        /// </summary>
        public int Test() => engine.Test();

        /// <summary>
        /// Generates a new, random <see cref="Sudoku"/> with the given difficutly, constraint, and
        /// pencilmark type.
        /// </summary>
        /// <param name="constraint">The constraint identifier for the generated Sudoku. Check out
        /// the crate-level documentation of the <tt>engine</tt> crate for a list of valid IDs.
        /// </param>
        /// <param name="difficulty">The difficulty on a scale from 1 to 5, both inclusive.</param>
        /// <param name="pencilmarkType">The <see cref="PencilmarkType"/> of the pencilmarks that
        /// can be entered into the cells of the generated Sudoku. This is not relevant for
        /// generation itself, but must be known here to ensure the returned Sudoku allows the
        /// correct type of pencilmarks.</param>
        public Sudoku Gen(int constraint, int difficulty, PencilmarkType pencilmarkType)
        {
            string json = engine.Gen(constraint, difficulty);
            return Sudoku.ParseJson(json, pencilmarkType);
        }

        /// <summary>
        /// Checks whether the given Sudoku is valid, i.e. its constraints are not violated by the
        /// big digits. Note this does <i>not</i> indicate that all digits are part of the true
        /// solution, only that they are internally consistent so far.
        /// </summary>
        /// <param name="sudoku">The Sudoku to check.</param>
        /// <returns>A <see cref="CheckResponse"/> that contains the result of the check. In case
        /// it is not valid, this also contains the cells which violated the constraint.</returns>
        public CheckResponse Check(Sudoku sudoku)
        {
            var sudokuJson = sudoku.ToJson();
            var responseJson = engine.Check(sudokuJson);
            return CheckResponse.ParseJson(responseJson);
        }

        /// <summary>
        /// Finds a random filling of the given Sudoku that respects its constraints and the digits
        /// already present. If not possibe, the response will indicate so.
        /// </summary>
        /// <param name="sudoku">The Sudoku to fill.</param>
        /// <returns>A <see cref="FillResponse"/> that contains the result of the operation, i.e.
        /// whether it was successful and the data, if it was.</returns>
        public FillResponse Fill(Sudoku sudoku)
        {
            var sudokuJson = sudoku.ToJson();
            var responseJson = engine.Fill(sudokuJson);
            return FillResponse.ParseJson(responseJson);
        }

        /// <summary>
        /// Checks the solvability of the given Sudoku, i.e. whether it is uniquely solvable,
        /// impossible, or ambiguous.
        /// </summary>
        /// <param name="sudoku">The Sudoku to check.</param>
        /// <returns>The <see cref="Solvability"/> of the given Sudoku.</returns>
        public Solvability IsSolvable(Sudoku sudoku) =>
            (Solvability)engine.IsSolvable(sudoku.ToJson());
    }
}
