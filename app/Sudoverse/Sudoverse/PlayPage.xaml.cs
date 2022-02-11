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
		private Notation notation1 = Notation.Center;
		private bool finished = false;

		public PlayPage(Sudoku sudoku)
		{
			InitializeComponent();
			sudokuView = new SudokuView(sudoku);
			Layout.Children.Insert(0, sudokuView);
            KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;

			ButtonNotationNormal.BorderColor = NotationButtonSelectedBackground;
			ButtonNotation1.BorderColor = NotationButtonUnselectedBackground;
			ButtonNotation2.BorderColor = NotationButtonUnselectedBackground;
			ButtonNotation3.BorderColor = NotationButtonUnselectedBackground;

			undos = new DropOutStack<Operation>(UNDO_CAPACITY);
			redos = new DropOutStack<Operation>(UNDO_CAPACITY);

			// TODO make more dynamic
			if (sudokuView.Sudoku.PencilmarkType == PencilmarkType.PositionalPencilmarkType)
            {
				ButtonNotation1.Source = "notation_positional.png";
				notation1 = Notation.Positional;
				ButtonNotation2.IsEnabled = false;
				ButtonNotation2.IsVisible = false;
            }
		}

		public void SaveCurrent()
        {
			if (!finished) SaveManager.SaveCurrent(sudokuView.Sudoku);
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
			UpdateNotationButton(ButtonNotation1, notation == notation1);
			UpdateNotationButton(ButtonNotation2, notation == Notation.Border);
			UpdateNotationButton(ButtonNotation3, notation == Notation.Color);
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
			finished = false;
		}

		private void Clear()
		{
			PushUndo(sudokuView.ClearSelected());
			finished = false;
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
			var checkResponse = SudokuEngineProvider.Engine.Check(sudokuView.Sudoku);
			
			if (checkResponse.Valid)
			{
				if (sudokuView.Sudoku.Full)
				{
					DisplayAlert("Check", "Congratulations, you won!", "Ok");
					finished = true;
					SaveManager.RemoveCurrent();
				}
				else DisplayAlert("Check", "No mistakes so far.", "Ok");
            }
			else
            {
				sudokuView.MarkInvalid(checkResponse.InvalidCells);
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

		private void SetButtonLabel(Button button, string text, Color color)
		{
			button.Text = text;
			button.TextColor = color;
		}

		private void SetNumberLabels()
        {
			SetButtonLabel(ButtonDigit1, "1", Color.Default);
			SetButtonLabel(ButtonDigit2, "2", Color.Default);
			SetButtonLabel(ButtonDigit3, "3", Color.Default);
			SetButtonLabel(ButtonDigit4, "4", Color.Default);
			SetButtonLabel(ButtonDigit5, "5", Color.Default);
			SetButtonLabel(ButtonDigit6, "6", Color.Default);
			SetButtonLabel(ButtonDigit7, "7", Color.Default);
			SetButtonLabel(ButtonDigit8, "8", Color.Default);
			SetButtonLabel(ButtonDigit9, "9", Color.Default);
		}

		private void SetColorLabel(Button button, Color color)
        {
			var opaqueColor = color.A == 0.0 ? Color.White : Color.FromRgba(color.R, color.G, color.B, 1.0);
			SetButtonLabel(button, "⬛", opaqueColor);
        }

		private void SetColorLabels()
		{
			SetColorLabel(ButtonDigit1, SudokuCellView.BackgroundColors[0]);
			SetColorLabel(ButtonDigit2, SudokuCellView.BackgroundColors[1]);
			SetColorLabel(ButtonDigit3, SudokuCellView.BackgroundColors[2]);
			SetColorLabel(ButtonDigit4, SudokuCellView.BackgroundColors[3]);
			SetColorLabel(ButtonDigit5, SudokuCellView.BackgroundColors[4]);
			SetColorLabel(ButtonDigit6, SudokuCellView.BackgroundColors[5]);
			SetColorLabel(ButtonDigit7, SudokuCellView.BackgroundColors[6]);
			SetColorLabel(ButtonDigit8, SudokuCellView.BackgroundColors[7]);
			SetColorLabel(ButtonDigit9, SudokuCellView.BackgroundColors[8]);
		}

        private void OnNotationNormal(object sender, EventArgs e)
        {
			SetNotation(Notation.Normal);
			SetNumberLabels();
		}

		private void OnNotation1(object sender, EventArgs e)
		{
			SetNotation(notation1);
			SetNumberLabels();
		}

		private void OnNotation2(object sender, EventArgs e)
		{
			SetNotation(Notation.Border);
			SetNumberLabels();
		}

		private void OnNotation3(object sender, EventArgs e)
        {
			SetNotation(Notation.Color);
			SetColorLabels();
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
