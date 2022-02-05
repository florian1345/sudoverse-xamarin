using Xamarin.Forms;

[assembly: ExportFont("OpenSans-Regular.ttf", Alias = "DigitFont")]
namespace Sudoverse
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart() { }

        protected override void OnSleep()
        {
            if (MainPage is PlayPage playPage)
                playPage.SaveCurrent();
        }

        protected override void OnResume() { }
    }
}
