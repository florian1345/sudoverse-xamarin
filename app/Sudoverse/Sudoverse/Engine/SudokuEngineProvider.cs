using System;

namespace Sudoverse.Engine
{
    /// <summary>
    /// Stores the <see cref="ISudokuEngine"/> to use.
    /// </summary>
    public static class SudokuEngineProvider
    {
        /// <summary>
        /// Gets the wrapped Sudoku engine to use.
        /// </summary>
        public static EngineWrapper Engine { get; private set; }

        /// <summary>
        /// Sets the low-level Sudoku engine to use.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the engine has already been set.
        /// </exception>
        public static void SetEngine(ISudokuEngine engine)
        {
            if (Engine != null)
                throw new InvalidOperationException("engine is already set.");

            Engine = new EngineWrapper(engine);
        }
    }
}
