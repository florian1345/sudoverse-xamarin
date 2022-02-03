using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sudoverse.Constraint;
using System;

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

        public void EnterCell(int column, int row, int digit, Notation notation)
        {
            var cell = cells[row * Size + column];

            switch (notation)
            {
                case Notation.Normal:
                    cell.EnterNormal(digit);
                    break;
                case Notation.Small:
                    if (!cell.Filled) cell.ToggleSmall(digit);
                    break;
                case Notation.Corner:
                    if (!cell.Filled) cell.ToggleCorner(digit);
                    break;
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
