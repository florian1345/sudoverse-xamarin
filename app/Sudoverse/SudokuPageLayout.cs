using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoverse
{
    /// <summary>
    /// Lays out the first child as a large square at the start of the longer axis (i.e. at the top
    /// in a vertical view and on the left in a horizontal view) and all other children on the
    /// opposite side along the shorter axis (i.e. as a row on the bottom in a vertical view and as
    /// a column on the right in a horizontal view).
    /// </summary>
    public class SudokuPageLayout : Layout<View>
    {
        public SudokuPageLayout() { }

        private void LayoutVertical(
            double x, double y, double width, double height, View sudoku, IEnumerable<View> menu)
		{
            double minMenuHeight = menu.Select(m => m.MinimumHeightRequest).Max();
            double sudokuSize = Math.Min(width, height - minMenuHeight);
            double menuHeight = height - sudokuSize;

            double sudokuX = x + (width - sudokuSize) * 0.5;
            LayoutChildIntoBoundingRegion(sudoku, new Rectangle(sudokuX, y, sudokuSize, sudokuSize));

            double optimalMenuWidth = menu.Select(m => m.WidthRequest).Sum();
            double minMenuWidth = menu.Select(m => m.MinimumWidthRequest).Sum();
            double menuWidth = Math.Min(optimalMenuWidth, Math.Min(minMenuWidth, width));

            var widthComputer = (View v) =>
            {
                if (width > optimalMenuWidth) return v.WidthRequest;
                else if (width > minMenuWidth) return v.MinimumWidthRequest;
                else return v.MinimumWidthRequest * width / minMenuWidth;
            };

            double elementX = x + (width - menuWidth) * 0.5;
            double elementY = y + sudokuSize;

            foreach (View element in menu) {
                double elementWidth = widthComputer(element);
                double elementHeight = menuHeight;
                var rect = new Rectangle(elementX, elementY, elementWidth, elementHeight);
                LayoutChildIntoBoundingRegion(element, rect);
                elementX += elementWidth;
            }
		}

		private void LayoutHorizontal(
            double x, double y, double width, double height, View sudoku, IEnumerable<View> menu)
        {
            double minMenuWidth = menu.Select(m => m.MinimumWidthRequest).Max();
            double sudokuSize = Math.Min(width - minMenuWidth, height);
            double menuWidth = width - sudokuSize;

            double sudokuY = y + (height - sudokuSize) * 0.5;
            LayoutChildIntoBoundingRegion(sudoku, new Rectangle(x, sudokuY, sudokuSize, sudokuSize));

            double optimalMenuHeight = menu.Select(m => m.HeightRequest).Sum();
            double minMenuHeight = menu.Select(m => m.MinimumHeightRequest).Sum();
            double menuHeight = Math.Min(optimalMenuHeight, Math.Min(minMenuHeight, height));

            var heightComputer = (View v) =>
            {
                if (height > optimalMenuHeight) return v.HeightRequest;
                else if (height > minMenuHeight) return v.MinimumHeightRequest;
                else return v.MinimumHeightRequest * height / minMenuHeight;
            };

            double elementX = x + sudokuSize;
            double elementY = y + (height - menuHeight) * 0.5;

            foreach (View element in menu)
            {
                double elementWidth = menuWidth;
                double elementHeight = heightComputer(element);
                var rect = new Rectangle(elementX, elementY, elementWidth, elementHeight);
                LayoutChildIntoBoundingRegion(element, rect);
                elementY += elementHeight;
            }
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
