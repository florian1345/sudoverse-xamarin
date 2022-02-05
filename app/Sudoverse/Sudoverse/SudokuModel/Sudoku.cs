using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sudoverse.Constraint;
using System;
using System.Linq;

namespace Sudoverse.SudokuModel
{
    public sealed class Sudoku
    {
        public int BlockWidth { get; private set; }
        public int BlockHeight { get; private set; }
        public int Size { get; private set; }
        public IConstraint Constraint { get; private set; }

        /// <summary>
        /// Indicates whether this Sudoku is full, i.e. all cells contain a big digit.
        /// </summary>
        public bool Full => cells.All(c => c.Filled);

        private SudokuCell[] cells;

        public Sudoku(int blockWidth, int blockHeight, IConstraint constraint)
        {
            if (blockWidth <= 0 || blockHeight <= 0)
                throw new ArgumentException("Block width and height must be positive.");

            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
            Size = BlockWidth * BlockHeight;
            Constraint = constraint;
            cells = new SudokuCell[Size * Size];

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new SudokuCell();
            }
        }

        public SudokuCell GetCell(int column, int row) =>
            cells[row * Size + column];

        /// <summary>
        /// Enters the given digit at the given location using the given notation. Small and corner
        /// digits are ignored if a normal digit is present. The inverse operation is returned (may
        /// be nop).
        /// </summary>
        public Operation EnterCell(int column, int row, int digit, Notation notation)
        {
            var cell = cells[row * Size + column];

            switch (notation)
            {
                case Notation.Normal:
                    if (digit != cell.Digit)
                    {
                        int oldDigit = cell.Digit;

                        if (cell.EnterNormal(digit))
                        {
                            if (oldDigit == 0)
                                return new ClearOperation(column, row);
                            else return new EnterOperation(column, row, oldDigit, notation);
                        }
                    }

                    break;
                case Notation.Small:
                    if (!cell.Filled && cell.ToggleSmall(digit))
                        return new EnterOperation(column, row, digit, notation);

                    break;
                case Notation.Corner:
                    if (!cell.Filled && cell.ToggleCorner(digit))
                        return new EnterOperation(column, row, digit, notation);

                    break;
            }

            return new NoOperation();
        }

        public Operation ClearCell(int column, int row)
        {
            var cell = GetCell(column, row);

            if (cell.Filled)
            {
                var digit = cell.Digit;
                if (!cell.Clear()) return new NoOperation();
                return new EnterOperation(column, row, digit, Notation.Normal);
            }
            else
            {
                var inverseOperations = cell.SmallDigits
                    .Select(d => new EnterOperation(column, row, d, Notation.Small))
                    .Concat(cell.CornerDigits
                        .Select(d => new EnterOperation(column, row, d, Notation.Corner)))
                    .ToArray();
                cell.Clear();
                return new CompositeOperation(inverseOperations);
            }
        }

        private string ToJsonWith(Func<SudokuCell, JToken> cellConverter)
        {
            var cellsJson = new JArray();

            foreach (var cell in cells)
            {
                cellsJson.Add(cellConverter(cell));
            }

            var grid = new JObject();
            grid.Add("block_width", BlockWidth);
            grid.Add("block_height", BlockHeight);
            grid.Add("cells", cellsJson);

            var sudoku = new JObject();
            sudoku.Add("grid", grid);
            sudoku.Add("constraint", ConstraintUtil.ToJson(Constraint));
            return JsonConvert.SerializeObject(sudoku);
        }

        /// <summary>
        /// Converts the rough structure of this Sudoku into JSON. Only cells containing a big
        /// digit are transcribed, and all state except that digit is disregarded. This is used for
        /// communication with the engine.
        /// </summary>
        public string ToJson()
        {
            return ToJsonWith(cell =>
            {
                int digit = cell.Digit;

                if (digit == 0) return JValue.CreateNull();
                else return digit;
            });
        }

        /// <summary>
        /// Converts the full state of this Sudoku into JSON. This includes all digits,
        /// annotations, and the lock status of each cell. This is used for savegames.
        /// </summary>
        public string ToFullJson() =>
            ToJsonWith(cell => cell.ToJson());

        private static Sudoku ParseJsonWith(string json, Func<JToken, SudokuCell> cellParser)
        {
            var sudokuToken = JToken.Parse(json);

            if (!(sudokuToken is JObject sudokuObject))
                throw new ParseSudokuException();

            if (!sudokuObject.TryGetValue("constraint", out JToken constraintToken))
                throw new ParseSudokuException();

            var constraint = ConstraintUtil.FromJson(constraintToken);
            var grid = sudokuObject.GetField<JObject>("grid");
            int blockWidth = grid.GetField<JValue>("block_width").ToInt();
            int blockHeight = grid.GetField<JValue>("block_height").ToInt();
            Sudoku result = new Sudoku(blockWidth, blockHeight, constraint);
            var jsonCells = grid.GetField<JArray>("cells");

            SudokuCell[] cells = new SudokuCell[jsonCells.Count];

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = cellParser(jsonCells[i]);
            }

            if (cells.Length != result.cells.Length)
                throw new ParseSudokuException();

            result.cells = cells;
            return result;
        }

        /// <summary>
        /// Parses the rough stucture of a Sudoku from JSON. Only cells containing a big
        /// digit are transcribed, and all state except that digit is disregarded. This is used for
        /// communication with the engine.
        /// </summary>
        public static Sudoku ParseJson(string json) =>
            ParseJsonWith(json, token => new SudokuCell(token.ToInt()));

        /// <summary>
        /// Parses the full state of this Sudoku from JSON. This includes all digits, annotations,
        /// and the lock status of each cell. This is used for savegames.
        /// </summary>
        public static Sudoku ParseFullJson(string json) =>
            ParseJsonWith(json, token => SudokuCell.ParseJson(token));
    }
}
