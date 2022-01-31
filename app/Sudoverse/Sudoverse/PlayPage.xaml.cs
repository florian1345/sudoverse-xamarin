using Sudoverse.Display;
using Sudoverse.Engine;
using System;
using Xamarin.Forms;

namespace Sudoverse
{
    public partial class PlayPage : ContentPage
	{
		private SudokuView sudokuView;

		public PlayPage(Sudoku sudoku)
		{
			InitializeComponent();
			sudokuView = new SudokuView(sudoku);
			Layout.Children.Insert(0, sudokuView);
		}

		private void OnOne(object sender, EventArgs e)
		{
			sudokuView.Enter(1);
		}

		private void OnTwo(object sender, EventArgs e)
		{
			sudokuView.Enter(2);
		}

		private void OnThree(object sender, EventArgs e)
		{
			sudokuView.Enter(3);
		}

		private void OnFour(object sender, EventArgs e)
		{
			sudokuView.Enter(4);
		}

		private void OnFive(object sender, EventArgs e)
		{
			sudokuView.Enter(5);
		}

		private void OnSix(object sender, EventArgs e)
		{
			sudokuView.Enter(6);
		}

		private void OnSeven(object sender, EventArgs e)
		{
			sudokuView.Enter(7);
		}

		private void OnEight(object sender, EventArgs e)
		{
			sudokuView.Enter(8);
		}

		private void OnNine(object sender, EventArgs e)
		{
			sudokuView.Enter(9);
		}

		private void OnClear(object sender, EventArgs e)
		{
			sudokuView.ClearCell();
		}

		private void OnCheck(object sender, EventArgs e)
		{
			string json = sudokuView.Sudoku.ToJson();
			
			if (SudokuEngineProvider.Engine.CheckDefault(json))
            {
				DisplayAlert("Check", "No mistakes detected.", "Ok");
            }
			else
            {
				DisplayAlert("Check", "Not correct.", "Ok");
			}
		}

		private void OnBack(object sender, EventArgs e)
		{
			App.Current.MainPage = new MainPage();
		}
	}
}
