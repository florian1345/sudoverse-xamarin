using Newtonsoft.Json.Linq;
using Sudoverse.Util;
using System.Collections.Generic;

namespace Sudoverse.SudokuModel
{
    /// <summary>
    /// A <see cref="IPencilmark"/> that allows noting digits in a position according to their
    /// value inside a cell. This can only be used for 9x9 Sudoku.
    /// </summary>
    public sealed class PositionalPencilmark : IPencilmark
    {
        private const int MAX_DIGIT = 9;

        private SmallIntSet digits;

        public string TopLeft => GetDigitString(1);

        public string TopCenter => GetDigitString(2);

        public string TopRight => GetDigitString(3);

        public string CenterLeft => GetDigitString(4);

        public string Center => GetDigitString(5);

        public string CenterRight => GetDigitString(6);

        public string BottomLeft => GetDigitString(7);

        public string BottomCenter => GetDigitString(8);

        public string BottomRight => GetDigitString(9);

        /// <summary>
        /// Creates a new, initially empty positional pencilmark.
        /// </summary>
        public PositionalPencilmark()
            : this(new SmallIntSet()) { }

        private PositionalPencilmark(SmallIntSet digits)
        {
            this.digits = digits;
        }

        private string GetDigitString(int digit)
        {
            if (digits.Contains(digit)) return digit.ToString();
            else return "";
        }

        public bool Enter(int digit, Notation notation)
        {
            if (notation != Notation.Positional)
                throw new UnsupportedNotationException();

            digits.Toggle(digit);
            return true;
        }

        public IEnumerable<(int, Notation)> Clear()
        {
            for (int i = 1; i <= MAX_DIGIT; i++)
            {
                if (digits.Contains(i)) yield return (i, Notation.Positional);
            }

            digits.Clear();
        }

        public JToken ToJson() => digits.Data;

        /// <summary>
        /// Loads a positional pencilmark from JSON data.
        /// </summary>
        /// <exception cref="ParseJsonException">If the JSON data doe not represent a valid
        /// positional pencilmark.</exception>
        public static PositionalPencilmark FromJson(JToken token) =>
            new PositionalPencilmark(new SmallIntSet(token.ToInt()));
    }
}
