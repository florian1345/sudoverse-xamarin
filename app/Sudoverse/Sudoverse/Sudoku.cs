using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sudoverse.Constraint;
using System;

namespace Sudoverse
{
    internal sealed class ParseSudokuException : Exception
    {
        public ParseSudokuException()
            : base("Invalid JSON data for Sudoku.") { }
    }

    public sealed class Sudoku
    {
        internal int BlockWidth { get; private set; }
        internal int BlockHeight { get; private set; }
        internal int Size { get; private set; }
        internal IConstraint Constraint { get; private set; }

        private int[] cells;

        internal Sudoku(int blockWidth, int blockHeight, IConstraint constraint)
        {
            if (blockWidth <= 0 || blockHeight <= 0)
                throw new ArgumentException("Block width and height must be positive.");

            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
            Size = BlockWidth * BlockHeight;
            cells = new int[Size * Size];
            Constraint = constraint;
        }

        internal int GetCell(int x, int y)
        {
            return cells[y * Size + x];
        }

        internal void SetCell(int x, int y, int digit)
        {
            cells[y * Size + x] = digit;
        }

        internal string ToJson()
        {
            var cellsJson = new JArray();

            for (int i = 0; i < cells.Length; i++)
            {
                int cell = cells[i];

                if (cell == 0) cellsJson.Add(JValue.CreateNull());
                else cellsJson.Add(cells[i]);
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

            int[] cells = new int[jsonCells.Count];

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = ToInt(jsonCells[i]);
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
