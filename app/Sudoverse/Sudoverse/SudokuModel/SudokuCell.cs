using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Sudoverse.SudokuModel
{
    /// <summary>
    /// Represents the contents of one Sudoku cell. This includes any annotations made by the user.
    /// </summary>
    public sealed class SudokuCell
    {
        public event EventHandler Updated;

        /// <summary>
        /// Indicates whether this cell is locked. If this is the case, no digits can be entered or
        /// removed.
        /// </summary>
        public bool Locked { get; private set; }

        /// <summary>
        /// The big digit entered in this cell, or 0 if there is none.
        /// </summary>
        public int Digit { get; private set; }

        /// <summary>
        /// Indicates whether this cell is filled with a big digit.
        /// </summary>
        public bool Filled => Digit > 0;

        /// <summary>
        /// The pencilmark contained in this cell. This may be hidden by its big digit.
        /// </summary>
        public IPencilmark Pencilmark { get; }

        /// <summary>
        /// Creates a new, empty Sudoku cell with the given pencilmark.
        /// </summary>
        public SudokuCell(IPencilmark pencilmark)
            : this(pencilmark, 0) { }

        /// <summary>
        /// Creates a new locked Sudoku cell filled with the given digit, or an unlocked, empty
        /// cell with the given pencilmark if it is 0.
        /// </summary>
        public SudokuCell(IPencilmark pencilmark, int digit)
        {
            Digit = digit;
            Pencilmark = pencilmark;

            if (digit > 0)
                Locked = true;
        }

        /// <summary>
        /// Clears the top layer of this cell, i.e. the big digit, if there is one, or the small-
        /// and corner-digits otherwise. A cell with annotations and a digit requires clearing
        /// twice. Returns an enumeration of all digits with their notation that need to be entered
        /// for the previous state to be restored.
        /// </summary>
        public IEnumerable<(int, Notation)> Clear()
        {
            if (!Locked)
            {
                if (Filled)
                {
                    var oldDigit = Digit;
                    Digit = 0;
                    Updated?.Invoke(this, new EventArgs());
                    yield return (oldDigit, Notation.Normal);
                }
                else
                {
                    var enumerable = Pencilmark.Clear();
                    bool some = false;

                    foreach (var item in enumerable)
                    {
                        some = true;
                        yield return item;
                    }

                    if (some) Updated?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Enters a digit to the cell using the given notation. If this is
        /// <see cref="Notation.Normal"/>, the big digit will be replaced, otherwise the call will
        /// be forwarded to <see cref="IPencilmark.Enter(int, Notation)"/> for the pencilmark in
        /// this cell.
        /// </summary>
        public bool Enter(int digit, Notation notation)
        {
            if (Locked) return false;

            if (notation == Notation.Normal)
            {
                if (Digit != digit)
                {
                    Digit = digit;
                    Updated?.Invoke(this, new EventArgs());
                    return true;
                }
                else return false;
            }
            else if (!Filled && Pencilmark.Enter(digit, notation))
            {
                Updated?.Invoke(this, new EventArgs());
                return true;
            }

            return false;
        }

        public JObject ToJson()
        {
            var pencilmark = Pencilmark.ToJson();

            return new JObject()
            {
                { "locked", Locked },
                { "digit", Digit },
                { "pencilmark", pencilmark }
            };
        }
        
        public static SudokuCell ParseJson(JToken token, PencilmarkType pencilmarkType)
        {
            if (!(token is JObject jobject))
                throw new ParseSudokuException();

            var lockedValue = jobject.GetField<JValue>("locked");

            if (lockedValue.Type != JTokenType.Boolean)
                throw new ParseSudokuException();

            var locked = (bool)lockedValue;
            var digit = jobject.GetField<JValue>("digit").ToInt();
            var pencilmark = pencilmarkType.FromJson(jobject.GetValue("pencilmark"));

            return new SudokuCell(pencilmark)
            {
                Digit = digit,
                Locked = locked
            };
        }
    }
}
