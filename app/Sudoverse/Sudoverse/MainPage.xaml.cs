using Sudoverse.Constraint;
using Sudoverse.Engine;
using Sudoverse.SudokuModel;
using System;
using Xamarin.Forms;

namespace Sudoverse
{
    public partial class MainPage : ContentPage
	{
		private sealed class LoadEngineException : Exception
        {
			public LoadEngineException(string message)
				: base(message) { }
        }

		public MainPage()
		{
			InitializeComponent();

			if (!SaveManager.HasCurrent())
				ButtonContinue.IsEnabled = false;

			try
            {
				VerifyEngine();
            }
			catch (Exception e)
			{
				string message = e is LoadEngineException ? e.Message : e.ToString();
				DisplayAlert("Error", "Could not load native engine: " + message, "Quit");

				// Since Xamarin does not allow us to exit the app, we make it useless instead.

				ButtonPlay.IsEnabled = false;
				ButtonContinue.IsEnabled = false;
			}
		}

		private void VerifyEngine()
		{
			var engine = SudokuEngineProvider.Engine;

			if (engine == null) throw new LoadEngineException("Engine not set.");

			int test = SudokuEngineProvider.Engine.Test();

			if (test != 42)
				throw new LoadEngineException("Engine returned wrong result: " + test);
		}

		private void OnPlay(object sender, EventArgs e)
		{
			App.Current.MainPage = new PlayOptionsPage();
		}

		private void OnContinue(object sender, EventArgs e)
        {
			App.Current.MainPage = new PlayPage(SaveManager.LoadCurrent());
        }

		private void OnSettings(object sender, EventArgs e)
        {
			App.Current.MainPage = new SettingsPage();
        }
	}
}
