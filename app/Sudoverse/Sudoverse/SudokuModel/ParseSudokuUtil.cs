using Newtonsoft.Json.Linq;
using System;

namespace Sudoverse.SudokuModel
{
    public sealed class ParseSudokuException : Exception
    {
        public ParseSudokuException()
            : base("Invalid JSON data for Sudoku.") { }
    }

    public static class ParseSudokuUtil
    {
        public static T GetField<T>(this JObject jsonObject, string name)
        {
            if (!jsonObject.TryGetValue(name, out JToken field))
                throw new ParseSudokuException();

            if (!(field is T t))
                throw new ParseSudokuException();

            return t;
        }

        public static int ToInt(this JToken token)
        {
            if (token.Type == JTokenType.Integer)
                return (int)token;

            if (token.Type == JTokenType.Null)
                return 0;

            throw new ParseSudokuException();
        }
    }
}
