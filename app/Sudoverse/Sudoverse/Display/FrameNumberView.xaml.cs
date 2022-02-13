using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse.Display
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FrameNumberView : ContentView
    {
        private const double FONT_SIZE_FACTOR = 0.4;

        public FrameNumberView()
        {
            InitializeComponent();
            LayoutChanged += OnLayoutChanged;
        }

        public void DisplayNumber(int number)
        {
            LabelNumber.Text = number.ToString();
        }

        public void ClearNumber()
        {
            LabelNumber.Text = "";
        }

        private void OnLayoutChanged(object sender, EventArgs e)
        {
            LabelNumber.FontSize = Height * FONT_SIZE_FACTOR;
        }
    }
}
