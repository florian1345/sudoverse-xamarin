using Newtonsoft.Json.Linq;
using System;

namespace Sudoverse.Util
{
    public sealed class ParseJsonException : Exception
    {
        public ParseJsonException()
            : base("Invalid JSON data.") { }

        public ParseJsonException(string missingFieldName)
            : base($"Missing field in JSON data: {missingFieldName}.") { }

        public ParseJsonException(JTokenType presentType, params JTokenType[] expectedTypes)
            : base($"Got value of type {presentType}, but expected one of " +
                  $"{string.Join(", ", expectedTypes)}.") { }

        public ParseJsonException(int arrayLength, int expectedLength)
            : base($"Got array of length {arrayLength}, but expected {expectedLength}.") { }
    }

    public static class ParseJsonUtil
    {
        public static T GetField<T>(this JObject jsonObject, string name)
        {
            if (!jsonObject.TryGetValue(name, out JToken field))
                throw new ParseJsonException(name);

            if (!(field is T t))
                throw new ParseJsonException();

            return t;
        }

        public static int ToInt(this JToken token, int nullValue)
        {
            if (token.Type == JTokenType.Integer)
                return (int)token;

            if (token.Type == JTokenType.Null)
                return nullValue;

            throw new ParseJsonException(token.Type, JTokenType.Integer, JTokenType.Null);
        }

        public static int ToInt(this JToken token) => token.ToInt(0);
    }
}
