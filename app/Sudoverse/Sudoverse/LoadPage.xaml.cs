using Sudoverse.SudokuModel;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadPage : ContentPage
    {
        // TODO do this a bit smarter, something like a NavigationPage
        private ContentPage previous;
        private Func<Sudoku, string, ContentPage> next;
        private PencilmarkType pencilmarkType;
        private bool locked;

        public LoadPage(ContentPage previous, Func<Sudoku, string, ContentPage> next,
            PencilmarkType pencilmarkType, bool locked)
        {
            InitializeComponent();

            this.previous = previous;
            this.next = next;
            this.pencilmarkType = pencilmarkType;
            this.locked = locked;
            FileList.ItemsSource = SaveManager.ListPuzzles();
        }

        private void OnBack(object sender, EventArgs e)
        {
            App.Current.MainPage = previous;
        }

        private async void OnLoad(object sender, EventArgs e)
        {
            var name = (string)FileList.SelectedItem;

            if (name == null)
            {
                await DisplayAlert("Error", "Please select a Sudoku to load.", "Ok");
                return;
            }

            var sudoku = SaveManager.LoadPuzzle(name, pencilmarkType, locked);
            App.Current.MainPage = next(sudoku, name);
        }
    }
}
