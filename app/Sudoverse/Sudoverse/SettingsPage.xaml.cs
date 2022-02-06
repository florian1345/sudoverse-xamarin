using Sudoverse.SudokuModel;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private static PencilmarkType[] PencilmarkTypes =
        {
            PencilmarkType.CenterBorderPencilmarkType,
            PencilmarkType.PositionalPencilmarkType
        };

        public SettingsPage()
        {
            InitializeComponent();
            PencilmarkTypePicker.SelectedIndex =
                Array.IndexOf(PencilmarkTypes, Config.PencilmarkType);
        }

        private void Back()
        {
            App.Current.MainPage = new MainPage();
        }

        private void OnAccept(object sender, EventArgs e)
        {
            Config.PencilmarkType = PencilmarkTypes[PencilmarkTypePicker.SelectedIndex];
            Back();
        }

        private void OnAbort(object sender, EventArgs e)
        {
            Back();
        }
    }
}
