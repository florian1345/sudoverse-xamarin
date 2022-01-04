using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;

namespace Sudoverse
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			MainPage = new MainPage();
		}
	}
}
