using System;

namespace Sudoverse.Engine
{
    public static class SudokuEngineProvider
    {
        public static EngineWrapper Engine { get; private set; }

        public static void SetEngine(ISudokuEngine engine)
        {
            if (Engine != null)
                throw new InvalidOperationException("Engine is already set.");

            Engine = new EngineWrapper(engine);
        }
    }
}
