using System.Linq;

namespace Sudoverse.SudokuModel
{
    public abstract class Operation
    {
        /// <summary>
        /// Applies the operation to the given Sudoku and returns its inverse.
        /// </summary>
        public abstract Operation Apply(Sudoku sudoku);

        /// <summary>
        /// Returns true if and only if this operation is always nop, no matter the Sudoku.
        /// </summary>
        public virtual bool IsNop() => false;
    }

    public abstract class CellOperation : Operation
    {
        protected int Column { get; }
        protected int Row { get; }

        protected CellOperation(int column, int row)
        {
            Column = column;
            Row = row;
        }
    }

    public sealed class EnterOperation : CellOperation
    {
        private int digit;
        private Notation notation;

        public EnterOperation(int column, int row, int digit, Notation notation)
            : base(column, row)
        {
            this.digit = digit;
            this.notation = notation;
        }

        public override Operation Apply(Sudoku sudoku) =>
            sudoku.EnterCell(Column, Row, digit, notation);
    }

    public sealed class ClearOperation : CellOperation
    {
        public ClearOperation(int column, int row)
            : base(column, row) { }

        public override Operation Apply(Sudoku sudoku) =>
            sudoku.ClearCell(Column, Row);
    }

    public sealed class CompositeOperation : Operation
    {
        private Operation[] operations;

        public CompositeOperation(Operation[] operations)
        {
            this.operations = operations;
        }

        public override Operation Apply(Sudoku sudoku)
        {
            var inverse = new Operation[operations.Length];

            for (int i = 0; i < operations.Length; i++)
            {
                inverse[operations.Length - i - 1] = operations[i].Apply(sudoku);
            }

            return new CompositeOperation(inverse);
        }

        public override bool IsNop() => operations.Length == 0;
    }

    public sealed class NoOperation : Operation
    {
        public NoOperation() { }

        public override Operation Apply(Sudoku sudoku) => this;

        public override bool IsNop() => true;
    }
}
