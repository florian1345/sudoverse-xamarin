using Sudoverse.Constraint;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse.Display
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConstraintSelector : ContentView
    {
        private sealed class ConstraintConfig
        {
            private Func<IConstraint> constructor;

            public string ImageSource { get; }
            public string Title { get; }
            public string Description { get; }
            public int ConstraintId { get; }

            public ConstraintConfig(string imageSource, string title, string description,
                int constraintId, Func<IConstraint> constructor)
            {
                ImageSource = imageSource;
                Title = title;
                Description = description;
                ConstraintId = constraintId;
                this.constructor = constructor;
            }

            public IConstraint Construct() => constructor();
        }

        private static readonly ConstraintConfig[] ConstraintConfigs =
        {
            new ConstraintConfig("rules_classic.png", "Classic Sudoku",
                "Each digit must appear exactly once in every row, column, and box.", 0,
                () => new StatelessConstraint("default")),
            new ConstraintConfig("rules_diagonal.png", "Diagonals Sudoku",
                "In addition to classic rules, each digit must appear exactly once on both " +
                "diagonals.", 1, () => new CompositeConstraint(
                    new StatelessConstraint("default"),
                    new StatelessConstraint("diagonals"))),
            new ConstraintConfig("rules_knights_move.png", "Knight's Move Sudoku",
                "In addition to classic rules, cells removed by a Chess knight's move must not " +
                "contain the same digit.", 2, () => new CompositeConstraint(
                    new StatelessConstraint("default"),
                    new StatelessConstraint("knights-move"))),
            new ConstraintConfig("rules_kings_move.png", "King's Move Sudoku",
                "In addition to classic rules, diagonally adjacent cells must not contain the " +
                "same digit.", 3, () => new CompositeConstraint(
                    new StatelessConstraint("default"),
                    new StatelessConstraint("kings-move"))),
            new ConstraintConfig("rules_chess.png", "Chess Sudoku",
                "A combination of Knight's Move and King's Move Sudoku.", 4,
                () => new CompositeConstraint(
                    new StatelessConstraint("default"),
                    new StatelessConstraint("knights-move"),
                    new StatelessConstraint("kings-move"))),
            new ConstraintConfig("rules_sandwich.png", "Sandwich Sudoku",
                "In addition to classic rules, digits located between 1 and 9 in their row or " +
                "column must add to the number outside the grid.", 5,
                () => new CompositeConstraint(
                    new StatelessConstraint("default"),
                    new SandwichConstraint(9))) // TODO make more general
        };

        // TODO find a more dynamic solution
        private const double BUTTON_HEIGHT_REQUEST = 128;

        private ConstraintConfig selectedConfig;
        private ConstraintSelectorButton selectedButton;

        public int SelectedConstraintId => selectedConfig.ConstraintId;

        public ConstraintSelector()
        {
            InitializeComponent();

            selectedConfig = ConstraintConfigs[0];

            foreach (var constraintConfig in ConstraintConfigs)
            {
                var selector = new ConstraintSelectorButton()
                {
                    ImageSource = constraintConfig.ImageSource,
                    Title = constraintConfig.Title,
                    Description = constraintConfig.Description,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    HeightRequest = BUTTON_HEIGHT_REQUEST
                };

                if (constraintConfig == selectedConfig)
                {
                    selector.Active = true;
                    selectedButton = selector;
                }

                selector.ActiveChanged += (_, active) =>
                {
                    if (active)
                    {
                        selectedButton.Active = false;
                        selectedConfig = constraintConfig;
                        selectedButton = selector;
                    }
                };

                ConstraintList.Children.Add(selector);
            }
        }

        public IConstraint ConstructSelectedConstraint() => selectedConfig.Construct();
    }
}
