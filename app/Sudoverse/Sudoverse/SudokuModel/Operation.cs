namespace Sudoverse.SudokuModel
{
    /// <summary>
    /// An operation that can be applied to a Sudoku to alter it in some way.
    /// </summary>
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

    /// <summary>
    /// A superclass for all <see cref="Operation"/>s that apply to a single cell.
    /// </summary>
    public abstract class CellOperation : Operation
    {
        /// <summary>
        /// The column of the cell this operation applies to.
        /// </summary>
        protected int Column { get; }

        /// <summary>
        /// The row of the cell this operation applies to.
        /// </summary>
        protected int Row { get; }

        /// <summary>
        /// Creates a new cell operation from the cell coordinates it applies to.
        /// </summary>
        /// <param name="column">The column of the cell this operation applies to.</param>
        /// <param name="row">The row of the cell this operation applies to.</param>
        protected CellOperation(int column, int row)
        {
            Column = column;
            Row = row;
        }
    }

    /// <summary>
    /// An <see cref="Operation"/> that enters a specific digit into a cell with a given notation.
    /// </summary>
    public sealed class EnterOperation : CellOperation
    {
        private int digit;
        private Notation notation;

        /// <summary>
        /// Creates a new enter operation from the cell coordinates, the digit, and the notation.
        /// </summary>
        /// <param name="column">The column of the cell this operation applies to.<</param>
        /// <param name="row">The row of the cell this operation applies to.</param>
        /// <param name="digit">The digit to enter into the cell.</param>
        /// <param name="notation">The notation to use for entering the digit.</param>
        public EnterOperation(int column, int row, int digit, Notation notation)
            : base(column, row)
        {
            this.digit = digit;
            this.notation = notation;
        }

        public override Operation Apply(Sudoku sudoku) =>
            sudoku.EnterCell(Column, Row, digit, notation);
    }

    /// <summary>
    /// An <see cref="Operation"/> that clears a specific cell.
    /// </summary>
    public sealed class ClearOperation : CellOperation
    {
        /// <summary>
        /// Creates a new clear operation from the cell coordinates to clear.
        /// </summary>
        /// <param name="column">The column of the cell to clear.</param>
        /// <param name="row">The row of the cell to clear.</param>
        public ClearOperation(int column, int row)
            : base(column, row) { }

        public override Operation Apply(Sudoku sudoku) =>
            sudoku.ClearCell(Column, Row);
    }

    /// <summary>
    /// An <see cref="Operation"/> that consists of multipel sub-operations, which are all applied
    /// whenever the composite operation is applied.
    /// </summary>
    public sealed class CompositeOperation : Operation
    {
        private Operation[] operations;

        /// <summary>
        /// Creates a new composite operation from the sub-<tt>operations</tt>.
        /// </summary>
        /// <param name="operations">An array of the operations to run when this operation is
        /// applied. Note these are executed in the order they appear in the array.</param>
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

    /// <summary>
    /// An <see cref="Operation"/> which does nothing.
    /// </summary>
    public sealed class NoOperation : Operation
    {
        /// <summary>
        /// Creates a new nop.
        /// </summary>
        public NoOperation() { }

        public override Operation Apply(Sudoku sudoku) => this;

        public override bool IsNop() => true;
    }
}
