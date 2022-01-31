using Xamarin.Forms;
using Xamarin.Forms.Shapes;

namespace Sudoverse.Display
{
    internal sealed class SudokuView : Layout<View>
    {
        // all relative to the size of a cell

        private const double FONT_SIZE_FACTOR = 0.5;
        private const double THICK_STROKE_WIDTH_FACTOR = 0.06;
        private const double THIN_STROKE_WIDTH_FACTOR = 0.03;

        public Sudoku Sudoku { get; private set; }

        private SudokuCellView[,] cells;
        private Line[] horizontalLines;
        private Line[] verticalLines;
        private int selectedColumn, selectedRow;

        public SudokuView(Sudoku sudoku) : base()
        {
            Sudoku = sudoku;
            cells = new SudokuCellView[Sudoku.Size, Sudoku.Size];

            for (int row = 0; row < Sudoku.Size; row++)
            {
                for (int column = 0; column < Sudoku.Size; column++)
                {
                    var view = new SudokuCellView();
                    int cell = Sudoku.GetCell(column, row);

                    if (cell > 0)
                    {
                        view.Enter(cell);
                        view.Lock();
                    }

                    cells[column, row] = view;
                    int fRow = row;
                    int fColumn = column;
                    view.Tapped += (s, e) =>
                    {
                        if (selectedRow >= 0 && selectedColumn >= 0)
                        {
                            cells[selectedColumn, selectedRow].Deselect();
                        }

                        view.Select();
                        selectedColumn = fColumn;
                        selectedRow = fRow;
                    };
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

            selectedColumn = -1;
            selectedRow = -1;
        }

        public void Enter(int digit)
        {
            if (selectedColumn < 0 || selectedRow < 0) return;

            if (cells[selectedColumn, selectedRow].Enter(digit))
                Sudoku.SetCell(selectedColumn, selectedRow, digit);
        }

        public void ClearCell()
        {
            if (selectedColumn < 0 || selectedRow < 0) return;

            if (cells[selectedColumn, selectedRow].Clear())
                Sudoku.SetCell(selectedColumn, selectedRow, 0);
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
