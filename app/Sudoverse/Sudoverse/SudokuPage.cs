using Sudoverse.Display;
using Sudoverse.Engine;
using Sudoverse.SudokuModel;

namespace Sudoverse
{
    /// <summary>
    /// Manages access to a <see cref="Display.SudokuView"/> from a page perspective, that is,
    /// forwards input from keyboard (on UWP) and offers methods for manipulation.
    /// </summary>
    public abstract class SudokuPage : KeyListenerPage
    {
		protected Notation Notation { get; set; }
        protected SudokuView SudokuView { get; private set; }

        public SudokuPage(Sudoku sudoku, bool editor)
        {
			Notation = Notation.Normal;
			SudokuView = new SudokuView(sudoku, editor);
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }

		protected abstract void OnChanged();

		protected void Enter(int digit)
		{
			SudokuView.Enter(digit, Notation);
			OnChanged();
		}

		protected void Clear()
		{
			SudokuView.ClearSelected();
			OnChanged();
		}

		protected abstract void OnCheckCorrect();

		protected void Check()
        {
			var checkResponse = SudokuEngineProvider.Engine.Check(SudokuView.Sudoku);

			if (checkResponse.Valid)
			{
				if (SudokuView.Sudoku.Full) OnCheckCorrect();
				else DisplayAlert("Check", "Correct so far.", "Ok");
			}
			else
			{
				SudokuView.MarkInvalid(checkResponse.InvalidCells);
				DisplayAlert("Check", "Not correct.", "Ok");
			}
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Digit1:
					Enter(1);
					break;
				case Key.Digit2:
					Enter(2);
					break;
				case Key.Digit3:
					Enter(3);
					break;
				case Key.Digit4:
					Enter(4);
					break;
				case Key.Digit5:
					Enter(5);
					break;
				case Key.Digit6:
					Enter(6);
					break;
				case Key.Digit7:
					Enter(7);
					break;
				case Key.Digit8:
					Enter(8);
					break;
				case Key.Digit9:
					Enter(9);
					break;
				case Key.Y:
					if (SudokuView.ControlDown) SudokuView.Redo();
					break;
				case Key.Z:
					if (SudokuView.ControlDown) SudokuView.Undo();
					break;
				case Key.Delete:
					Clear();
					break;
				case Key.Shift:
					SudokuView.ShiftDown.Set();
					break;
				case Key.Control:
					SudokuView.ControlDown.Set();
					break;
			}
		}

		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Shift:
					SudokuView.ShiftDown.Reset();
					break;
				case Key.Control:
					SudokuView.ControlDown.Reset();
					break;
			}
		}
	}
}
