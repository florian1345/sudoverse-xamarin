using Sudoverse;
using Sudoverse.UWP;
using Xamarin.Forms.Platform.UWP;
using Windows.System;
using Windows.UI.Core;
using WindowsKeyEventArgs = Windows.UI.Core.KeyEventArgs;

[assembly: ExportRenderer(typeof(KeyListenerPage), typeof(KeyListenerPageRenderer))]
namespace Sudoverse.UWP
{
    /// <summary>
    /// A <see cref="PageRenderer"/> for all content pages deriving from
    /// <see cref="KeyListenerPage"/>. It forwards key events to the page.
    /// </summary>
    public sealed class KeyListenerPageRenderer : PageRenderer
    {
        // Code derived from anonymous answers by User 181025 and User380833 on
        // https://social.msdn.microsoft.com/Forums/en-US/fc8d198a-2012-43a1-8062-5ff82973b06d/how-can-i-detect-key-events-in-xamarinforms?forum=xamarinforms

        public KeyListenerPageRenderer()
            : base()
        {
            Loaded += (sender, e) =>
            {
                CoreWindow.GetForCurrentThread().KeyDown += OnKeyDown;
                CoreWindow.GetForCurrentThread().KeyUp += OnKeyUp;
            };

            Unloaded += (sender, e) =>
            {
                CoreWindow.GetForCurrentThread().KeyDown -= OnKeyDown;
                CoreWindow.GetForCurrentThread().KeyUp -= OnKeyUp;
            };
        }

        private void OnKeyDown(CoreWindow sender, WindowsKeyEventArgs e)
        {
            var key = ToKey(e.VirtualKey);

            if (key != null)
                (Element as KeyListenerPage).SendKeyDown(sender, new KeyEventArgs((Key)key));
        }

        private void OnKeyUp(CoreWindow sender, WindowsKeyEventArgs e)
        {
            var key = ToKey(e.VirtualKey);

            if (key != null)
                (Element as KeyListenerPage).SendKeyUp(sender, new KeyEventArgs((Key)key));
        }

        private static Key? ToKey(VirtualKey vkey)
        {
            switch (vkey)
            {
                case VirtualKey.Number1:
                case VirtualKey.NumberPad1:
                    return Key.Digit1;
                case VirtualKey.Number2:
                case VirtualKey.NumberPad2:
                    return Key.Digit2;
                case VirtualKey.Number3:
                case VirtualKey.NumberPad3:
                    return Key.Digit3;
                case VirtualKey.Number4:
                case VirtualKey.NumberPad4:
                    return Key.Digit4;
                case VirtualKey.Number5:
                case VirtualKey.NumberPad5:
                    return Key.Digit5;
                case VirtualKey.Number6:
                case VirtualKey.NumberPad6:
                    return Key.Digit6;
                case VirtualKey.Number7:
                case VirtualKey.NumberPad7:
                    return Key.Digit7;
                case VirtualKey.Number8:
                case VirtualKey.NumberPad8:
                    return Key.Digit8;
                case VirtualKey.Number9:
                case VirtualKey.NumberPad9:
                    return Key.Digit9;
                case VirtualKey.Back:
                case VirtualKey.Delete:
                    return Key.Delete;
                case VirtualKey.Shift:
                case VirtualKey.LeftShift:
                case VirtualKey.RightShift:
                    return Key.Shift;
                case VirtualKey.Control:
                case VirtualKey.LeftControl:
                case VirtualKey.RightControl:
                    return Key.Control;
                default: return null;
            }
        }
    }
}
