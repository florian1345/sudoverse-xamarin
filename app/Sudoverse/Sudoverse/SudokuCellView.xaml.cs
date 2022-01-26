using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SudokuCellView : ContentView
    {
        public event EventHandler Tapped;

        public SudokuCellView()
        {
            InitializeComponent();
        }

        public void Select()
        {
            Background.Color = Color.LightBlue;
        }

        public void Deselect()
        {
            Background.Color = Color.White;
        }

        public void Enter(int digit)
        {
            Label.Text = digit.ToString();
        }

        public void Clear()
        {
            Label.Text = "";
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
