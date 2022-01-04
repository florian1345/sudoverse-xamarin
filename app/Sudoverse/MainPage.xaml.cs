using System;
using Microsoft.Maui.Controls;

namespace Sudoverse
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private void OnEditor(object sender, EventArgs e)
		{
			App.Current.MainPage = new EditorPage();
		}
	}
}
