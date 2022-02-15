using Engine;
using Sudoverse.Engine;

namespace Sudoverse.Droid
{
    internal sealed class AndroidSudokuEngine : ISudokuEngine
    {
        public string Check(string json) =>
            EngineSo.Check(json);

        public string Gen(int constraint, int difficulty) =>
            EngineSo.Gen(constraint, difficulty);

        public int Test() =>
            EngineSo.Test();

        public string Fill(string json) =>
            EngineSo.Fill(json);

        public byte IsSolvable(string json) =>
            EngineSo.IsSolvable(json);
    }
}
