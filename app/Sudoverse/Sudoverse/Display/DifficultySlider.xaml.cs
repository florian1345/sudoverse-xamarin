using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse.Display
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DifficultySlider : ContentView
    {
        private int difficulty;
        private DifficultySliderStar[] stars;

        public int Difficulty
        {
            get => difficulty;
            set
            {
                if (value < 1 || value > 5)
                    throw new ArgumentException("Difficulty must be in the range [1, 5].");

                difficulty = value;
                UpdateStars();
            }
        }

        public DifficultySlider()
        {
            InitializeComponent();

            stars = new DifficultySliderStar[]
            {
                Star1,
                Star2,
                Star3,
                Star4,
                Star5
            };

            Difficulty = 5;
        }

        private void UpdateStars()
        {
            if (stars == null) return;

            for (int i = 0; i < Difficulty; i++)
            {
                stars[i].On = true;
            }

            for (int i = Difficulty; i < stars.Length; i++)
            {
                stars[i].On = false;
            }
        }

        private void OnStar1Tapped(object sender, EventArgs e)
        {
            Difficulty = 1;
        }

        private void OnStar2Tapped(object sender, EventArgs e)
        {
            Difficulty = 2;
        }

        private void OnStar3Tapped(object sender, EventArgs e)
        {
            Difficulty = 3;
        }

        private void OnStar4Tapped(object sender, EventArgs e)
        {
            Difficulty = 4;
        }

        private void OnStar5Tapped(object sender, EventArgs e)
        {
            Difficulty = 5;
        }
    }
}
