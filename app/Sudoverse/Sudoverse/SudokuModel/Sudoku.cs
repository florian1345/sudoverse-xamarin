using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sudoverse.Constraint;
using System;
using System.Linq;

namespace Sudoverse.SudokuModel
{
    public sealed class ParseSudokuException : Exception
    {
        public ParseSudokuException()
            : base("Invalid JSON data for Sudoku.") { }
    }

    public sealed class Sudoku
    {
        public int BlockWidth { get; private set; }
        public int BlockHeight { get; private set; }
        public int Size { get; private set; }
        public IConstraint Constraint { get; private set; }

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
                        cell.EnterNormal(digit);
                        return new EnterOperation(column, row, oldDigit, notation);
                    }

                    break;
                case Notation.Small:
                    if (!cell.Filled)
                    {
                        cell.ToggleSmall(digit);
                        return new EnterOperation(column, row, digit, notation);
                    }

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
                cell.Clear();
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

        public string ToJson()
        {
            var cellsJson = new JArray();

            for (int i = 0; i < cells.Length; i++)
            {
                int cell = cells[i].Digit;

                if (cell == 0) cellsJson.Add(JValue.CreateNull());
                else cellsJson.Add(cell);
            }

            var grid = new JObject();
            grid.Add("block_width", BlockWidth);
            grid.Add("block_height", BlockHeight);
            grid.Add("cells", cellsJson);

            var sudoku = new JObject();
            sudoku.Add("grid", grid);
            sudoku.Add("constraint", Constraint.ToJson());
            return JsonConvert.SerializeObject(sudoku);
        }

        private static T GetField<T>(JObject jsonObject, string name)
        {
            if (!jsonObject.TryGetValue(name, out JToken field))
                throw new ParseSudokuException();

            if (!(field is T t))
                throw new ParseSudokuException();

            return t;
        }

        private static int ToInt(JToken token)
        {
            if (token.Type == JTokenType.Integer)
                return (int)token;

            if (token.Type == JTokenType.Null)
                return 0;

            throw new ParseSudokuException();
        }

        internal static Sudoku ParseJson<C>(string json)
            where C : IConstraint, new()
        {
            var sudokuToken = JToken.Parse(json);

            if (!(sudokuToken is JObject sudokuObject))
                throw new ParseSudokuException();

            var grid = GetField<JObject>(sudokuObject, "grid");
            int blockWidth = ToInt(GetField<JValue>(grid, "block_width"));
            int blockHeight = ToInt(GetField<JValue>(grid, "block_height"));
            Sudoku result = new Sudoku(blockWidth, blockHeight, new C());
            var jsonCells = GetField<JArray>(grid, "cells");

            SudokuCell[] cells = new SudokuCell[jsonCells.Count];

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new SudokuCell(ToInt(jsonCells[i]));
            }

            if (cells.Length != result.cells.Length)
                throw new ParseSudokuException();

            result.cells = cells;

            if (!sudokuObject.TryGetValue("constraint", out JToken constraintToken))
                throw new ParseSudokuException();

            result.Constraint.FromJson(constraintToken);
            return result;
        }
    }
}
