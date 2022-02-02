using Sudoverse.Display;
using Sudoverse.Engine;
using System;

namespace Sudoverse
{
    public partial class PlayPage : KeyListenerPage
	{
		private SudokuView sudokuView;

		public PlayPage(Sudoku sudoku)
		{
			InitializeComponent();
			sudokuView = new SudokuView(sudoku);
			Layout.Children.Insert(0, sudokuView);
            KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;
		}

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
				case Key.Digit1:
					sudokuView.Enter(1);
					break;
				case Key.Digit2:
					sudokuView.Enter(2);
					break;
				case Key.Digit3:
					sudokuView.Enter(3);
					break;
				case Key.Digit4:
					sudokuView.Enter(4);
					break;
				case Key.Digit5:
					sudokuView.Enter(5);
					break;
				case Key.Digit6:
					sudokuView.Enter(6);
					break;
				case Key.Digit7:
					sudokuView.Enter(7);
					break;
				case Key.Digit8:
					sudokuView.Enter(8);
					break;
				case Key.Digit9:
					sudokuView.Enter(9);
					break;
				case Key.Delete:
					sudokuView.ClearCell();
					break;
				case Key.Shift:
					sudokuView.ShiftDown.Set();
					break;
				case Key.Control:
					sudokuView.ControlDown.Set();
					break;
			}
        }

		private void OnKeyUp(object sender, KeyEventArgs e)
        {
			switch (e.Key)
            {
				case Key.Shift:
					sudokuView.ShiftDown.Reset();
					break;
				case Key.Control:
					sudokuView.ControlDown.Reset();
					break;
            }
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
