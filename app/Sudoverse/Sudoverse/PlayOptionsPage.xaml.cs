using Sudoverse.Constraint;
using Sudoverse.Engine;
using Sudoverse.SudokuModel;

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

        private void OnStart(object sender, EventArgs e)
        {
            int difficulty = DifficultySlider.Difficulty;
            string json = SudokuEngineProvider.Engine.GenDefault(difficulty);
            Sudoku sudoku = Sudoku.ParseJson<StatelessConstraint>(json);
            App.Current.MainPage = new PlayPage(sudoku);
        }
    }
}