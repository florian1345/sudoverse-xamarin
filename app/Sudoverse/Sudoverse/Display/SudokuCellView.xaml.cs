using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse.Display
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SudokuCellView : ContentView
    {
        private static readonly Color LockedColor = Color.Black;
        private static readonly Color UnlockedColor = Color.Blue;

        private static readonly Color SelectedColor = Color.FromRgba(0, 0, 1, 0.1);
        private static readonly Color UnselectedColor = Color.Transparent;

        public event EventHandler Tapped;

        public bool Locked { get; private set; }

        public SudokuCellView()
        {
            InitializeComponent();
            Label.TextColor = UnlockedColor;
        }

        public void Lock()
        {
            Locked = true;
            Label.TextColor = LockedColor;
        }

        public void Select()
        {
            Background.Color = SelectedColor;
        }

        public void Deselect()
        {
            Background.Color = UnselectedColor;
        }

        /// <summary>
        /// Attempty to enter the digit and returns true if and only if it was successful (i.e. the
        /// cell was not locked).
        /// </summary>
        public bool Enter(int digit)
        {
            if (Locked) return false;
            Label.Text = digit.ToString();
            return true;
        }

        /// <summary>
        /// Attempty to clear this cell and returns true if and only if it was successful (i.e. the
        /// cell was not locked).
        /// </summary>
        public bool Clear()
        {
            if (Locked) return false;
            Label.Text = "";
            return true;
        }

        public void SetFontSize(double fontSize)
        {
            Label.FontSize = fontSize;
        }

        private void OnTapped(object sender, EventArgs e)
        {
            Tapped?.Invoke(sender, e);
        }
    }
}
