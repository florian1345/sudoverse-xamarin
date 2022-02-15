using Sudoverse.SudokuModel;

namespace Sudoverse.Engine
{
    /// <summary>
    /// Wraps a <see cref="ISudokuEngine"/> and gives higher level functions.
    /// </summary>
    public sealed class EngineWrapper
    {
        private ISudokuEngine engine;

        public EngineWrapper(ISudokuEngine engine)
        {
            this.engine = engine;
        }

        public int Test() => engine.Test();

        public Sudoku Gen(int constraint, int difficulty, PencilmarkType pencilmarkType)
        {
            string json = engine.Gen(constraint, difficulty);
            return Sudoku.ParseJson(json, pencilmarkType);
        }

        public CheckResponse Check(Sudoku sudoku)
        {
            var sudokuJson = sudoku.ToJson();
            var responseJson = engine.Check(sudokuJson);
            return CheckResponse.ParseJson(responseJson);
        }

        public FillResponse Fill(Sudoku sudoku)
        {
            var sudokuJson = sudoku.ToJson();
            var responseJson = engine.Fill(sudokuJson);
            return FillResponse.ParseJson(responseJson);
        }

        public Solvability IsSolvable(Sudoku sudoku) =>
            (Solvability)engine.IsSolvable(sudoku.ToJson());
    }
}
