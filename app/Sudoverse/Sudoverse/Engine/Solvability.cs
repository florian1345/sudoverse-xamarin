namespace Sudoverse.Engine
{
    /// <summary>
    /// An enumeration of the different states a Sudoku can have with regards to solvability.
    /// </summary>
    public enum Solvability : byte
    {
        /// <summary>
        /// There exists exactly one solution. This is the required state for a viable Sudoku
        /// puzzle.
        /// </summary>
        Unique = 0,

        /// <summary>
        /// There exist no solutions.
        /// </summary>
        Impossible = 1,

        /// <summary>
        /// There exist more than one solution.
        /// </summary>
        Ambiguous = 2
    }
}
