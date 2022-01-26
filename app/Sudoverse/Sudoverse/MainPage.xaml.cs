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

				ButtonEditor.IsEnabled = false;
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

		private void OnEditor(object sender, EventArgs e)
		{
			App.Current.MainPage = new EditorPage();
		}
	}
}
