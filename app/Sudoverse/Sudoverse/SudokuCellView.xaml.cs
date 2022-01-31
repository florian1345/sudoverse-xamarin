using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SudokuCellView : ContentView
    {
        private static readonly Color LockedColor = Color.Black;
        private static readonly Color UnlockedColor = Color.Blue;

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
            Background.Color = Color.LightBlue;
        }

        public void Deselect()
        {
            Background.Color = Color.White;
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
