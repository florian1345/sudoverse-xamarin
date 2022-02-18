using Newtonsoft.Json.Linq;

using Sudoverse.Util;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms;

namespace Sudoverse.Constraint
{
    /// <summary>
    /// A <see cref="IConstraint"/> that consists of a list of sub-constraints.
    /// </summary>
    public sealed class CompositeConstraint : IConstraint
    {
        public event EventHandler EditorFrameFocused;
        public event EventHandler<ConstraintOperation> Changed;

        private List<IConstraint> constraints;

        public string Type => "composite";

        /// <summary>
        /// A <see cref="ReadOnlyCollection{T}"/> that contains all subconstraints of this
        /// composite constraint.
        /// </summary>
        public ReadOnlyCollection<IConstraint> Constraints =>
            new ReadOnlyCollection<IConstraint>(constraints);

        private CompositeConstraint(List<IConstraint> constraints)
        {
            this.constraints = constraints;

            for (int i = 0; i < this.constraints.Count; i++)
            {
                var constraint = this.constraints[i];
                var fi = i;

                constraint.EditorFrameFocused += (sender, e) =>
                    EditorFrameFocused?.Invoke(sender, e);
                constraint.Changed += (sender, op) =>
                    Changed?.Invoke(sender, new CompositeConstraintOperation(fi, op));
            }
        }

        /// <summary>
        /// Creates a new composite constraints from its sub-<tt>constraints</tt>.
        /// </summary>
        public CompositeConstraint(params IConstraint[] constraints)
            : this(constraints.ToList()) { }

        public View[] GetBackgroundViews(ReadOnlyMatrix<ReadOnlyRect> fieldBounds)
        {
            var result = Enumerable.Empty<View>();

            foreach (var constraint in constraints)
                result = result.Concat(constraint.GetBackgroundViews(fieldBounds));

            return result.ToArray();
        }

        public FrameGroup GetFrames() =>
            constraints.Select(c => c.GetFrames()).Aggregate(FrameGroup.Combine);

        public FrameGroup GetEditorFrames() =>
            constraints.Select(c => c.GetEditorFrames()).Aggregate(FrameGroup.Combine);

        public JToken ToJsonValue()
        {
            JArray jarray = new JArray();

            foreach (var constraint in constraints)
                jarray.Add(ConstraintUtil.ToJson(constraint));

            return jarray;
        }

        /// <summary>
        /// Loads a composite constraint from JSON data.
        /// </summary>
        /// <exception cref="ParseJsonException">If the JSON data does not represent a valid
        /// composite constraint.</exception>
        public static IConstraint FromJsonValue(JToken token)
        {
            if (token.Type != JTokenType.Array)
                throw new ParseJsonException();

            var constraints = ((JArray)token).Select(t => ConstraintUtil.FromJson(t))
                .ToList();
            return new CompositeConstraint(constraints);
        }
    }
}
