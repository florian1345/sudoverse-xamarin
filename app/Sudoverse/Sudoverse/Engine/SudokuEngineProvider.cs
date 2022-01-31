using System;

namespace Sudoverse.Engine
{
    public static class SudokuEngineProvider
    {
        private static ISudokuEngine engine;

        public static ISudokuEngine Engine
        {
            get
            {
                return engine;
            }

            set
            {
                if (engine != null)
                    throw new Exception("Engine has already been set.");

                engine = value;
            }
        }
    }
}
