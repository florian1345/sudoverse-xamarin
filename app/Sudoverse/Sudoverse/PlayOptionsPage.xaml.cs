using Sudoverse.Display;
using Sudoverse.Engine;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoverse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlayOptionsPage : ContentPage
    {
        private sealed class RuleConfig
        {
            public string ImageSource { get; }
            public string Title { get; }
            public string Description { get; }
            public int ConstraintId { get; }

            public RuleConfig(string imageSource, string title, string description,
                int constraintId)
            {
                ImageSource = imageSource;
                Title = title;
                Description = description;
                ConstraintId = constraintId;
            }
        }

        private static readonly RuleConfig[] RuleConfigs =
        {
            new RuleConfig("rules_classic.png", "Classic Sudoku",
                "Each digit must appear exactly once in every row, column, and box.", 0),
            new RuleConfig("rules_diagonal.png", "Diagonals Sudoku",
                "In addition to classic rules, each digit must appear exactly once on both " +
                "diagonals.", 1),
            new RuleConfig("rules_knights_move.png", "Knight's Move Sudoku",
                "In addition to classic rules, cells removed by a Chess knight's move must not " +
                "contain the same digit.", 2),
            new RuleConfig("rules_kings_move.png", "King's Move Sudoku",
                "In addition to classic rules, diagonally adjacent cells must not contain the " +
                "same digit.", 3),
            new RuleConfig("rules_chess.png", "Chess Sudoku",
                "A combination of Knight's Move and King's Move Sudoku.", 4)
        };

        // TODO find a more dynamic solution
        private const double CONSTRAINT_SELECTOR_HEIGHT_REQUEST = 128;

        private RuleConfig selectedRuleConfig;
        private ConstraintSelector selectedConstraintSelector;

        public PlayOptionsPage()
        {
            InitializeComponent();

            selectedRuleConfig = RuleConfigs[0];

            foreach (var ruleConfig in RuleConfigs)
            {
                var selector = new ConstraintSelector()
                {
                    ImageSource = ruleConfig.ImageSource,
                    Title = ruleConfig.Title,
                    Description = ruleConfig.Description,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    HeightRequest = CONSTRAINT_SELECTOR_HEIGHT_REQUEST
                };

                if (ruleConfig == selectedRuleConfig)
                {
                    selector.Active = true;
                    selectedConstraintSelector = selector;
                }

                selector.ActiveChanged += (_, active) =>
                {
                    if (active)
                    {
                        selectedConstraintSelector.Active = false;
                        selectedRuleConfig = ruleConfig;
                        selectedConstraintSelector = selector;
                    }
                };

                RuleList.Children.Add(selector);
            }
        }

        private void OnBack(object sender, EventArgs e)
        {
            App.Current.MainPage = new MainPage();
        }

        private void OnStart(object sender, EventArgs e)
        {
            int constraint = selectedRuleConfig.ConstraintId;
            int difficulty = DifficultySlider.Difficulty;
            var pencilmarkType = Config.PencilmarkType;
            var sudoku = SudokuEngineProvider.Engine.Gen(constraint, difficulty, pencilmarkType);
            App.Current.MainPage = new PlayPage(sudoku);
        }
    }
}
