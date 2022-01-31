using Newtonsoft.Json.Linq;
using Sudoverse.Util;
using System;
using Xamarin.Forms;

namespace Sudoverse.Constraint
{
    internal sealed class CompositeConstraint<C1, C2> : IConstraint
        where C1 : IConstraint, new()
        where C2 : IConstraint, new()
    {
        private C1 c1;
        private C2 c2;

        public CompositeConstraint()
        {
            c1 = new C1();
            c2 = new C2();
        }

        public View[] GetBackgroundViews(ReadOnlyMatrix<ReadOnlyRect> fieldBounds) =>
            c1.GetBackgroundViews(fieldBounds).Concat(c2.GetBackgroundViews(fieldBounds));

        public Frame GetFrame()
        {
            throw new NotImplementedException();
        }

        public JToken ToJson()
        {
            throw new NotImplementedException();
        }

        public void FromJson(JToken token)
        {
            if (!(token is JObject jobject))
                throw new LoadConstraintException();

            if (jobject.TryGetValue("c1", out JToken tc1))
                c1.FromJson(tc1);
            else throw new LoadConstraintException();

            if (jobject.TryGetValue("c2", out JToken tc2))
                c2.FromJson(tc2);
            else throw new LoadConstraintException();
        }
    }
}
