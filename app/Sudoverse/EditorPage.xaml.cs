using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;

namespace Sudoverse
{
	public partial class EditorPage : ContentPage
	{
		private static readonly Color White = Color.FromRgb(255, 255, 255);

		private Sudoku sudoku;

		public EditorPage()
		{
			InitializeComponent();

			sudoku = new Sudoku(3, 3);
			LoadSudoku();
		}

		private void AddInPosition<T>(T view, int column, int row)
		where
			T: BindableObject, IView
		{
			GridLayout.SetColumn(view, column);
			GridLayout.SetRow(view, row);
			SudokuView.Add(view);
		}

		private void LoadSudoku()
        {
			SudokuView.Clear();
			SudokuView.ColumnDefinitions.Clear();
			SudokuView.RowDefinitions.Clear();

			for (int x = 0; x < sudoku.Size; x++)
			{
				SudokuView.AddColumnDefinition(new ColumnDefinition
				{
					Width = new GridLength(1, GridUnitType.Star)
				});
			}

			for (int y = 0; y < sudoku.Size; y++)
			{
				SudokuView.AddRowDefinition(new RowDefinition
				{
					Height = new GridLength(1, GridUnitType.Star)
				});
			}

			for (int y = 0; y < sudoku.Size; y++)
            {
				for (int x = 0; x < sudoku.Size; x++)
                {
					var boxView = new BoxView { Color = White };
					var label = new Label
					{
						Text = sudoku.GetCell(x, y).ToString(),
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					};

					AddInPosition(boxView, x, y);
					AddInPosition(label, x, y);
				}
            }
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