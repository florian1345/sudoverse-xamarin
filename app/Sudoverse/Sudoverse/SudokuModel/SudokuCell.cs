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
        private SortedSet<int> smallDigits;
        private SortedSet<int> cornerDigits;

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
        /// A <see cref="ReadOnlySet{T}"/> which contains all small digits in the center of this
        /// cell. They may be ignored if <see cref="Digit"/> is not 0.
        /// </summary>
        public ReadOnlySet<int> SmallDigits => new ReadOnlySet<int>(smallDigits);

        /// <summary>
        /// A <see cref="ReadOnlySet{T}"/> which contains all small digits in the corners of this
        /// cell. They may be ignored if <see cref="Digit"/> is not 0.
        /// </summary>
        public ReadOnlySet<int> CornerDigits => new ReadOnlySet<int>(cornerDigits);

        /// <summary>
        /// Creates a new, empty Sudoku cell.
        /// </summary>
        public SudokuCell()
            : this(0) { }

        /// <summary>
        /// Creates a new locked Sudoku cell filled with the given digit, or an unlocked, empty
        /// cell if it is 0.
        /// </summary>
        public SudokuCell(int digit)
        {
            Digit = digit;
            smallDigits = new SortedSet<int>();
            cornerDigits = new SortedSet<int>();

            if (digit > 0)
                Locked = true;
        }

        /// <summary>
        /// Clears the top layer of this cell, i.e. the big digit, if there is one, or the small-
        /// and corner-digits otherwise. A cell with annotations and a digit requires clearing
        /// twice. Returns <tt>true</tt> if and only if something changed.
        /// </summary>
        public bool Clear()
        {
            if (Locked) return false;

            if (Filled)
            {
                Digit = 0;
                Updated?.Invoke(this, new EventArgs());
                return true;
            }
            else if (smallDigits.Count + cornerDigits.Count > 0)
            {
                smallDigits.Clear();
                cornerDigits.Clear();
                Updated?.Invoke(this, new EventArgs());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Enters a big digit to the cell. For clearing, use <see cref="Clear"/> instead. Returns
        /// true if changed.
        /// </summary>
        public bool EnterNormal(int digit)
        {
            if (Locked) return false;

            if (Digit != digit)
            {
                Digit = digit;
                Updated?.Invoke(this, new EventArgs());
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Toggles a small digit to the corners of this cell. If there are already four different
        /// digits in the corners, nothing is changed. True is returned if something changed.
        /// </summary>
        public bool ToggleCorner(int digit)
        {
            if (Locked) return false;

            if (cornerDigits.Add(digit))
            {
                if (cornerDigits.Count > 4)
                {
                    cornerDigits.Remove(digit);
                    return false;
                }

                Updated?.Invoke(this, new EventArgs());
                return true;
            }
            else if (cornerDigits.Remove(digit))
            {
                Updated?.Invoke(this, new EventArgs());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Enters a small digit to the center of this cell. Returns <tt>true</tt> if and only if
        /// something changed.
        /// </summary>
        public bool ToggleSmall(int digit)
        {
            if (Locked) return false;

            if (smallDigits.Add(digit) || smallDigits.Remove(digit))
            {
                Updated?.Invoke(this, new EventArgs());
                return true;
            }

            return false;
        }

        private JArray ToJArray(SortedSet<int> digits)
        {
            var array = new JArray();

            foreach (int digit in digits)
            {
                array.Add(digit);
            }

            return array;
        }

        public JObject ToJson()
        {
            var small = ToJArray(smallDigits);
            var corner = ToJArray(cornerDigits);

            return new JObject()
            {
                { "locked", Locked },
                { "digit", Digit },
                { "small", small },
                { "corner", corner }
            };
        }

        private static SortedSet<int> FromJArray(JArray array)
        {
            var set = new SortedSet<int>();

            foreach (JValue jvalue in array)
            {
                set.Add(jvalue.ToInt());
            }

            return set;
        }
        
        public static SudokuCell ParseJson(JToken token)
        {
            if (!(token is JObject jobject))
                throw new ParseSudokuException();

            var lockedValue = jobject.GetField<JValue>("locked");

            if (lockedValue.Type != JTokenType.Boolean)
                throw new ParseSudokuException();

            var locked = (bool)lockedValue;
            var digit = jobject.GetField<JValue>("digit").ToInt();
            var smallDigits = FromJArray(jobject.GetField<JArray>("small"));
            var cornerDigits = FromJArray(jobject.GetField<JArray>("corner"));

            return new SudokuCell()
            {
                Digit = digit,
                Locked = locked,
                smallDigits = smallDigits,
                cornerDigits = cornerDigits
            };
        }
    }
}
