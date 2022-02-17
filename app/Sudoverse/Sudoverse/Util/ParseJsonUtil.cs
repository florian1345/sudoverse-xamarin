using Newtonsoft.Json.Linq;
using System;

namespace Sudoverse.Util
{
    /// <summary>
    /// An exception thrown whenever JSON data with an invalid schema is parsed.
    /// </summary>
    public sealed class ParseJsonException : Exception
    {
        /// <summary>
        /// Creates a new parse JSON exception with a generic message.
        /// </summary>
        public ParseJsonException()
            : base("Invalid JSON data.") { }

        /// <summary>
        /// Creates a new parse JSON exception with a message that indicates that a struct field
        /// with the given name is missing..
        /// </summary>
        public ParseJsonException(string missingFieldName)
            : base($"Missing field in JSON data: {missingFieldName}.") { }

        /// <summary>
        /// Creates a new parse JSON exception with a message that indicates a value of the given
        /// <tt>presentType</tt> should actually have been of one of the <tt>expectedTypes</tt>.
        /// </summary>
        public ParseJsonException(JTokenType presentType, params JTokenType[] expectedTypes)
            : base($"Got value of type {presentType}, but expected one of " +
                  $"{string.Join(", ", expectedTypes)}.") { }

        /// <summary>
        /// Creates a new parse JSON exception with a message that indicates an array of the given
        /// <tt>arrayLength</tt> should actually have been of the <tt>expectedLength</tt>.
        /// </summary>
        public ParseJsonException(int arrayLength, int expectedLength)
            : base($"Got array of length {arrayLength}, but expected {expectedLength}.") { }
    }

    /// <summary>
    /// A collection of utility methods regarding JSON parsing.
    /// </summary>
    public static class ParseJsonUtil
    {
        /// <summary>
        /// Gets a field of this object that has the given name. Also casts it to the specified
        /// generic type.
        /// </summary>
        /// <typeparam name="T">The expected type of the field.</typeparam>
        /// <returns>The value stored in the field with the given name casted to the given type.
        /// </returns>
        /// <exception cref="ParseJsonException">If there is no field with the given name or it has
        /// the wrong type.</exception>
        public static T GetField<T>(this JObject jsonObject, string name)
        {
            if (!jsonObject.TryGetValue(name, out JToken field))
                throw new ParseJsonException(name);

            if (!(field is T t))
                throw new ParseJsonException();

            return t;
        }

        /// <summary>
        /// Checks that the given token represents an integer or <tt>null</tt> and converts it to a
        /// C# integer.
        /// </summary>
        /// <param name="nullValue">The value to be returned if the value is null.</param>
        /// <returns>This JSON integer value as a C# integer, or <tt>nullValue</tt> if it was a
        /// JSON <tt>null</tt> value.</returns>
        /// <exception cref="ParseJsonException">If the token is not of type integer or null.
        /// </exception>
        public static int ToInt(this JToken token, int nullValue)
        {
            if (token.Type == JTokenType.Integer)
                return (int)token;

            if (token.Type == JTokenType.Null)
                return nullValue;

            throw new ParseJsonException(token.Type, JTokenType.Integer, JTokenType.Null);
        }

        /// <summary>
        /// Checks that the given token represents an integer or <tt>null</tt> and converts it to a
        /// C# integer. If it is null, returns 0.
        /// </summary>
        /// <returns>This JSON integer value as a C# integer, or 0 if it was a JSON <tt>null</tt>
        /// value.</returns>
        /// <exception cref="ParseJsonException">If the token is not of type integer or null.
        /// </exception>
        public static int ToInt(this JToken token) => token.ToInt(0);
    }
}
