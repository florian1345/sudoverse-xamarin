using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse
{
	public partial class EditorPage : ContentPage
	{
		private static readonly Color White = Color.FromRgb(255, 255, 255);

		private Sudoku sudoku;
		private SudokuView sudokuView;

		public EditorPage()
		{
			InitializeComponent();

			sudoku = new Sudoku(3, 3);

			for (int row = 0; row < sudoku.Size; row++)
            {
				for (int column = 0; column < sudoku.Size; column++)
                {
					sudoku.SetCell(column, row, (4 * column + 5 * row) % 9 + 1);
                }
            }

			sudokuView = new SudokuView(sudoku);
			Layout.Children.Insert(0, sudokuView);
		}

		private void OnSolve(object sender, EventArgs e)
		{
			// TODO
		}

		private void OnCheck(object sender, EventArgs e)
		{
			// TODO
		}

		private void OnBack(object sender, EventArgs e)
		{
			App.Current.MainPage = new MainPage();
		}
	}
}
