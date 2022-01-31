using Sudoverse.Constraint;
using Sudoverse.Engine;
using System;
using Xamarin.Forms;

namespace Sudoverse
{
    public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

			if (!EngineLoaded())
			{
				DisplayAlert("Error", "Could not load native engine.", "Quit");

				// Since Xamarin does not allow us to exit the app, we make it useless instead.

				ButtonPlay.IsEnabled = false;
			}
		}

		private bool EngineLoaded()
		{
			var engine = SudokuEngineProvider.Engine;

			if (engine == null) return false;

			try
			{
				int test = SudokuEngineProvider.Engine.Test();
				return test == 42;
			}
			catch
            {
				return false;
            }
		}

		private void OnPlay(object sender, EventArgs e)
		{
			string json = SudokuEngineProvider.Engine.GenDefault();
			Sudoku sudoku = Sudoku.ParseJson<StatelessConstraint>(json);
			App.Current.MainPage = new PlayPage(sudoku);
		}
	}
}
