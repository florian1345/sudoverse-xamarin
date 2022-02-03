using Sudoverse.SudokuModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse.Display
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SudokuCellView : ContentView
    {
        private const double SMALL_FONT_FACTOR = 0.5;

        private static readonly Color LockedColor = Color.Black;
        private static readonly Color UnlockedColor = Color.Blue;

        private static readonly Color SelectedColor = Color.FromRgba(0, 0, 1, 0.1);
        private static readonly Color UnselectedColor = Color.Transparent;

        private double fontSize;
        private SudokuCell cell;
        private Label[] cornerLabels;

        public bool Locked { get; private set; }

        public SudokuCellView(SudokuCell cell)
        {
            InitializeComponent();
            LabelCenter.TextColor = UnlockedColor;
            cell.Updated += (s, e) => Update();
            this.cell = cell;
            cornerLabels = new Label[]
            {
                LabelTopLeft,
                LabelTopRight,
                LabelBottomLeft,
                LabelBottomRight
            };

            LabelCenter.TextColor = UnlockedColor;

            foreach (var label in cornerLabels)
                label.TextColor = UnlockedColor;
        }

        private void Update()
        {
            if (cell.Filled)
            {
                LabelCenter.FontSize = fontSize;
                LabelCenter.Text = cell.Digit.ToString();

                foreach (var label in cornerLabels)
                    label.Text = "";
            }
            else
            {
                LabelCenter.FontSize = SMALL_FONT_FACTOR * fontSize;
                LabelCenter.Text = string.Join("", cell.SmallDigits.Select(d => d.ToString()));
                int i = 0;

                foreach (int cornerDigit in cell.CornerDigits)
                {
                    cornerLabels[i].Text = cornerDigit.ToString();
                    i++;
                }

                for (; i < cornerLabels.Length; i++)
                {
                    cornerLabels[i].Text = "";
                }
            }
        }

        public void Lock()
        {
            Locked = true;
            LabelCenter.TextColor = LockedColor;
        }

        public void Select()
        {
            Background.Color = SelectedColor;
        }

        public void Deselect()
        {
            Background.Color = UnselectedColor;
        }

        public void SetFontSize(double fontSize)
        {
            this.fontSize = fontSize;
            var smallFontSize = SMALL_FONT_FACTOR * fontSize;
            
            foreach (var label in cornerLabels)
                label.FontSize = smallFontSize;

            Update();
        }
    }
}
