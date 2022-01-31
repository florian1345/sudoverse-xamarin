using Xamarin.Forms;

namespace Sudoverse.Util
{
    public class ReadOnlyRect
    {
        private Rect rect;

        /// <summary>
        /// See <see cref="Rect.X"/>.
        /// </summary>
        public double X { get => rect.X; }

        /// <summary>
        /// See <see cref="Rect.Y"/>.
        /// </summary>
        public double Y { get => rect.Y; }

        /// <summary>
        /// See <see cref="Rect.Width"/>.
        /// </summary>
        public double Width { get => rect.Width; }

        /// <summary>
        /// See <see cref="Rect.Height"/>.
        /// </summary>
        public double Height { get => rect.Height; }

        /// <summary>
        /// See <see cref="Rect.Left"/>.
        /// </summary>
        public double Left { get => rect.Left; }

        /// <summary>
        /// See <see cref="Rect.Right"/>.
        /// </summary>
        public double Right { get => rect.Right; }

        /// <summary>
        /// See <see cref="Rect.Top"/>.
        /// </summary>
        public double Top { get => rect.Top; }

        /// <summary>
        /// See <see cref="Rect.Bottom"/>.
        /// </summary>
        public double Bottom { get => rect.Bottom; }

        /// <summary>
        /// Creates a readonly wrapper around the given rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to wrap.</param>
        public ReadOnlyRect(Rect rect)
        {
            this.rect = rect;
        }
    }
}
