using Newtonsoft.Json.Linq;

namespace Sudoverse.SudokuModel
{
    /// <summary>
    /// Abstractly represents a type of <see cref="IPencilmark"/> and supports diverse utility.
    /// </summary>
    public abstract class PencilmarkType
    {
        private sealed class CenterBorderPencilmarkTypeImpl : PencilmarkType
        {
            public const string IDENTIFIER = "center-border";

            public override IPencilmark Empty() => new CenterBorderPencilmark();

            public override IPencilmark FromJson(JToken token) =>
                CenterBorderPencilmark.FromJson(token);

            public override string GetIdentifier() => IDENTIFIER;
        }

        private sealed class PositionalPencilmarkTypeImpl : PencilmarkType
        {
            public const string IDENTIFIER = "positional";

            public override IPencilmark Empty() => new PositionalPencilmark();

            public override IPencilmark FromJson(JToken token) =>
                PositionalPencilmark.FromJson(token);

            public override string GetIdentifier() => IDENTIFIER;
        }

        /// <summary>
        /// A pencilmark type for the <see cref="CenterBorderPencilmark"/>.
        /// </summary>
        public static readonly PencilmarkType CenterBorderPencilmarkType =
            new CenterBorderPencilmarkTypeImpl();

        /// <summary>
        /// A pencilmark type for the <see cref="PositionalPencilmark"/>.
        /// </summary>
        public static readonly PencilmarkType PositionalPencilmarkType =
            new PositionalPencilmarkTypeImpl();

        private PencilmarkType() { }

        /// <summary>
        /// Creates a new, empty <see cref="IPencilmark"/> of this type.
        /// </summary>
        public abstract IPencilmark Empty();

        /// <summary>
        /// Parses a <see cref="IPencilmark"/> of this type from JSON data.
        /// </summary>
        public abstract IPencilmark FromJson(JToken token);

        /// <summary>
        /// Gets the identifier for this pencilmark type.
        /// </summary>
        /// <returns></returns>
        public abstract string GetIdentifier();

        /// <summary>
        /// Returns the appropriate pencilmark type for the given identifier, or throws a
        /// <see cref="ParseSudokuException"/> if there is none.
        /// </summary>
        public static PencilmarkType FromIdentifier(string identifier)
        {
            switch (identifier)
            {
                case CenterBorderPencilmarkTypeImpl.IDENTIFIER: return CenterBorderPencilmarkType;
                case PositionalPencilmarkTypeImpl.IDENTIFIER: return PositionalPencilmarkType;
                default: throw new ParseSudokuException();
            }
        }
    }
}
