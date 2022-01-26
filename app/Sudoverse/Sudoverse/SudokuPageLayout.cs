using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Sudoverse
{
    /// <summary>
    /// Lays out the given Sudoku as a large square at the start of the longer axis (i.e. at the
    /// top in a vertical view and on the left in a horizontal view) and all other children on the
    /// opposite side along the shorter axis (i.e. as a row on the bottom in a vertical view and as
    /// a column on the right in a horizontal view).
    /// </summary>
    public class SudokuPageLayout : Layout<View>
    {
        private const double FONT_SIZE_FACTOR = 0.05;
        private const double SUDOKU_SIZE_FACTOR = 0.8;

        private IEnumerable<View> GetAllChildren(View v)
        {
            yield return v;

            if (v is IViewContainer<View> vs)
            {
                foreach (View child in vs.Children)
                {
                    foreach (View recursiveChild in GetAllChildren(child))
                    {
                        yield return recursiveChild;
                    }
                }
            }
        }

        private void UpdateFontSize(View sudoku)
        {
            double fontSize = sudoku.Width * FONT_SIZE_FACTOR;

            foreach (var child in GetAllChildren(sudoku))
            {
                if (child is Label label)
                {
                    label.FontSize = fontSize;
                }
            }
        }

        private void LayoutVertical(
            double x, double y, double width, double height, View sudoku, IEnumerable<View> menu)
        {
            double sudokuSize = Math.Min(SUDOKU_SIZE_FACTOR * height, width);
            double menuHeight = height - sudokuSize;

            double sudokuX = x + (width - sudokuSize) * 0.5;
            LayoutChildIntoBoundingRegion(sudoku, new Rectangle(sudokuX, y, sudokuSize, sudokuSize));

            double elementX = 0;
            double elementY = y + sudokuSize;
            double elementWidth = width / menu.Count();

            foreach (View element in menu)
            {
                double elementHeight = menuHeight;
                var rect = new Rectangle(elementX, elementY, elementWidth, elementHeight);
                LayoutChildIntoBoundingRegion(element, rect);
                elementX += elementWidth;
            }

            UpdateFontSize(sudoku);
        }

        private void LayoutHorizontal(
            double x, double y, double width, double height, View sudoku, IEnumerable<View> menu)
        {
            double sudokuSize = Math.Min(SUDOKU_SIZE_FACTOR * width, height);
            double menuWidth = width - sudokuSize;

            double sudokuY = y + (height - sudokuSize) * 0.5;
            LayoutChildIntoBoundingRegion(sudoku, new Rectangle(x, sudokuY, sudokuSize, sudokuSize));

            double elementX = x + sudokuSize;
            double elementY = 0;
            double elementHeight = height / menu.Count();

            foreach (View element in menu)
            {
                double elementWidth = menuWidth;
                var rect = new Rectangle(elementX, elementY, elementWidth, elementHeight);
                LayoutChildIntoBoundingRegion(element, rect);
                elementY += elementHeight;
            }

            UpdateFontSize(sudoku);
        }

        protected override void LayoutChildren(
            double x, double y, double width, double height)
        {
            if (Children.Count == 0) return;

            var sudoku = Children[0];
            var menu = Children.Skip(1);

            if (width > height) LayoutHorizontal(x, y, width, height, sudoku, menu);
            else LayoutVertical(x, y, width, height, sudoku, menu);
        }
    }
}
