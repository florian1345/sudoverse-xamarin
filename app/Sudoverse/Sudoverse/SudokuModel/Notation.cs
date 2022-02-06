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
        /// "Center" notation, i.e. a group of small digits in the center of the cell. Used in the
        /// <see cref="CenterBorderPencilmark"/>.
        /// </summary>
        Center,

        /// <summary>
        /// "Border" notation, i.e. at most eight small digis along the border of the cell. Corners
        /// are filled first. Used in the <see cref="CenterBorderPencilmark"/>.
        /// </summary>
        Border,

        /// <summary>
        /// "Positional" notation, i.e. the digit is entered in a position according to its value.
        /// Used in the <see cref="PositionalPencilmark"/>.
        /// </summary>
        Positional
    }
}
