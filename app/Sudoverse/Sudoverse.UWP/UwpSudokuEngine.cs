using Engine;

namespace Sudoverse.UWP
{
    internal class UwpSudokuEngine : ISudokuEngine
    {
        public int Test()
        {
            return EngineDll.Test();
        }
    }
}
