using System;
using System.Text.RegularExpressions;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse.Display
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FrameNumberEditView : ContentView
    {
        private int? number;

        /// <summary>
        /// An event raised whenever <see cref="Number"/> changed.
        /// </summary>
        public event EventHandler NumberChanged;

        /// <summary>
        /// An event raised whenever this view is selected by the user.
        /// </summary>
        public event EventHandler Focused;

        /// <summary>
        /// The number currently input in this edit view, or <tt>null</tt> if it is empty.
        /// </summary>
        public int? Number
        {
            get => number;
            set
            {
                if (value != number)
                {
                    number = value;
                    EntryNumber.Text = number == -1 ? "" : number.ToString();
                    NumberChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        public FrameNumberEditView()
        {
            InitializeComponent();
            number = null;
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            var text = EntryNumber.Text;

            if (Regex.IsMatch(text, @"^[0-9]*?$"))
            {
                if (text.Length == 0) Number = null;
                else Number = int.Parse(text);
            }
            else if (Number == null) EntryNumber.Text = "";
            else EntryNumber.Text = Number.ToString();
        }

        private void OnFocused(object sender, EventArgs e)
        {
            Focused?.Invoke(sender, e);
        }
    }
}
