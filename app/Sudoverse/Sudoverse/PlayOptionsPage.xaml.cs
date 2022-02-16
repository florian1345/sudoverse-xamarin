using Sudoverse.Display;
using Sudoverse.Engine;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlayOptionsPage : ContentPage
    {
        public PlayOptionsPage()
        {
            InitializeComponent();
        }

        private void OnBack(object sender, EventArgs e)
        {
            App.Current.MainPage = new MainPage();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            var pencilmarkType = Config.PencilmarkType;
            App.Current.MainPage =
                new LoadPage(this, (sudoku, _) => new PlayPage(sudoku), pencilmarkType, true);
        }

        private void OnStart(object sender, EventArgs e)
        {
            int constraint = ConstraintSelector.SelectedConstraintId;
            int difficulty = DifficultySlider.Difficulty;
            var pencilmarkType = Config.PencilmarkType;
            var sudoku = SudokuEngineProvider.Engine.Gen(constraint, difficulty, pencilmarkType);
            App.Current.MainPage = new PlayPage(sudoku);
        }
    }
}
