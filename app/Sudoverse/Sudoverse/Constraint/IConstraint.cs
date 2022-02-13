using Newtonsoft.Json.Linq;
using Sudoverse.Util;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Sudoverse.Constraint
{
    public static class ConstraintUtil
    {
        public static IConstraint FromJson(JToken jtoken)
        {
            if (!(jtoken is JObject jobject))
                throw new ParseJsonException(jtoken.Type, JTokenType.Object);

            if (!jobject.TryGetValue("type", out JToken typeToken))
                throw new ParseJsonException("type");

            if (typeToken.Type != JTokenType.String)
                throw new ParseJsonException(typeToken.Type, JTokenType.String);

            var type = (string)typeToken;

            switch (type)
            {
                case "default":
                case "diagonals":
                case "knights-move":
                case "kings-move":
                    return new StatelessConstraint(type);
                case "sandwich":
                    return SandwichConstraint.FromJsonValue(jobject.GetField<JToken>("value"));
                case "composite":
                    return CompositeConstraint.FromJsonValue(jobject.GetField<JToken>("value"));
            }

            throw new ParseJsonException();
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
        /// Gets the <see cref="FrameGroup"/> that shall surround the Sudoku with additional
        /// information about this constraint. Return an empty group if you do not need such a
        /// frame.
        /// </summary>
        FrameGroup GetFrames();

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
