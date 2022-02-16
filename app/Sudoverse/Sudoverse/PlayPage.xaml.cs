using Sudoverse.Display;
using Sudoverse.SudokuModel;

using System;

using Xamarin.Forms;

namespace Sudoverse
{
    public partial class PlayPage : SudokuPage
	{
		private static readonly Color NotationButtonSelectedBackground = Color.Gray;
		private static readonly Color NotationButtonUnselectedBackground = Color.LightGray;

		private Notation notation1 = Notation.Center;
		private bool finished = false;

		public PlayPage(Sudoku sudoku)
			: base(sudoku, false)
		{
			InitializeComponent();
			Layout.Children.Insert(0, SudokuView);

			ButtonNotationNormal.BorderColor = NotationButtonSelectedBackground;
			ButtonNotation1.BorderColor = NotationButtonUnselectedBackground;
			ButtonNotation2.BorderColor = NotationButtonUnselectedBackground;
			ButtonNotation3.BorderColor = NotationButtonUnselectedBackground;

			// TODO make more dynamic
			if (SudokuView.Sudoku.PencilmarkType == PencilmarkType.PositionalPencilmarkType)
            {
				ButtonNotation1.Source = "notation_positional.png";
				notation1 = Notation.Positional;
				ButtonNotation2.IsEnabled = false;
				ButtonNotation2.IsVisible = false;
            }
		}

        protected override void OnChanged()
		{
			finished = false;
		}

		protected override void OnCheckCorrect()
		{
			DisplayAlert("Check", "Congratulations, you won!", "Ok");
			finished = true;
			SaveManager.RemoveCurrent();
		}

		public void SaveCurrent()
        {
			if (!finished) SaveManager.SaveCurrent(SudokuView.Sudoku);
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
			UpdateNotationButton(ButtonNotationNormal, Notation == Notation.Normal);
			UpdateNotationButton(ButtonNotation1, Notation == notation1);
			UpdateNotationButton(ButtonNotation2, Notation == Notation.Border);
			UpdateNotationButton(ButtonNotation3, Notation == Notation.Color);
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
			Check();
		}

		private void OnBack(object sender, EventArgs e)
		{
			SaveCurrent();
			App.Current.MainPage = new MainPage();
		}

		private void SetNotation(Notation notation)
        {
			Notation = notation;
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

		private void OnUndo(object sender, EventArgs e)
        {
			SudokuView.Undo();
        }

		private void OnRedo(object sender, EventArgs e)
        {
			SudokuView.Redo();
        }
	}
}
