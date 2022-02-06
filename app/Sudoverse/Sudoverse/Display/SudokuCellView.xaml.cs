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
        private Label[] borderLabels;

        /// <summary>
        /// Gets the current big digit entered in this cell, or 0 if it is empty.
        /// </summary>
        public int Digit => cell.Digit;

        public SudokuCellView(SudokuCell cell)
        {
            InitializeComponent();
            cell.Updated += (s, e) => Update();
            this.cell = cell;
            borderLabels = new Label[]
            {
                LabelTopLeft,
                LabelTopCenter,
                LabelTopRight,
                LabelCenterLeft,
                LabelCenterRight,
                LabelBottomLeft,
                LabelBottomCenter,
                LabelBottomRight
            };

            LabelCenter.TextColor = cell.Locked ? LockedColor : UnlockedColor;

            foreach (var label in borderLabels)
                label.TextColor = UnlockedColor;
        }

        private void Update()
        {
            if (cell.Filled)
            {
                LabelCenter.FontSize = fontSize;
                LabelCenter.Text = cell.Digit.ToString();

                foreach (var label in borderLabels)
                    label.Text = "";
            }
            else
            {
                LabelCenter.FontSize = SMALL_FONT_FACTOR * fontSize;
                LabelTopLeft.Text = cell.Pencilmark.TopLeft;
                LabelTopCenter.Text = cell.Pencilmark.TopCenter;
                LabelTopRight.Text = cell.Pencilmark.TopRight;
                LabelCenterLeft.Text = cell.Pencilmark.CenterLeft;
                LabelCenter.Text = cell.Pencilmark.Center;
                LabelCenterRight.Text = cell.Pencilmark.CenterRight;
                LabelBottomLeft.Text = cell.Pencilmark.BottomLeft;
                LabelBottomCenter.Text = cell.Pencilmark.BottomCenter;
                LabelBottomRight.Text = cell.Pencilmark.BottomRight;
            }
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
            
            foreach (var label in borderLabels)
                label.FontSize = smallFontSize;

            Update();
        }
    }
}
