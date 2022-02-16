using Newtonsoft.Json.Linq;
using Sudoverse.Util;
using System;
using System.Collections.Generic;

namespace Sudoverse.SudokuModel
{
    /// <summary>
    /// Represents the contents of one Sudoku cell. This includes any annotations made by the user.
    /// </summary>
    public sealed class SudokuCell
    {
        /// <summary>
        /// Raised whenever the content of this cell, i.e. its <see cref="Digit"/>, its
        /// <see cref="Pencilmark"/>, or its <see cref="ColorIndex"/> change in any way.
        /// </summary>
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
        /// The 1-based index of the background color of this cell.
        /// </summary>
        public int ColorIndex { get; private set; }

        /// <summary>
        /// Creates a new, empty Sudoku cell with the given pencilmark.
        /// </summary>
        public SudokuCell(IPencilmark pencilmark)
            : this(pencilmark, 0) { }

        /// <summary>
        /// Creates a new locked Sudoku cell filled with the given digit, or an unlocked, empty
        /// cell with the given pencilmark if it is 0. The parameter <tt>locked</tt> determines
        /// whether the cell will be unable to be modified. It is ignored and the cell will remain
        /// unlocked should <tt>digit</tt> be 0.
        /// </summary>
        public SudokuCell(IPencilmark pencilmark, int digit, bool locked = true)
        {
            Digit = digit;
            Pencilmark = pencilmark;
            ColorIndex = 1;

            if (digit > 0 && locked)
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
            if (notation == Notation.Color)
            {
                if (ColorIndex != digit)
                {
                    ColorIndex = digit;
                    Updated?.Invoke(this, new EventArgs());
                    return true;
                }
                else return false;

            }

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
                { "pencilmark", pencilmark },
                { "color", ColorIndex }
            };
        }
        
        public static SudokuCell ParseJson(JToken token, PencilmarkType pencilmarkType)
        {
            if (!(token is JObject jobject))
                throw new ParseJsonException();

            var lockedValue = jobject.GetField<JValue>("locked");

            if (lockedValue.Type != JTokenType.Boolean)
                throw new ParseJsonException();

            var locked = (bool)lockedValue;
            var digit = jobject.GetField<JValue>("digit").ToInt();
            var pencilmark = pencilmarkType.FromJson(jobject.GetValue("pencilmark"));
            var colorIndex = jobject.GetField<JValue>("color").ToInt();

            return new SudokuCell(pencilmark)
            {
                Digit = digit,
                Locked = locked,
                ColorIndex = colorIndex
            };
        }
    }
}
