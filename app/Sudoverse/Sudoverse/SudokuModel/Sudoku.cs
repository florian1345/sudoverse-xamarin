using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sudoverse.Constraint;
using Sudoverse.Util;
using System;
using System.Linq;

namespace Sudoverse.SudokuModel
{
    /// <summary>
    /// A Sudoku that can be manipulated. It stores <see cref="SudokuCell"/>s, which store the
    /// state of each of the cells in the grid.
    /// </summary>
    public sealed class Sudoku
    {
        /// <summary>
        /// The width of a single block of the Sudoku in number of cells. In an ordinary Sudoku,
        /// the blocks are 3x3, so this would be 3.
        /// </summary>
        public int BlockWidth { get; private set; }

        /// <summary>
        /// The height of a single block of the Sudoku in number of cells. In an ordinary Sudoku,
        /// the blocks are 3x3, so this would be 3.
        /// </summary>
        public int BlockHeight { get; private set; }

        /// <summary>
        /// The number of cells of the Sudoku along any one axis. As Sudoku are meant to be
        /// squares, this represents both its width and height.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// The <see cref="IConstraint"/> that applies to this Sudoku.
        /// </summary>
        public IConstraint Constraint { get; private set; }

        /// <summary>
        /// The <see cref="SudokuModel.PencilmarkType"/> that determines which style of
        /// pencilmarking the cells of this Sudoku support.
        /// </summary>
        public PencilmarkType PencilmarkType { get; }

        /// <summary>
        /// Indicates whether this Sudoku is full, i.e. all cells contain a big digit.
        /// </summary>
        public bool Full => cells.All(c => c.Filled);

        private SudokuCell[] cells;

        /// <summary>
        /// Creates a new, initially empty Sudoku with the given dimensions, constraint, and
        /// pencilmark type.
        /// </summary>
        /// <param name="blockWidth">The width of a single block of the Sudoku in number of cells.
        /// In an ordinary Sudoku, the blocks are 3x3, so this would be 3. Must be positive.
        /// </param>
        /// <param name="blockHeight">The height of a single block of the Sudoku in number of
        /// cells. In an ordinary Sudoku, the blocks are 3x3, so this would be 3. Must be positive.
        /// </param>
        /// <param name="constraint">The <see cref="IConstraint"/> that applies to the created
        /// Sudoku.</param>
        /// <param name="pencilmarkType">The <see cref="SudokuModel.PencilmarkType"/> that
        /// determines which style of pencilmarking the cells of the created Sudoku should support.
        /// </param>
        /// <exception cref="ArgumentException">If <tt>blockWidth</tt> or <tt>blockHeight</tt> are
        /// less than 1.</exception>
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

        /// <summary>
        /// Gets the <see cref="SudokuCell"/> in the given <tt>column</tt> and <tt>row</tt> of this
        /// Sudoku's grid.
        /// </summary>
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
            var oldColor = cell.ColorIndex;

            if (cell.Enter(digit, notation))
            {
                switch (notation)
                {
                    case Notation.Normal:
                        if (oldDigit == 0)
                            return new ClearOperation(column, row);
                        else return new EnterOperation(column, row, oldDigit, notation);
                    case Notation.Color:
                        return new EnterOperation(column, row, oldColor, notation);
                    default: return new EnterOperation(column, row, digit, notation);
                }
            }

            return new NoOperation();
        }

        /// <summary>
        /// Clears the top layer of the cell in the given <tt>column</tt> and <tt>row</tt>, i.e.
        /// removes the big digit, if there is one, and clears the pencilmark otherwise. A cell
        /// with pencilmark and big digit must be cleared twice.
        /// </summary>
        /// <returns>An operation to revert this action.</returns>
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
                throw new ParseJsonException();

            if (!sudokuObject.TryGetValue("constraint", out JToken constraintToken))
                throw new ParseJsonException();

            var constraint = ConstraintUtil.FromJson(constraintToken);
            var grid = sudokuObject.GetField<JObject>("grid");
            int blockWidth = grid.GetField<JValue>("block_width").ToInt();
            int blockHeight = grid.GetField<JValue>("block_height").ToInt();

            if (pencilmarkType == null)
            {
                var idValue = sudokuObject.GetField<JValue>("pencilmark_type");

                if (idValue.Type != JTokenType.String)
                    throw new ParseJsonException();

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
                throw new ParseJsonException();

            result.cells = cells;
            return result;
        }

        /// <summary>
        /// Parses the rough stucture of a Sudoku from JSON. Only cells containing a big
        /// digit are transcribed, and all state except that digit is disregarded. This is used for
        /// communication with the engine. The pencilmark type must be provided, since it is not
        /// stored in the JSON data. The parameter <tt>locked</tt> determines whether the cells
        /// containing digits will be locked, i.e. unable to be modified.
        /// </summary>
        public static Sudoku ParseJson(string json, PencilmarkType pencilmarkType,
                bool locked = true) =>
            ParseJsonWith(json,
                (token, ptype) => new SudokuCell(ptype.Empty(), token.ToInt(), locked),
                pencilmarkType);

        /// <summary>
        /// Parses the full state of this Sudoku from JSON. This includes all digits, annotations,
        /// and the lock status of each cell. This is used for savegames.
        /// </summary>
        public static Sudoku ParseFullJson(string json) =>
            ParseJsonWith(json, (token, ptype) => SudokuCell.ParseJson(token, ptype), null);
    }
}
