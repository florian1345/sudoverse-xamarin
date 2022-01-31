using Newtonsoft.Json.Linq;
using Sudoverse.Util;
using Xamarin.Forms;

namespace Sudoverse.Constraint
{
    /// <summary>
    /// A <see cref="IConstraint"/> that does not hold any additional information besides its type,
    /// such as a column constraint or a row constraint.
    /// </summary>
    internal sealed class StatelessConstraint : IConstraint
    {
        public StatelessConstraint() { }

        public View[] GetBackgroundViews(ReadOnlyMatrix<ReadOnlyRect> fieldBounds) =>
            new View[0];

        public Frame GetFrame() =>
            Frame.Empty();

        public JToken ToJson() =>
            JValue.CreateNull();

        public void FromJson(JToken token)
        {
            if (token.Type != JTokenType.Null)
                throw new LoadConstraintException();
        }
    }
}
