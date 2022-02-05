﻿using Engine;
using Sudoverse.Engine;

namespace Sudoverse.Droid
{
    internal sealed class AndroidSudokuEngine : ISudokuEngine
    {
        public bool Check(string json) =>
            EngineSo.Check(json) > 0;

        public string Gen(int constraint, int difficulty) =>
            EngineSo.Gen(constraint, difficulty);

        public int Test() =>
            EngineSo.Test();
    }
}
