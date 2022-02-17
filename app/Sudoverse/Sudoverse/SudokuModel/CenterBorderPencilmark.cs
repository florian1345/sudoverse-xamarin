using Newtonsoft.Json.Linq;
using Sudoverse.Util;
using System.Collections.Generic;
using System.Linq;

namespace Sudoverse.SudokuModel
{
    /// <summary>
    /// A <see cref="IPencilmark"/> that allows noting digits in the center or along the borders of
    /// the cell. In the latter case, the corners are filled first.
    /// </summary>
    public sealed class CenterBorderPencilmark : IPencilmark
    {
        private const int MAX_BORDER_DIGITS = 8;

        /// <summary>
        /// The value at <tt>(index, count)</tt> contains the index of the digit in the set of
        /// border digits of size <tt>count</tt> to display at the location with index
        /// <tt>index</tt> with respect to the order in which they are filled. If there is nothing
        /// to be displayed, this matrix contains -1 at that position.
        ///
        /// As an example, <tt>BorderDigitIndices[1, 5]</tt> indicates the index of the digit to
        /// display in the top-right corner if there are 5 border digits. In this case, it is 2,
        /// i.e. the third digit, because the second digit is displayed in the top-center spot.
        /// With up to 4 digits that is not the case, which is why
        /// <tt>BorderDigitIndices[1, 4]</tt> is 1.
        /// </summary>
        private static readonly int[,] BorderDigitIndices =
        {
            { -1,  0,  0,  0,  0,  0,  0,  0,  0 },
            { -1, -1,  1,  1,  1,  2,  2,  2,  2 },
            { -1, -1, -1,  2,  2,  3,  4,  5,  5 },
            { -1, -1, -1, -1,  3,  4,  5,  6,  7 },
            { -1, -1, -1, -1, -1,  1,  1,  1,  1 },
            { -1, -1, -1, -1, -1, -1,  3,  3,  3 },
            { -1, -1, -1, -1, -1, -1, -1,  4,  4 },
            { -1, -1, -1, -1, -1, -1, -1, -1,  6 }
        };

        private SortedSet<int> centerDigits;
        private SortedSet<int> borderDigits;

        public string TopLeft => GetDigit(0);

        public string TopCenter => GetDigit(4);

        public string TopRight => GetDigit(1);

        public string CenterLeft => GetDigit(5);

        public string Center => string.Join("", centerDigits.Select(d => d.ToString()));

        public string CenterRight => GetDigit(6);

        public string BottomLeft => GetDigit(2);

        public string BottomCenter => GetDigit(7);

        public string BottomRight => GetDigit(3);

        public CenterBorderPencilmark()
        {
            centerDigits = new SortedSet<int>();
            borderDigits = new SortedSet<int>();
        }

        /// <summary>
        /// Gets the digit to display in the field with the given index, where the index is
        /// respective to the order in which fields are filled.
        /// </summary>
        private string GetDigit(int fieldIndex)
        {
            var index = BorderDigitIndices[fieldIndex, borderDigits.Count];

            if (index < 0) return "";

            // TODO find constant-time way of doing this
            return borderDigits.ElementAt(index).ToString();
        }

        public bool Enter(int digit, Notation notation)
        {
            switch (notation)
            {
                case Notation.Center:
                    return centerDigits.Add(digit) || centerDigits.Remove(digit);
                case Notation.Border:
                    if (borderDigits.Remove(digit))
                        return true;

                    if (borderDigits.Count >= MAX_BORDER_DIGITS)
                        return false;

                    return borderDigits.Add(digit);
                default: throw new UnsupportedNotationException();
            }
        }

        public IEnumerable<(int, Notation)> Clear()
        {
            foreach (int centerDigit in centerDigits)
                yield return (centerDigit, Notation.Center);

            foreach (int borderDigit in borderDigits)
                yield return (borderDigit, Notation.Border);

            centerDigits.Clear();
            borderDigits.Clear();
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

        public JToken ToJson()
        {
            var center = ToJArray(centerDigits);
            var border = ToJArray(borderDigits);

            return new JObject()
            {
                { "center", center },
                { "border", border }
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

        /// <summary>
        /// Loads a center-border pencilmark from JSON data.
        /// </summary>
        /// <exception cref="ParseJsonException">If the JSON data doe not represent a valid
        /// center-border pencilmark.</exception>
        public static CenterBorderPencilmark FromJson(JToken token)
        {
            if (!(token is JObject jobject))
                throw new ParseJsonException();

            var centerDigits = FromJArray(jobject.GetField<JArray>("center"));
            var borderDigits = FromJArray(jobject.GetField<JArray>("border"));

            return new CenterBorderPencilmark()
            {
                centerDigits = centerDigits,
                borderDigits = borderDigits
            };
        }
    }
}
