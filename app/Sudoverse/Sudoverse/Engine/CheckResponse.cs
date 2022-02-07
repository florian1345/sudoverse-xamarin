using Newtonsoft.Json.Linq;
using Sudoverse.Util;
using System.Collections.ObjectModel;

namespace Sudoverse.Engine
{
    /// <summary>
    /// The response to a <see cref="EngineWrapper.Check(SudokuModel.Sudoku)"/> call. Determines
    /// whether the Sudoku is valid and, if not, gives information about which cells are invalid.
    /// </summary>
    public sealed class CheckResponse
    {
        private (int, int)[] invalidCells;

        /// <summary>
        /// Indicates whether the Sudoku was valid.
        /// </summary>
        public bool Valid { get; }

        /// <summary>
        /// If the Sudoku was not valid, this collection contains all coordinates of the form
        /// <tt>(column, row)</tt> of the cells which were not valid.
        /// </summary>
        public ReadOnlyCollection<(int, int)> InvalidCells =>
            new ReadOnlyCollection<(int, int)>(invalidCells);

        private CheckResponse(bool valid, (int, int)[] invalidCells)
        {
            Valid = valid;
            this.invalidCells = invalidCells;
        }

        private static bool ToValid(JValue typeValue)
        {
            if (typeValue.Type != JTokenType.String)
                throw new ParseJsonException(typeValue.Type, JTokenType.String);

            switch ((string)typeValue.Value)
            {
                case "valid": return true;
                case "invalid": return false;
                default: throw new ParseJsonException();
            }
        }

        public static CheckResponse ParseJson(string json)
        {
            var token = JToken.Parse(json);

            if (!(token is JObject jobject))
                throw new ParseJsonException(token.Type, JTokenType.Object);

            var typeValue = jobject.GetField<JValue>("type");
            bool valid = ToValid(typeValue);

            if (!valid)
            {
                var valueArray = jobject.GetField<JArray>("value");
                int length = valueArray.Count;
                var invalidCells = new (int, int)[length];

                for (int i = 0; i < length; i++)
                {
                    if (!(valueArray[i] is JArray coordinatesArray))
                        throw new ParseJsonException();

                    if (coordinatesArray.Count != 2)
                        throw new ParseJsonException(coordinatesArray.Count, 2);

                    invalidCells[i] = (coordinatesArray[0].ToInt(), coordinatesArray[1].ToInt());
                }

                return new CheckResponse(valid, invalidCells);
            }
            else return new CheckResponse(valid, new (int, int)[0]);
        }
    }
}
