using Newtonsoft.Json.Linq;

using Sudoverse.SudokuModel;
using Sudoverse.Util;

namespace Sudoverse.Engine
{
    /// <summary>
    /// The response to a <see cref="EngineWrapper.Fill(Sudoku)"/> call. Contains the
    /// <see cref="Grid"/> that was generated or, if no grid could be generated, indicates so by
    /// putting <see cref="Successful"/> to <tt>false</tt>. This occurs if the Sudoku is not
    /// solvable.
    /// </summary>
    public sealed class FillResponse
    {
        /// <summary>
        /// The <see cref="SudokuGrid"/> that fills the Sudoku that was queried, or <tt>null</tt>
        /// if none could be generated.
        /// </summary>
        public SudokuGrid Grid { get; }

        /// <summary>
        /// Indicates whether filling was successful. This occurs if the Sudoku is not solvable.
        /// </summary>
        public bool Successful => Grid != null;

        private FillResponse(SudokuGrid grid)
        {
            Grid = grid;
        }

        /// <summary>
        /// Parses JSON data into a fill response.
        /// </summary>
        /// <exception cref="ParseJsonException">If the JSON does not encode a valid fill response.
        /// </exception>
        public static FillResponse ParseJson(string json)
        {
            var token = JToken.Parse(json);

            if (!(token is JObject jobject))
                throw new ParseJsonException(token.Type, JTokenType.Object);

            var type = jobject.GetField<JValue>("type");

            if (type.Type != JTokenType.String)
                throw new ParseJsonException(type.Type, JTokenType.String);

            switch ((string)type)
            {
                case "ok":
                    var value = jobject.GetField<JToken>("value");
                    return new FillResponse(SudokuGrid.FromJson(value));
                case "unsatisfiable":
                    return new FillResponse(null);
                default:
                    throw new ParseJsonException();
            }
        }
    }
}
