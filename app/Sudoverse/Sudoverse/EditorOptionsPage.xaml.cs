using Sudoverse.SudokuModel;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditorOptionsPage : ContentPage
    {
        public EditorOptionsPage()
        {
            InitializeComponent();
        }

        private void OnBack(object sender, EventArgs e)
        {
            App.Current.MainPage = new MainPage();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            var pencilmarkType = PencilmarkType.NoPencilmarkType;
            App.Current.MainPage = new LoadPage(this,
                (sudoku, name) => new EditorPage(sudoku, name), pencilmarkType, false);
        }

        private void OnEdit(object sender, EventArgs e)
        {
            App.Current.MainPage =
                new EditorPage(ConstraintSelector.ConstructSelectedConstraint());
        }
    }
}
