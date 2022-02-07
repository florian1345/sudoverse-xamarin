using Sudoverse.SudokuModel;
using Sudoverse.Touch;
using Sudoverse.Util;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;

namespace Sudoverse.Display
{
    public sealed class SudokuView : Layout<View>
    {
        private enum SelectMode
        {
            Normal,
            Add,
            Remove
        }

        // all relative to the size of a cell

        private const double FONT_SIZE_FACTOR = 0.5;
        private const double THICK_STROKE_WIDTH_FACTOR = 0.06;
        private const double THIN_STROKE_WIDTH_FACTOR = 0.03;

        public Sudoku Sudoku { get; private set; }

        private SudokuCellView[,] cells;
        private Line[] horizontalLines;
        private Line[] verticalLines;
        private HashSet<(int, int)> selected;
        private ICollection<(int, int)> markedInvalid;
        private bool mouseDown;

        public SharedFlag ShiftDown { get; }
        public SharedFlag ControlDown { get; }

        public SudokuView(Sudoku sudoku) : base()
        {
            Sudoku = sudoku;
            cells = new SudokuCellView[Sudoku.Size, Sudoku.Size];

            for (int row = 0; row < Sudoku.Size; row++)
            {
                for (int column = 0; column < Sudoku.Size; column++)
                {
                    var cell = Sudoku.GetCell(column, row);
                    var view = new SudokuCellView(cell);
                    int fcolumn = column;
                    int frow = row;
                    cells[column, row] = view;
                    Children.Add(view);
                }
            }

            horizontalLines = new Line[Sudoku.Size + 1];
            verticalLines = new Line[Sudoku.Size + 1];

            for (int i = 0; i <= Sudoku.Size; i++)
            {
                var horizontalLine = new Line()
                {
                    Stroke = new SolidColorBrush(Color.Black),
                    StrokeLineCap = PenLineCap.Round
                };
                var verticalLine = new Line()
                {
                    Stroke = new SolidColorBrush(Color.Black),
                    StrokeLineCap = PenLineCap.Round
                };

                horizontalLines[i] = horizontalLine;
                verticalLines[i] = verticalLine;
                Children.Add(horizontalLine);
                Children.Add(verticalLine);
            }

            selected = new HashSet<(int, int)>();
            var effect = new TouchEffect();
            effect.TouchAction += OnTouchAction;
            Effects.Add(effect);
            mouseDown = false;
            ShiftDown = new SharedFlag();
            ControlDown = new SharedFlag();
        }

        private int FindCellCoordinate(double pointCoordinate, Func<int, double> lineCoordinateGetter)
        {
            int cellCoordinateMin = 0;
            int cellCoordinateMax = Sudoku.Size;

            while (cellCoordinateMin != cellCoordinateMax - 1)
            {
                int cellCoordinateMid = (cellCoordinateMin + cellCoordinateMax) / 2;

                if (pointCoordinate >= lineCoordinateGetter(cellCoordinateMid))
                    cellCoordinateMin = cellCoordinateMid;
                else cellCoordinateMax = cellCoordinateMid;
            }

            return cellCoordinateMin;
        }

        private (int, int) FindCellCoordinates(Point location)
        {
            // Note: For some weird reason, using Left and Top instead of Center.X and Center.Y
            // yields more accurate results when clicking on different sides of a line.

            int column = FindCellCoordinate(location.X, i => verticalLines[i].Bounds.Left);
            int row = FindCellCoordinate(location.Y, i => horizontalLines[i].Bounds.Top);

            return (column, row);
        }

        private void OnMoved(int column, int row, SudokuCellView view, SelectMode selectMode)
        {
            switch (selectMode)
            {
                case SelectMode.Normal:
                case SelectMode.Add:
                    if (selected.Add((column, row)))
                        view.Select();
                    break;
                case SelectMode.Remove:
                    if (selected.Remove((column, row)))
                        view.Deselect();
                    break;
            }
        }

        private void OnDoublePressed(SudokuCellView view)
        {
            int digit = view.Digit;

            if (digit == 0) return;

            for (int row = 0; row < Sudoku.Size; row++)
            {
                for (int column = 0; column < Sudoku.Size; column++)
                {
                    var cellView = cells[column, row];

                    if (cellView.Digit == digit && selected.Add((column, row)))
                        cellView.Select();
                }
            }
        }

        private void DeselectAll()
        {
            foreach ((int selectedColumn, int selectedRow) in selected)
            {
                cells[selectedColumn, selectedRow].Deselect();
            }

            selected.Clear();
        }

        private void OnPressed(int column, int row, SudokuCellView view, SelectMode selectMode)
        {
            mouseDown = true;

            switch (selectMode)
            {
                case SelectMode.Normal:
                    if (selected.Count == 1 && selected.Contains((column, row)))
                    {
                        OnDoublePressed(view);
                        return;
                    }

                    DeselectAll();
                    goto case SelectMode.Add;
                case SelectMode.Add:
                    if (selected.Add((column, row)))
                        view.Select();
                    break;
                case SelectMode.Remove:
                    if (selected.Remove((column, row)))
                        view.Deselect();
                    break;
            }
        }

        private void ResetInvalid()
        {
            if (markedInvalid != null)
            {
                foreach ((int column, int row) in markedInvalid)
                {
                    cells[column, row].Deselect();
                }

                markedInvalid = null;
            }
        }

        private void OnTouchAction(object sender, TouchActionEventArgs e)
        {
            if (e.Type == TouchActionType.Moved && !mouseDown)
                return;

            if (e.Type == TouchActionType.Entered || e.Type == TouchActionType.Cancelled)
                return;

            if (e.Type == TouchActionType.Exited || e.Type == TouchActionType.Released)
            {
                mouseDown = false;
                return;
            }

            (int column, int row) = FindCellCoordinates(e.Location);
            var view = cells[column, row];

            if (view == null) return;

            var selectMode = SelectMode.Normal;

            if (ShiftDown) selectMode = SelectMode.Add;
            else if (ControlDown) selectMode = SelectMode.Remove;

            ResetInvalid();

            switch (e.Type)
            {
                case TouchActionType.Pressed:
                    OnPressed(column, row, view, selectMode);
                    break;
                case TouchActionType.Moved:
                    OnMoved(column, row, view, selectMode);
                    break;
            }
        }

        private Operation ApplyToSelected(Func<int, int, Operation> operation)
        {
            var reverseStack = new Stack<Operation>();

            foreach ((int column, int row) in selected)
            {
                var reverse = operation(column, row);

                if (!reverse.IsNop())
                    reverseStack.Push(reverse);
            }

            if (reverseStack.Count == 0)
                return new NoOperation();
            else if (reverseStack.Count == 1)
                return reverseStack.Pop();
            else
            {
                var reverseArray = new Operation[reverseStack.Count];

                for (int i = 0; i < reverseArray.Length; i++)
                {
                    reverseArray[i] = reverseStack.Pop();
                }

                return new CompositeOperation(reverseArray);
            }
        }

        public Operation Enter(int digit, Notation notation) =>
            ApplyToSelected((column, row) => Sudoku.EnterCell(column, row, digit, notation));

        public Operation ClearSelected() =>
            ApplyToSelected((column, row) => Sudoku.ClearCell(column, row));

        public void MarkInvalid(ICollection<(int, int)> invalidCells)
        {
            DeselectAll();

            foreach ((int column, int row) in invalidCells)
            {
                cells[column, row].SetInvalid();
            }

            markedInvalid = invalidCells;
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            // We simply assume that width == height, it should always be like that

            double cellSize = width / Sudoku.Size;
            double fontSize = cellSize * FONT_SIZE_FACTOR;

            for (int row = 0; row < Sudoku.Size; row++)
            {
                for (int column = 0; column < Sudoku.Size; column++)
                {
                    double cellX = x + cellSize * column;
                    double cellY = y + cellSize * row;
                    var rect = new Xamarin.Forms.Rectangle(cellX, cellY, cellSize, cellSize);
                    var view = cells[column, row];
                    view.SetFontSize(fontSize);
                    LayoutChildIntoBoundingRegion(view, rect);
                }
            }

            double thickStrokeWidth = cellSize * THICK_STROKE_WIDTH_FACTOR;
            double thinStrokeWidth = cellSize * THIN_STROKE_WIDTH_FACTOR;

            for (int i = 0; i <= Sudoku.Size; i++)
            {
                double hlineY = y + cellSize * i;
                double hlineStrokeWidth = i % Sudoku.BlockHeight == 0 ? thickStrokeWidth : thinStrokeWidth;
                var hlineRect = new Xamarin.Forms.Rectangle(
                    x - 0.5 * hlineStrokeWidth,
                    hlineY - 0.5 * hlineStrokeWidth,
                    hlineStrokeWidth + width,
                    hlineStrokeWidth
                );
                double vlineX = x + cellSize * i;
                double vlineStrokeWidth = i % Sudoku.BlockWidth == 0 ? thickStrokeWidth : thinStrokeWidth;
                var vlineRect = new Xamarin.Forms.Rectangle(
                    vlineX - 0.5 * vlineStrokeWidth,
                    y - 0.5 * vlineStrokeWidth,
                    vlineStrokeWidth,
                    vlineStrokeWidth + height
                );

                var hline = horizontalLines[i];
                hline.X1 = 0;
                hline.X2 = width;
                hline.Y1 = 0;
                hline.Y2 = 0;
                hline.StrokeThickness = hlineStrokeWidth;
                LayoutChildIntoBoundingRegion(hline, hlineRect);

                var vline = verticalLines[i];
                vline.X1 = 0;
                vline.X2 = 0;
                vline.Y1 = 0;
                vline.Y2 = height;
                vline.StrokeThickness = vlineStrokeWidth;
                LayoutChildIntoBoundingRegion(vline, vlineRect);
            }
        }
    }
}
