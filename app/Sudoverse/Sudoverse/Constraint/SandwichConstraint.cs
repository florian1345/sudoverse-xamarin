using Newtonsoft.Json.Linq;

using Sudoverse.Display;
using Sudoverse.Util;

using System;
using System.Linq;

using Xamarin.Forms;

namespace Sudoverse.Constraint
{
    /// <summary>
    /// A constraint which provides numbers above and to the left of the grid which specify the sum
    /// of all cells located between 1 and 9 in their respective column or row (1 and 9 excluded).
    /// </summary>
    public sealed class SandwichConstraint : IConstraint
    {
        public event EventHandler EditorFrameFocused;

        private int[] columnSandwiches;
        private int[] rowSandwiches;

        public string Type => "sandwich";

        private SandwichConstraint(int[] columnSandwiches, int[] rowSandwiches)
        {
            this.columnSandwiches = columnSandwiches;
            this.rowSandwiches = rowSandwiches;
        }

        /// <summary>
        /// Creates a new, empty sandwich constraint (i.e. without any sandwich clues) for a Sudoku
        /// of the given <tt>sudokuSize</tt>.
        /// </summary>
        public SandwichConstraint(int sudokuSize)
            : this(Utils.ArrayRepeat(-1, sudokuSize), Utils.ArrayRepeat(-1, sudokuSize)) { }

        public View[] GetBackgroundViews(ReadOnlyMatrix<ReadOnlyRect> fieldBounds) =>
            new View[0];

        private View ToFrameView(int sandwich)
        {
            if (sandwich > 0)
            {
                var view = new FrameNumberView();
                view.DisplayNumber(sandwich);
                return view;
            }
            else return null;
        }

        private View[] ToFrameLine(int[] sandwiches) =>
            sandwiches.Select(ToFrameView).ToArray();

        public FrameGroup GetFrames() =>
            FrameGroup.Singleton(new Frame.Builder()
                .WithTopLine(ToFrameLine(columnSandwiches))
                .WithLeftLine(ToFrameLine(rowSandwiches))
                .Build());

        private View[] ToEditorLine(int[] sandwiches)
        {
            var line = new View[sandwiches.Length];

            for (int i = 0; i < line.Length; i++)
            {
                var view = new FrameNumberEditView();
                int fi = i;

                view.NumberChanged += (sender, e) =>
                {
                    sandwiches[fi] = view.Number == null ? -1 : (int)view.Number;
                };

                view.Focused += (sender, e) => EditorFrameFocused?.Invoke(sender, e);
                line[i] = view;
            }

            return line;
        }

        public FrameGroup GetEditorFrames() =>
            FrameGroup.Singleton(new Frame.Builder()
                .WithTopLine(ToEditorLine(columnSandwiches))
                .WithLeftLine(ToEditorLine(rowSandwiches))
                .Build());

        private JArray ToJArray(int[] array)
        {
            var jarray = new JArray();

            foreach (int i in array)
                jarray.Add(i >= 0 ? (JValue)i : null);

            return jarray;
        }

        public JToken ToJsonValue()
        {
            return new JObject()
            {
                { "columns", ToJArray(columnSandwiches) },
                { "rows", ToJArray(rowSandwiches) }
            };
        }

        private static int[] ToIntArray(JArray jarray)
        {
            int[] intArray = new int[jarray.Count];

            for (int i = 0; i < jarray.Count; i++)
            {
                intArray[i] = jarray[i].ToInt(-1);
            }

            return intArray;
        }

        /// <summary>
        /// Parses a sandwich constraint from the given JSON data.
        /// </summary>
        /// <exception cref="ParseJsonException">If the JSON data does not represent a valid
        /// sandwich constraint.</exception>
        public static SandwichConstraint FromJsonValue(JToken token)
        {
            if (!(token is JObject jobject))
                throw new ParseJsonException(token.Type, JTokenType.Object);

            var columnSandwiches = ToIntArray(jobject.GetField<JArray>("columns"));
            var rowSandwiches = ToIntArray(jobject.GetField<JArray>("rows"));

            return new SandwichConstraint(columnSandwiches, rowSandwiches);
        }
    }
}
