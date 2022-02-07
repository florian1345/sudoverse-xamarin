using Engine;
using Sudoverse.Engine;

namespace Sudoverse.UWP
{
    internal class UwpSudokuEngine : ISudokuEngine
    {
        public int Test() =>
            EngineDll.Test();

        public string Gen(int constraint, int difficulty) =>
            EngineDll.Gen(constraint, difficulty);

        public string Check(string json) =>
            EngineDll.Check(json);
    }
}
