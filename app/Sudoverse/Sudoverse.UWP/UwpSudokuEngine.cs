using Engine;
using Sudoverse.Engine;

namespace Sudoverse.UWP
{
    internal class UwpSudokuEngine : ISudokuEngine
    {
        public int Test() =>
            EngineDll.Test();

        public string GenDefault() =>
            EngineDll.GenDefault();

        public bool CheckDefault(string json) =>
            EngineDll.CheckDefault(json);
    }
}
