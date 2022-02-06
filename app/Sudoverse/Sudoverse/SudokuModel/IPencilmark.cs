using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Sudoverse.SudokuModel
{
    /// <summary>
    /// An exception raised if a digit is attempted to be entered into a pencil mark with an
    /// invalid notation for that kind of pencil mark.
    /// </summary>
    public sealed class UnsupportedNotationException : Exception
    {
        public UnsupportedNotationException()
            : base("The used pencilmark type does not support the given notation.") { }
    }

    /// <summary>
    /// An interface for all types that represent pencil marks inside a <see cref="SudokuCell"/>.
    /// </summary>
    public interface IPencilmark
    {
        /// <summary>
        /// The string to display in the top-left corner of the cell.
        /// </summary>
        string TopLeft { get; }

        /// <summary>
        /// The string to display in the top-center spot of the cell.
        /// </summary>
        string TopCenter { get; }

        /// <summary>
        /// The string to display in the top-right corner of the cell.
        /// </summary>
        string TopRight { get; }

        /// <summary>
        /// The string to display in the center-left spot of the cell.
        /// </summary>
        string CenterLeft { get; }

        /// <summary>
        /// The string to display in the center of the cell.
        /// </summary>
        string Center { get; }

        /// <summary>
        /// The string to display in the center-right spot of the cell.
        /// </summary>
        string CenterRight { get; }

        /// <summary>
        /// The string to display in the bottom-left corner of the cell.
        /// </summary>
        string BottomLeft { get; }

        /// <summary>
        /// The string to display in the bottom-center spot of the cell.
        /// </summary>
        string BottomCenter { get; }

        /// <summary>
        /// The string to display in the bottom-right corner of the cell.
        /// </summary>
        string BottomRight { get; }

        /// <summary>
        /// Enters a digit in the cell using the given notation. Returns <tt>true</tt> if and only
        /// if this pencilmark has changed.
        /// </summary>
        /// <exception cref="UnsupportedNotationException">If the given notation is not supported
        /// by this pencilmark type.</exception>
        bool Enter(int digit, Notation notation);

        /// <summary>
        /// Clears this pencilmark and enumerates all digits that were removed with their
        /// respective notation.
        /// </summary>
        IEnumerable<(int, Notation)> Clear();

        /// <summary>
        /// Converts this pencilmark into JSON format from which it can be parsed again.
        /// </summary>
        JToken ToJson();
    }
}
