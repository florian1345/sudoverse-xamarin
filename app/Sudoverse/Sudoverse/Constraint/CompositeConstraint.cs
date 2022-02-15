using Newtonsoft.Json.Linq;

using Sudoverse.Util;

using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace Sudoverse.Constraint
{
    internal sealed class CompositeConstraint : IConstraint
    {
        public event EventHandler EditorFrameFocused;

        private List<IConstraint> constraints;

        public string Type => "composite";

        private CompositeConstraint(List<IConstraint> constraints)
        {
            this.constraints = constraints;

            foreach (var constraint in this.constraints)
            {
                constraint.EditorFrameFocused += (sender, e) =>
                    EditorFrameFocused?.Invoke(sender, e);
            }
        }

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
