namespace Sudoverse.SudokuModel
{

    /// <summary>
    /// An enumeration of the different types of notation that can be used in a sudoku cell.
    /// </summary>
    public enum Notation
    {
        /// <summary>
        /// "Normal" notation, i.e. a single, big digit in the center of the cell. This counts as
        /// committing to the digit as the correct digit for the cell.
        /// </summary>
        Normal,

        /// <summary>
        /// "Small" notation, i.e. a group of small digits in the center of the cell.
        /// </summary>
        Small,

        /// <summary>
        /// "Corner" notation, i.e. at most four small digis in the corners of the cell.
        /// </summary>
        Corner
    }
}
