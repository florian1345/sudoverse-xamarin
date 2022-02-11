using Sudoverse.SudokuModel;
using System;
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

        private static readonly Color UnselectedColor = Color.Transparent;
        private static readonly Color SelectedColor = Color.FromRgba(0, 0, 1, 0.1);
        private static readonly Color InvalidColor = Color.FromRgba(1, 0, 0, 0.1);

        public static readonly Color[] BackgroundColors = new Color[]
        {
            Color.Transparent,
            Color.FromRgba(1.0, 0.0, 0.0, 0.2),
            Color.FromRgba(1.0, 0.5, 0.0, 0.2),
            Color.FromRgba(1.0, 1.0, 0.0, 0.2),
            Color.FromRgba(0.0, 1.0, 0.0, 0.2),
            Color.FromRgba(0.0, 1.0, 1.0, 0.2),
            Color.FromRgba(0.0, 0.0, 1.0, 0.2),
            Color.FromRgba(1.0, 0.0, 1.0, 0.2),
            Color.FromRgba(0.0, 0.0, 0.0, 0.2)
        };

        private double fontSize;
        private SudokuCell cell;
        private Label[] borderLabels;
        private Color stateBackgroundColor;

        /// <summary>
        /// Gets the current big digit entered in this cell, or 0 if it is empty.
        /// </summary>
        public int Digit => cell.Digit;

        public SudokuCellView(SudokuCell cell)
        {
            InitializeComponent();
            cell.Updated += (s, e) =>
            {
                UpdateText();
                UpdateColor();
            };
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
            stateBackgroundColor = UnselectedColor;

            LabelCenter.TextColor = cell.Locked ? LockedColor : UnlockedColor;

            foreach (var label in borderLabels)
                label.TextColor = UnlockedColor;

            // Note: UpdateText would be redundant here, as it is called again on layout
            UpdateColor();
        }

        private void UpdateText()
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

        private double MixChannel(double back, double backA, double front, double frontA) =>
            Math.Sqrt((back * back * backA + front * front * frontA) / (backA + frontA));

        private Color Mix(Color back, Color front)
        {
            var r = MixChannel(back.R, back.A, front.R, front.A);
            var g = MixChannel(back.G, back.A, front.G, front.A);
            var b = MixChannel(back.B, back.A, front.B, front.A);
            var a = 1.0 - (1.0 - back.A) * (1.0 - front.A);
            return Color.FromRgba(r, g, b, a);
        }

        private void UpdateColor()
        {
            var selectedBackgroundColor = BackgroundColors[cell.ColorIndex - 1];
            Background.Color = Mix(selectedBackgroundColor, stateBackgroundColor);
        }

        public void Select()
        {
            stateBackgroundColor = SelectedColor;
            UpdateColor();
        }

        public void Deselect()
        {
            stateBackgroundColor = UnselectedColor;
            UpdateColor();
        }

        public void SetInvalid()
        {
            stateBackgroundColor = InvalidColor;
            UpdateColor();
        }

        public void SetFontSize(double fontSize)
        {
            this.fontSize = fontSize;
            var smallFontSize = SMALL_FONT_FACTOR * fontSize;
            
            foreach (var label in borderLabels)
                label.FontSize = smallFontSize;

            UpdateText();
        }
    }
}
