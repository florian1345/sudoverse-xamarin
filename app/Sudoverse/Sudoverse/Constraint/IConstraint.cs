using Newtonsoft.Json.Linq;
using Sudoverse.Util;
using System;
using Xamarin.Forms;

namespace Sudoverse.Constraint
{
    /// <summary>
    /// Various static utility methods for handling <see cref="IConstraint"/>s.
    /// </summary>
    public static class ConstraintUtil
    {
        /// <summary>
        /// Loads a constraint from JSON data.
        /// </summary>
        /// <exception cref="ParseJsonException">If the JSON data does not represent a valid
        /// constraint.</exception>
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

        /// <summary>
        /// Converts the given constraint into JSON data.
        /// </summary>
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

    /// <summary>
    /// Specifies behavior that relates to a constraint's appearance. A constraint is a condution
    /// that applies to a Sudoku grid. Note that the constraint's logic is implemented by the
    /// engine. It also serves identification purposes for communication with the engine.
    /// </summary>
    public interface IConstraint
    {
        /// <summary>
        /// This event is raised whenever a view in the editor frame of this constraint is
        /// focused. In this case, all cells in the grid should be deselected to avoid any
        /// unintended input.
        /// </summary>
        event EventHandler EditorFrameFocused;

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
        /// Gets the <see cref="FrameGroup"/> that shall surround the Sudoku with additional
        /// information about this constraint in the editor. It should support modification of the
        /// data inside the frame, which should then modify the constraint. Return an empty group
        /// if you do not need such a frame.
        /// </summary>
        FrameGroup GetEditorFrames();

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
