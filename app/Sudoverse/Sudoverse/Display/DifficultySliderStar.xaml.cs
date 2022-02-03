using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse.Display
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DifficultySliderStar : ContentView
    {
        public static readonly BindableProperty OnProperty =
            BindableProperty.Create(nameof(On), typeof(bool), typeof(DifficultySliderStar), true);

        public bool On
        {
            get => (bool)GetValue(OnProperty);
            set
            {
                if (value != On)
                    Image.Source = value ? "star_on.png" : "star_off.png";

                SetValue(OnProperty, value);
            }
        }

        public DifficultySliderStar()
        {
            InitializeComponent();
        }
    }
}
