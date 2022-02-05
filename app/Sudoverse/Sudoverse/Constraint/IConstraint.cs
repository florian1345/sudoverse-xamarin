using Newtonsoft.Json.Linq;
using Sudoverse.Util;
using System;
using Xamarin.Forms;

namespace Sudoverse.Constraint
{
    /// <summary>
    /// An exception raised if the JSON data used to load a constraint is invalid for that type.
    /// </summary>
    internal class LoadConstraintException : Exception
    {
        public LoadConstraintException()
            : base("Invalid JSON data for constraint.") { }
    }

    public static class ConstraintUtil
    {
        public static IConstraint FromJson(JToken jtoken)
        {
            if (!(jtoken is JObject jobject))
                throw new LoadConstraintException();

            if (!jobject.TryGetValue("type", out JToken typeToken))
                throw new LoadConstraintException();

            if (typeToken.Type != JTokenType.String)
                throw new LoadConstraintException();

            var type = (string)typeToken;

            switch (type)
            {
                case "default":
                case "diagonals":
                case "knights-move":
                case "kings-move":
                    return new StatelessConstraint(type);
                case "composite":
                    if (!jobject.TryGetValue("value", out JToken valueToken))
                        throw new LoadConstraintException();

                    return CompositeConstraint.FromJsonValue(valueToken);
            }

            throw new LoadConstraintException();
        }

        public static JToken ToJson(IConstraint constraint)
        {
            var jobject = new JObject();
            jobject.Add("type", constraint.Type);
            var value = constraint.ToJsonValue();

            if (value != null)
                jobject.Add("value", value);

            return jobject;
        }
    }

    public interface IConstraint
    {
        /// <summary>
        /// The textual type identifier of this constraint.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the <see cref="Frame"/> that shall surround the Sudoku with additional information
        /// about this constraint. Use <see cref="Frame.Empty"/> if you do not need such a frame.
        /// </summary>
        Frame GetFrame();

        /// <summary>
        /// Gets an array of <see cref="View"/>s which should be rendered behind the cells (between
        /// the cell background and text/grid).
        /// </summary>
        /// <param name="fieldBounds">A <see cref="ReadOnlyMatrix{T}"/>, whose element at (column,
        /// row) contains the <see cref="ReadOnlyRect"/> bounding the cell view at that location.
        /// Column and row are 0-based.</param>
        View[] GetBackgroundViews(ReadOnlyMatrix<ReadOnlyRect> fieldBounds);

        /// <summary>
        /// Converts this constraint to a <see cref="JToken"/> representing its data. For stateless
        /// constraint, return <tt>null</tt>.
        /// </summary>
        JToken ToJsonValue();
    }
}
