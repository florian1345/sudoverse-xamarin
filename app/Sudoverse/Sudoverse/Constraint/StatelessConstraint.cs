using Newtonsoft.Json.Linq;
using Sudoverse.Util;
using System;
using Xamarin.Forms;

namespace Sudoverse.Constraint
{
    /// <summary>
    /// A <see cref="IConstraint"/> that does not hold any additional information besides its type,
    /// such as a column constraint or a row constraint.
    /// </summary>
    public sealed class StatelessConstraint : IConstraint
    {
        public event EventHandler EditorFrameFocused;
        public event EventHandler<ConstraintOperation> Changed;

        public string Type { get; }

        /// <summary>
        /// Creates a new stateless constraint for a constraint with the given <tt>type</tt>
        /// identifier.
        /// </summary>
        public StatelessConstraint(string type)
        {
            Type = type;
        }

        public View[] GetBackgroundViews(ReadOnlyMatrix<ReadOnlyRect> fieldBounds) =>
            new View[0];

        public FrameGroup GetFrames() =>
            FrameGroup.Empty();

        public FrameGroup GetEditorFrames() =>
            FrameGroup.Empty();

        public JToken ToJsonValue() =>
            null;
    }
}
