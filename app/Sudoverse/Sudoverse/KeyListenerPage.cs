using System;

using Xamarin.Forms;

namespace Sudoverse
{
    /// <summary>
    /// An enumeration of all (relevant) keys. They are simplified to equivalence classes of
    /// identical behavior in the app (e.g. the "1" key and "NumPad1" key are collapsed to
    /// "Digit1").
    /// </summary>
    public enum Key
    {
        /// <summary>
        /// The "1" or "NumPad1" key.
        /// </summary>
        Digit1,

        /// <summary>
        /// The "2" or "NumPad2" key.
        /// </summary>
        Digit2,

        /// <summary>
        /// The "3" or "NumPad3" key.
        /// </summary>
        Digit3,

        /// <summary>
        /// The "4" or "NumPad4" key.
        /// </summary>
        Digit4,

        /// <summary>
        /// The "5" or "NumPad5" key.
        /// </summary>
        Digit5,

        /// <summary>
        /// The "6" or "NumPad6" key.
        /// </summary>
        Digit6,

        /// <summary>
        /// The "7" or "NumPad7" key.
        /// </summary>
        Digit7,

        /// <summary>
        /// The "8" or "NumPad8" key.
        /// </summary>
        Digit8,

        /// <summary>
        /// The "9" or "NumPad9" key.
        /// </summary>
        Digit9,

        /// <summary>
        /// The "Backspace" or "Delete" key.
        /// </summary>
        Delete
    }

    /// <summary>
    /// Event arguments for a key event.
    /// </summary>
    public sealed class KeyEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="Sudoverse.Key"/> with which was interacted.
        /// </summary>
        public Key Key { get; }

        /// <summary>
        /// Creates new key event args from the given key.
        /// </summary>
        /// <param name="key">The <see cref="Sudoverse.Key"/> with which was interacted.</param>
        public KeyEventArgs(Key key)
            : base()
        {
            Key = key;
        }
    }

    /// <summary>
    /// A <see cref="ContentPage"/> that listens for key pressed events.
    /// </summary>
    public abstract class KeyListenerPage : ContentPage
    {
        /// <summary>
        /// This event is raised whenever <see cref="SendKeyPressed(object, KeyEventArgs)"/> is
        /// called, which is the responsibility of the specific platform projects.
        /// </summary>
        protected event EventHandler<KeyEventArgs> KeyPressed;

        protected KeyListenerPage() { }

        /// <summary>
        /// Raises a key pressed event with the given arguments.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> which provide additional information
        /// about the key event.</param>
        public void SendKeyPressed(object sender, KeyEventArgs e)
        {
            KeyPressed?.Invoke(sender, e);
        }
    }
}
