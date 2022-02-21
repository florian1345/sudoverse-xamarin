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
        public event EventHandler<ConstraintOperation> Changed;
        public event EventHandler ChangedInternal;

        private int[] columnSandwiches;
        private int[] rowSandwiches;

        /// <summary>
        /// The number of sandwich clues in each line (top and left, i.e. for columns and rows).
        /// </summary>
        public int SudokuSize => columnSandwiches.Length;

        public string Type => "sandwich";

        private SandwichConstraint(int[] columnSandwiches, int[] rowSandwiches)
        {
            this.columnSandwiches = columnSandwiches;
            this.rowSandwiches = rowSandwiches;
            Changed += (sender, op) => ChangedInternal?.Invoke(sender, new EventArgs());
        }

        /// <summary>
        /// Creates a new, empty sandwich constraint (i.e. without any sandwich clues) for a Sudoku
        /// of the given <tt>sudokuSize</tt>.
        /// </summary>
        public SandwichConstraint(int sudokuSize)
            : this(Utils.ArrayRepeat(-1, sudokuSize), Utils.ArrayRepeat(-1, sudokuSize)) { }

        /// <summary>
        /// Gets the sandwich sum annotated for the column with the given <tt>index</tt>, or -1 if
        /// there is no sum.
        /// </summary>
        public int GetColumnSandwich(int index) => columnSandwiches[index];

        /// <summary>
        /// Gets the sandwich sum annotated for the row with the given <tt>index</tt>, or -1 if
        /// there is no sum.
        /// </summary>
        public int GetRowSandwich(int index) => rowSandwiches[index];

        private int SetSandwich(int[] sandwiches, int index, int sum, bool raiseEvent,
            SandwichSumAxis axis)
        {
            int old = sandwiches[index];
            sandwiches[index] = sum;

            if (old != sum)
            {
                if (raiseEvent)
                {
                    ConstraintOperation reverse;

                    if (old == -1) reverse = new ClearSandwichSumConstraintOperation(axis, index);
                    else reverse = new SetSandwichSumConstraintOperation(axis, index, old);

                    Changed?.Invoke(this, reverse);
                }
                else ChangedInternal?.Invoke(this, new EventArgs());
            }

            return old;
        }

        /// <summary>
        /// Sets the sandwich sum annotated for the column with the given <tt>index</tt> to the
        /// given <tt>sum</tt>. If you want to clear the sum, use
        /// <see cref="ClearColumnSandwich(int)"/> instead. Returns the old sum at that position,
        /// or -1 if there was none. If <tt>raiseEvent</tt> is set to false, <see cref="Changed"/>
        /// will not be invoked.
        /// </summary>
        public int SetColumnSandwich(int index, int sum, bool raiseEvent = true) =>
            SetSandwich(columnSandwiches, index, sum, raiseEvent, SandwichSumAxis.Columns);

        /// <summary>
        /// Sets the sandwich sum annotated for the row with the given <tt>index</tt> to the given
        /// <tt>sum</tt>. If you want to clear the sum, use <see cref="ClearRowSandwich(int)"/>
        /// instead. Returns the old sum at that position, or -1 if there was none. If
        /// <tt>raiseEvent</tt> is set to false, <see cref="Changed"/> will not be invoked.
        /// </summary>
        public int SetRowSandwich(int index, int sum, bool raiseEvent = true) =>
            SetSandwich(rowSandwiches, index, sum, raiseEvent, SandwichSumAxis.Rows);

        /// <summary>
        /// Removes the sandwich sum annotated for the column with the given <tt>index</tt>.
        /// Returns the old sum at that position, or -1 if there was none. If <tt>raiseEvent</tt>
        /// is set to false, <see cref="Changed"/> will not be invoked.
        /// </summary>
        public int ClearColumnSandwich(int index, bool raiseEvent = true) =>
            SetSandwich(columnSandwiches, index, -1, raiseEvent, SandwichSumAxis.Columns);

        /// <summary>
        /// Removes the sandwich sum annotated for the row with the given <tt>index</tt>. Returns
        /// the old sum at that position, or -1 if there was none. If <tt>raiseEvent</tt> is set to
        /// false, <see cref="Changed"/> will not be invoked.
        /// </summary>
        public int ClearRowSandwich(int index, bool raiseEvent = true) =>
            SetSandwich(rowSandwiches, index, -1, raiseEvent, SandwichSumAxis.Rows);

        public View[] GetBackgroundViews(ReadOnlyMatrix<ReadOnlyRect> fieldBounds) =>
            new View[0];

        private View ToFrameView(int sandwich)
        {
            if (sandwich >= 0)
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

        private View[] ToEditorLine(int[] sandwiches, Func<int, int, int> setter,
            Func<int, int> clearer)
        {
            var line = new View[sandwiches.Length];

            for (int i = 0; i < line.Length; i++)
            {
                var view = new FrameNumberEditView();
                int fi = i;

                view.NumberChanged += (sender, e) =>
                {
                    if (view.Number == null) clearer(fi);
                    else setter(fi, (int)view.Number);
                };

                ChangedInternal += (sender, e) =>
                {
                    if (sandwiches[fi] == -1) view.Number = null;
                    else view.Number = sandwiches[fi];
                };

                if (sandwiches[fi] >= 0) view.Number = sandwiches[fi];

                view.Focused += (sender, e) => EditorFrameFocused?.Invoke(sender, e);
                line[i] = view;
            }

            return line;
        }

        public FrameGroup GetEditorFrames() =>
            FrameGroup.Singleton(new Frame.Builder()
                .WithTopLine(ToEditorLine(columnSandwiches,
                    (column, sum) => SetColumnSandwich(column, sum),
                    column => ClearColumnSandwich(column)))
                .WithLeftLine(ToEditorLine(rowSandwiches,
                    (row, sum) => SetRowSandwich(row, sum),
                    row => ClearRowSandwich(row)))
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

            if (columnSandwiches.Length != rowSandwiches.Length)
                throw new ParseJsonException();

            return new SandwichConstraint(columnSandwiches, rowSandwiches);
        }
    }
}
