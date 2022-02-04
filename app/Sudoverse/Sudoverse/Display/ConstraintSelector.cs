using System;

using Xamarin.Forms;

namespace Sudoverse.Display
{
    public sealed class ConstraintSelector : Layout<View>
    {
        public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create(nameof(ImageSource), typeof(string),
                typeof(ConstraintSelector), propertyChanged: OnImageSourceChanged);

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(ConstraintSelector),
                propertyChanged: OnTitleChanged);

        public static readonly BindableProperty DescriptionProperty =
            BindableProperty.Create(nameof(Description), typeof(string),
                typeof(ConstraintSelector), propertyChanged: OnDescriptionChanged);

        public static readonly BindableProperty ActiveProperty =
            BindableProperty.Create(nameof(Active), typeof(bool), typeof(ConstraintSelector),
                false, propertyChanged: OnActiveChanged);

        private static void OnImageSourceChanged(BindableObject bindable, object _, object value)
        {
            ((ConstraintSelector)bindable).image.Source = (string)value;
        }

        private static void OnTitleChanged(BindableObject bindable, object _, object value)
        {
            ((ConstraintSelector)bindable).title.Text = (string)value;
        }

        private static void OnDescriptionChanged(BindableObject bindable, object _, object value)
        {
            ((ConstraintSelector)bindable).description.Text = (string)value;
        }

        private static void OnActiveChanged(BindableObject bindable, object _, object value)
        {
            bool bValue = (bool)value;
            var constraintSelector = (ConstraintSelector)bindable;

            if (bValue) constraintSelector.frame.BackgroundColor = SelectedColor;
            else constraintSelector.frame.BackgroundColor = UnselectedColor;

            constraintSelector.ActiveChanged?.Invoke(bindable, bValue);
        }

        private static readonly Color UnselectedColor = Color.White;
        private static readonly Color SelectedColor = Color.FromRgb(160, 160, 240);

        // Metrics are a proportion of the height.

        private const double MARGIN = 0.05;
        private const double TITLE_HEIGHT = 0.3;
        private const double TITLE_SEPARATION = 0.00;

        private Frame frame;
        private Image image;
        private Label title;
        private Label description;

        public string ImageSource
        {
            get => (string)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public bool Active
        {
            get => (bool)GetValue(ActiveProperty);
            set => SetValue(ActiveProperty, value);
        }

        public event EventHandler<bool> ActiveChanged;

        public ConstraintSelector()
        {
            frame = new Frame()
            {
                BackgroundColor = UnselectedColor,
                BorderColor = Color.Black
            };
            image = new Image()
            {
                Aspect = Aspect.AspectFit,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            title = new Label()
            {
                FontSize = Device.GetNamedSize(NamedSize.Title, this)
            };
            description = new Label()
            {
                FontSize = Device.GetNamedSize(NamedSize.Body, this)
            };

            Children.Add(frame);
            Children.Add(image);
            Children.Add(title);
            Children.Add(description);

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnTapped;
            GestureRecognizers.Add(tapGestureRecognizer);
        }

        private void OnTapped(object sender, EventArgs e)
        {
            Active = true;
        }

        private void LayoutChild(View child, double x, double y, double width, double height)
        {
            LayoutChildIntoBoundingRegion(child, new Rectangle(x, y, width, height));
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            LayoutChild(frame, x, y, width, height);

            double margin = MARGIN * height;
            double imageX = x + margin;
            double imageY = y + margin;
            double imageSize = height - 2 * margin;

            LayoutChild(image, imageX, imageY, imageSize, imageSize);

            double titleX = imageX + imageSize + margin;
            double titleY = imageY;
            double titleWidth = x + width - titleX - margin;
            double titleHeight = height * TITLE_HEIGHT;

            LayoutChild(title, titleX, titleY, titleWidth, titleHeight);

            double descriptionX = titleX;
            double descriptionY = titleY + titleHeight + height * TITLE_SEPARATION;
            double descriptionWidth = titleWidth;
            double descriptionHeight = y + height - descriptionY - margin;

            LayoutChild(description, descriptionX, descriptionY, descriptionWidth,
                descriptionHeight);
        }
    }
}
