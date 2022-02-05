using Sudoverse.Display;
using Sudoverse.Engine;
using Sudoverse.SudokuModel;
using Sudoverse.Util;
using System;
using Xamarin.Forms;

namespace Sudoverse
{
    public partial class PlayPage : KeyListenerPage
	{
		private static readonly Color NotationButtonSelectedBackground = Color.Gray;
		private static readonly Color NotationButtonUnselectedBackground = Color.LightGray;

		private const int UNDO_CAPACITY = 255;

		private Notation notation;
		private SudokuView sudokuView;
		private DropOutStack<Operation> undos;
		private DropOutStack<Operation> redos;

		public PlayPage(Sudoku sudoku)
		{
			InitializeComponent();
			sudokuView = new SudokuView(sudoku);
			Layout.Children.Insert(0, sudokuView);
            KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;

			ButtonNotationNormal.BorderColor = NotationButtonSelectedBackground;
			ButtonNotationSmall.BorderColor = NotationButtonUnselectedBackground;
			ButtonNotationCorner.BorderColor = NotationButtonUnselectedBackground;

			undos = new DropOutStack<Operation>(UNDO_CAPACITY);
			redos = new DropOutStack<Operation>(UNDO_CAPACITY);
		}

		public void SaveCurrent()
        {
			SaveManager.SaveCurrent(sudokuView.Sudoku);
		}

		private void UpdateNotationButton(ImageButton button, bool selected)
		{
			if (selected)
				button.BorderColor = NotationButtonSelectedBackground;
			else
				button.BorderColor = NotationButtonUnselectedBackground;
		}

		private void UpdateNotationButtons()
        {
			UpdateNotationButton(ButtonNotationNormal, notation == Notation.Normal);
			UpdateNotationButton(ButtonNotationSmall, notation == Notation.Small);
			UpdateNotationButton(ButtonNotationCorner, notation == Notation.Corner);
		}

		private void PushUndo(Operation operation)
		{
			if (!operation.IsNop())
			{
				undos.Push(operation);
				redos.Clear();
			}
		}

		private void Enter(int digit)
        {
			PushUndo(sudokuView.Enter(digit, notation));
		}

		private void Clear()
		{
			PushUndo(sudokuView.ClearSelected());
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
					if (sudokuView.ControlDown) Redo();
					break;
				case Key.Z:
					if (sudokuView.ControlDown) Undo();
					break;
				case Key.Delete:
					Clear();
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
			Enter(1);
		}

		private void OnTwo(object sender, EventArgs e)
		{
			Enter(2);
		}

		private void OnThree(object sender, EventArgs e)
		{
			Enter(3);
		}

		private void OnFour(object sender, EventArgs e)
		{
			Enter(4);
		}

		private void OnFive(object sender, EventArgs e)
		{
			Enter(5);
		}

		private void OnSix(object sender, EventArgs e)
		{
			Enter(6);
		}

		private void OnSeven(object sender, EventArgs e)
		{
			Enter(7);
		}

		private void OnEight(object sender, EventArgs e)
		{
			Enter(8);
		}

		private void OnNine(object sender, EventArgs e)
		{
			Enter(9);
		}

		private void OnClear(object sender, EventArgs e)
		{
			Clear();
		}

		private void OnCheck(object sender, EventArgs e)
		{
			string json = sudokuView.Sudoku.ToJson();
			
			if (SudokuEngineProvider.Engine.Check(json))
			{
				if (sudokuView.Sudoku.Full)
				{
					DisplayAlert("Check", "Congratulations, you won!", "Ok");
					SaveManager.RemoveCurrent();
				}
				else DisplayAlert("Check", "No mistakes so far.", "Ok");
            }
			else
            {
				DisplayAlert("Check", "Not correct.", "Ok");
			}
		}

		private void OnBack(object sender, EventArgs e)
		{
			SaveCurrent();
			App.Current.MainPage = new MainPage();
		}

		private void SetNotation(Notation notation)
        {
			this.notation = notation;
			UpdateNotationButtons();
        }

        private void OnNotationNormal(object sender, EventArgs e)
        {
			SetNotation(Notation.Normal);
		}

		private void OnNotationSmall(object sender, EventArgs e)
		{
			SetNotation(Notation.Small);
		}

		private void OnNotationCorner(object sender, EventArgs e)
		{
			SetNotation(Notation.Corner);
		}

		private void Undo()
        {
			if (!undos.Empty)
				redos.Push(undos.Pop().Apply(sudokuView.Sudoku));
        }

		private void Redo()
		{
			if (!redos.Empty)
				undos.Push(redos.Pop().Apply(sudokuView.Sudoku));
		}

		private void OnUndo(object sender, EventArgs e)
        {
			Undo();
        }

		private void OnRedo(object sender, EventArgs e)
        {
			Redo();
        }
	}
}
