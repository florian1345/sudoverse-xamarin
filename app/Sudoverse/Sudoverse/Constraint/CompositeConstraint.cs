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
        private List<IConstraint> constraints;

        public string Type => "composite";

        private CompositeConstraint(List<IConstraint> constraints)
        {
            this.constraints = constraints;
        }

        public View[] GetBackgroundViews(ReadOnlyMatrix<ReadOnlyRect> fieldBounds)
        {
            var result = Enumerable.Empty<View>();

            foreach (var constraint in constraints)
                result = result.Concat(constraint.GetBackgroundViews(fieldBounds));

            return result.ToArray();
        }

        public Frame GetFrame()
        {
            throw new NotImplementedException();
        }

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
                throw new LoadConstraintException();

            var constraints = ((JArray)token).Select(t => ConstraintUtil.FromJson(t))
                .ToList();
            return new CompositeConstraint(constraints);
        }
    }
}
