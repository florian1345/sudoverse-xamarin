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

    public interface IConstraint
    {
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
        /// constraint, use <see cref="JValue.CreateNull"/>. Loading this in the same type using
        /// <see cref="FromJson(JToken)"/> must yield an equivalent result.
        /// </summary>
        JToken ToJson();

        /// <summary>
        /// Loads the parameters of this constraint from JSON data. Converting it back into JSON
        /// using <see cref="ToJson"/> must yield an equivalent <see cref="JToken"/>.
        /// </summary>
        /// <param name="token">The JSON token holding the data to load.</param>
        /// <exception cref="LoadConstraintException">If the JSON data does not represent a
        /// constraint of this type.</exception>
        void FromJson(JToken token);
    }
}
