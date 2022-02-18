using Sudoverse.SudokuModel;

using System;

namespace Sudoverse.Constraint
{
    /// <summary>
    /// An exception that is raised whenever a <see cref="ConstraintOperation"/> is applied to a
    /// <see cref="IConstraint"/> that does not match the structure expected by the operation.
    /// </summary>
    public sealed class ConstraintStructureException : Exception
    {
        /// <summary>
        /// Creates a new constraint structure exception with a default message.
        /// </summary>
        public ConstraintStructureException()
            : base("The constraint structure does not match the operation structure.") { }
    }

    /// <summary>
    /// An <see cref="Operation"/> that applies to the <see cref="IConstraint"/> of a Sudoku.
    /// </summary>
    public abstract class ConstraintOperation : Operation
    {
        public override Operation Apply(Sudoku sudoku) =>
            Apply(sudoku.Constraint);

        /// <summary>
        /// Applies this operation to a constraint and returns its inverse.
        /// </summary>
        public abstract Operation Apply(IConstraint constraint);
    }

    /// <summary>
    /// A <see cref="ConstraintOperation"/> that applies some operation to one partial constraint
    /// of a <see cref="CompositeConstraint"/>.
    /// </summary>
    public sealed class CompositeConstraintOperation : ConstraintOperation
    {
        private int index;
        private ConstraintOperation operation;

        /// <summary>
        /// Creates a new composite constraint operation that applies the specified operation to
        /// the subconstraint with the given index.
        /// </summary>
        /// <param name="index">The 0-based index of the subconstraint to which to apply the given
        /// operation within the list of constraints in the composite constraint to which the
        /// created operation is applied. Must be non-negative. The created operation can only be
        /// applied to composite constraints with a number of subconstraints greated than this
        /// number.</param>
        /// <param name="operation">The constraint operation to apply to the subconstraint.</param>
        /// <exception cref="ArgumentException">If <tt>index</tt> is negative.</exception>
        public CompositeConstraintOperation(int index, ConstraintOperation operation)
        {
            if (index < 0)
                throw new ArgumentException("Index must be non-negative.");

            this.index = index;
            this.operation = operation;
        }

        public override Operation Apply(IConstraint constraint)
        {
            if (!(constraint is CompositeConstraint compositeConstraint))
                throw new ConstraintStructureException();

            if (index >= compositeConstraint.Constraints.Count)
                throw new ConstraintStructureException();

            var reverse = operation.Apply(compositeConstraint.Constraints[index]);

            if (reverse is ConstraintOperation reverseConstraint)
                return new CompositeConstraintOperation(index, reverseConstraint);
            else return reverse;
        }

        public override bool IsNop() =>
            operation.IsNop();
    }

    /// <summary>
    /// An enumeration of the different axes along which a sandwich constraint annotates sandwich
    /// sums.
    /// </summary>
    public enum SandwichSumAxis
    {
        /// <summary>
        /// The line of sandwich sums of columns, annotated above the grid.
        /// </summary>
        Columns,

        /// <summary>
        /// The line of sandwich sums of rows, annotated to the left of the grid.
        /// </summary>
        Rows
    }

    /// <summary>
    /// A superclass for all <see cref="ConstraintOperation"/> that apply to the
    /// <see cref="SandwichConstraint"/>.
    /// </summary>
    public abstract class SandwichConstraintOperation : ConstraintOperation
    {
        /// <summary>
        /// The <see cref="SandwichSumAxis"/> along which the sandwich sum modified by this
        /// operation lies.
        /// </summary>
        protected SandwichSumAxis Axis { get; }

        /// <summary>
        /// The index of the sandwich sum modified by this operation within its line.
        /// </summary>
        protected int Index { get; }

        /// <summary>
        /// Creates a new sandwich constraint operation from a specification which sum is modified.
        /// </summary>
        /// <param name="axis">The <see cref="SandwichSumAxis"/> along which the sandwich sum
        /// modified by the created operation lies.</param>
        /// <param name="index">The index of the sandwich sum modified by the created operation
        /// within its line. Must not be negative.</param>
        /// <exception cref="ArgumentException">If <tt>index</tt> is negative.</exception>
        protected SandwichConstraintOperation(SandwichSumAxis axis, int index)
        {
            if (index < 0)
                throw new ArgumentException("Index must be non-negative.");

            Axis = axis;
            Index = index;
        }

        /// <summary>
        /// Checks the constraint's structure and returns a sandwich constraint, if it is valid.
        /// </summary>
        /// <exception cref="ConstraintStructureException">If the constraint is not a sandwich
        /// constraint, or it does not have a sum with the <see cref="Index"/>.</exception>
        protected SandwichConstraint CheckConstraint(IConstraint constraint)
        {
            if (!(constraint is SandwichConstraint sandwichConstraint))
                throw new ConstraintStructureException();

            if (Index >= sandwichConstraint.SudokuSize)
                throw new ConstraintStructureException();

            return sandwichConstraint;
        }
    }

    /// <summary>
    /// A <see cref="SandwichConstraintOperation"/> that sets the sandwich sum of one row or
    /// column.
    /// </summary>
    public sealed class SetSandwichSumConstraintOperation : SandwichConstraintOperation
    {
        private int sum;

        /// <summary>
        /// Creates a new set sandwich sum constraint operation that sets the specified sum to the
        /// given value.
        /// </summary>
        /// <param name="axis">The <see cref="SandwichSumAxis"/> along which the sandwich sum set
        /// by the created operation lies.</param>
        /// <param name="index">The index of the sandwich sum assigned by the created operation
        /// within its line. Must not be negative.</param>
        /// <param name="sum">The value to which to set the specified sandwich sum.</param>
        /// <exception cref="ArgumentException">If <tt>index</tt> is negative.</exception>
        public SetSandwichSumConstraintOperation(SandwichSumAxis axis, int index, int sum)
            : base(axis, index)
        {
            this.sum = sum;
        }

        public override Operation Apply(IConstraint constraint)
        {
            var sandwichConstraint = CheckConstraint(constraint);
            int oldSum;

            switch (Axis)
            {
                case SandwichSumAxis.Columns:
                    oldSum = sandwichConstraint.SetColumnSandwich(Index, sum, false);
                    break;
                case SandwichSumAxis.Rows:
                    oldSum = sandwichConstraint.SetRowSandwich(Index, sum, false);
                    break;
                default: throw new Exception("Unknown SandwichSumAxis. This should not happen.");
            }

            if (oldSum == -1) return new ClearSandwichSumConstraintOperation(Axis, Index);
            else if (oldSum != sum)
                return new SetSandwichSumConstraintOperation(Axis, Index, oldSum);
            else return new NoOperation();
        }
    }

    /// <summary>
    /// A <see cref="SandwichConstraintOperation"/> that removes the sandwich sum from one row or
    /// column.
    /// </summary>
    public sealed class ClearSandwichSumConstraintOperation : SandwichConstraintOperation
    {
        /// <summary>
        /// Creates a new clear sandwich sum constraint operation that removes the specified sum.
        /// </summary>
        /// <param name="axis">The <see cref="SandwichSumAxis"/> along which the sandwich sum
        /// removed by the created operation lies.</param>
        /// <param name="index">The index of the sandwich sum removed by the created operation
        /// within its line. Must not be negative.</param>
        /// <exception cref="ArgumentException">If <tt>index</tt> is negative.</exception>
        public ClearSandwichSumConstraintOperation(SandwichSumAxis axis, int index)
            : base(axis, index) { }

        public override Operation Apply(IConstraint constraint)
        {
            var sandwichConstraint = CheckConstraint(constraint);
            int oldSum;

            switch (Axis)
            {
                case SandwichSumAxis.Columns:
                    oldSum = sandwichConstraint.ClearColumnSandwich(Index, false);
                    break;
                case SandwichSumAxis.Rows:
                    oldSum = sandwichConstraint.ClearRowSandwich(Index, false);
                    break;
                default: throw new Exception("Unknown SandwichSumAxis. This should not happen.");
            }

            if (oldSum != -1) return new SetSandwichSumConstraintOperation(Axis, Index, oldSum);
            else return new NoOperation();
        }
    }
}
