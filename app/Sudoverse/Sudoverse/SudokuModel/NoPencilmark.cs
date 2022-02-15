using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoverse.SudokuModel
{
    /// <summary>
    /// A dummy <see cref="IPencilmark"/> that does not actually allow any notation. For use in the
    /// editor.
    /// </summary>
    public sealed class NoPencilmark : IPencilmark
    {
        public string TopLeft => "";

        public string TopCenter => "";

        public string TopRight => "";

        public string CenterLeft => "";

        public string Center => "";

        public string CenterRight => "";

        public string BottomLeft => "";

        public string BottomCenter => "";

        public string BottomRight => "";

        public IEnumerable<(int, Notation)> Clear() =>
            Enumerable.Empty<(int, Notation)>();

        public bool Enter(int digit, Notation notation)
        {
            throw new InvalidOperationException("Cannot enter digit in NoPencilmark.");
        }

        public JToken ToJson() => JValue.CreateNull();
    }
}
