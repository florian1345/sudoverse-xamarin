using Sudoverse.Constraint;
using Sudoverse.Engine;
using Sudoverse.SudokuModel;

using System;
using System.Threading.Tasks;

using Xamarin.Forms.Xaml;

namespace Sudoverse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditorPage : SudokuPage
    {
		private bool modified;
		private string name;

        public EditorPage(IConstraint constraint)
			: this(new Sudoku(3, 3, constraint, PencilmarkType.NoPencilmarkType), null) { }

		public EditorPage(Sudoku sudoku, string name)
			: base(sudoku, true)
		{
			InitializeComponent();
			Layout.Children.Insert(0, SudokuView);
			SudokuView.Sudoku.Constraint.EditorFrameFocused +=
				(sender, e) => SudokuView.DeselectAll();
			modified = false;
			this.name = name;
		}

		protected override void OnChanged()
        {
			modified = true;
		}

        protected override void OnCheckCorrect()
		{
			DisplayAlert("Check", "Correct.", "Ok");
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

		private void OnUndo(object sender, EventArgs e)
		{
			SudokuView.Undo();
		}

		private void OnRedo(object sender, EventArgs e)
		{
			SudokuView.Redo();
		}

		private void OnCheck(object sender, EventArgs e)
		{
			Check();
		}

		private void OnSolve(object sender, EventArgs e)
        {
			switch (SudokuEngineProvider.Engine.IsSolvable(SudokuView.Sudoku))
            {
				case Solvability.Unique:
					DisplayAlert("Solvability", "The Sudoku is uniquely solvable.", "Ok");
					break;
				case Solvability.Impossible:
					DisplayAlert("Solvability", "The Sudoku has no solution.", "Ok");
					break;
				case Solvability.Ambiguous:
					DisplayAlert("Solvability", "The Sudoku has multiple solutions.", "Ok");
					break;
			}
        }

		private void OnFill(object sender, EventArgs e)
        {
			var response = SudokuEngineProvider.Engine.Fill(SudokuView.Sudoku);

			if (response.Successful)
			{
				SudokuView.FillWith(response.Grid);
				OnChanged();
			}
			else DisplayAlert("Error", "No valid grid was found.", "Ok");
		}

		private async Task Save()
		{
			// TODO find a way to avoid deselecting and simultaneously block keyboard input
			SudokuView.DeselectAll();

			if (name == null)
				name = await DisplayPromptAsync("Name", "Enter puzzle name");

			SaveManager.SavePuzzle(SudokuView.Sudoku, name);
			modified = false;
		}

		private async void OnSave(object sender, EventArgs e)
        {
			await Save();
		}

		private async void OnBack(object sender, EventArgs e)
		{
			if (modified)
            {
				bool save = await DisplayAlert("Save",
					"There are unsaved changes. Do you want to save the puzzle before exiting?",
					"Yes", "No");

				if (save) await Save();
            }

			App.Current.MainPage = new MainPage();
		}
	}
}
