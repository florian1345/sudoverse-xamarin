using Engine;
using Sudoverse.Engine;

namespace Sudoverse.Droid
{
    internal sealed class AndroidSudokuEngine : ISudokuEngine
    {
        public bool CheckDefault(string json) =>
            EngineSo.CheckDefault(json);

        public string GenDefault(int difficulty) =>
            EngineSo.GenDefault(difficulty);

        public int Test() =>
            EngineSo.Test();
    }
}
