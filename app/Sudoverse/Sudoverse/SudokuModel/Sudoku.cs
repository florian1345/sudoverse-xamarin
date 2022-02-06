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
        public PencilmarkType PencilmarkType { get; }

        /// <summary>
        /// Indicates whether this Sudoku is full, i.e. all cells contain a big digit.
        /// </summary>
        public bool Full => cells.All(c => c.Filled);

        private SudokuCell[] cells;

        public Sudoku(int blockWidth, int blockHeight, IConstraint constraint,
            PencilmarkType pencilmarkType)
        {
            if (blockWidth <= 0 || blockHeight <= 0)
                throw new ArgumentException("Block width and height must be positive.");

            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
            Size = BlockWidth * BlockHeight;
            Constraint = constraint;
            PencilmarkType = pencilmarkType;
            cells = new SudokuCell[Size * Size];

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new SudokuCell(PencilmarkType.Empty());
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
            var oldDigit = cell.Digit;

            if (cell.Enter(digit, notation))
            {
                switch (notation)
                {
                    case Notation.Normal:
                        if (oldDigit == 0)
                            return new ClearOperation(column, row);
                        else return new EnterOperation(column, row, oldDigit, notation);
                    default: return new EnterOperation(column, row, digit, notation);
                }
            }

            return new NoOperation();
        }

        public Operation ClearCell(int column, int row)
        {
            var cell = GetCell(column, row);
            var inverseOperations = cell.Clear()
                .Select(op =>
                    new EnterOperation(column, row, op.Item1, op.Item2))
                .ToArray();

            switch (inverseOperations.Length)
            {
                case 0: return new NoOperation();
                case 1: return inverseOperations[0];
                default: return new CompositeOperation(inverseOperations);
            }
        }

        private string ToJsonWith(Func<SudokuCell, JToken> cellConverter, bool addPencilmarkType)
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

            if (addPencilmarkType)
                sudoku.Add("pencilmark_type", PencilmarkType.GetIdentifier());

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
            }, false);
        }

        /// <summary>
        /// Converts the full state of this Sudoku into JSON. This includes all digits,
        /// annotations, and the lock status of each cell. This is used for savegames.
        /// </summary>
        public string ToFullJson() =>
            ToJsonWith(cell => cell.ToJson(), true);

        private static Sudoku ParseJsonWith(string json,
            Func<JToken, PencilmarkType, SudokuCell> cellParser, PencilmarkType pencilmarkType)
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

            if (pencilmarkType == null)
            {
                var idValue = sudokuObject.GetField<JValue>("pencilmark_type");

                if (idValue.Type != JTokenType.String)
                    throw new ParseSudokuException();

                pencilmarkType = PencilmarkType.FromIdentifier((string)idValue);
            }

            Sudoku result = new Sudoku(blockWidth, blockHeight, constraint, pencilmarkType);
            var jsonCells = grid.GetField<JArray>("cells");

            SudokuCell[] cells = new SudokuCell[jsonCells.Count];

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = cellParser(jsonCells[i], pencilmarkType);
            }

            if (cells.Length != result.cells.Length)
                throw new ParseSudokuException();

            result.cells = cells;
            return result;
        }

        /// <summary>
        /// Parses the rough stucture of a Sudoku from JSON. Only cells containing a big
        /// digit are transcribed, and all state except that digit is disregarded. This is used for
        /// communication with the engine. The pencilmark type must be provided, since it is not
        /// stored in the JSON data.
        /// </summary>
        public static Sudoku ParseJson(string json, PencilmarkType pencilmarkType) =>
            ParseJsonWith(json, (token, ptype) => new SudokuCell(ptype.Empty(), token.ToInt()),
                pencilmarkType);

        /// <summary>
        /// Parses the full state of this Sudoku from JSON. This includes all digits, annotations,
        /// and the lock status of each cell. This is used for savegames.
        /// </summary>
        public static Sudoku ParseFullJson(string json) =>
            ParseJsonWith(json, (token, ptype) => SudokuCell.ParseJson(token, ptype), null);
    }
}
