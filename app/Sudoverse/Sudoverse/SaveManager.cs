using Sudoverse.SudokuModel;
using System;
using System.IO;

namespace Sudoverse
{
    /// <summary>
    /// Handles saving and loading of Sudoku to allow them to be continued later.
    /// </summary>
    public static class SaveManager
    {
        private static readonly string CurrentFileName = GetLocalPath("current.json");
        private static readonly string PuzzleDirectory = GetLocalPath("puzzles");

        private static string GetLocalPath(string path) =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path);

        static SaveManager()
        {
            Directory.CreateDirectory(PuzzleDirectory);
        }

        /// <summary>
        /// Indicates whether there is a current Sudoku to continue at the moment.
        /// </summary>
        public static bool HasCurrent() =>
            File.Exists(CurrentFileName);

        /// <summary>
        /// Loads the current Sudoku.
        /// </summary>
        public static Sudoku LoadCurrent() =>
            Sudoku.ParseFullJson(File.ReadAllText(CurrentFileName));

        /// <summary>
        /// Stores the given Sudoku as the current one.
        /// </summary>
        public static void SaveCurrent(Sudoku sudoku)
        {
            File.WriteAllText(CurrentFileName, sudoku.ToFullJson());
        }

        /// <summary>
        /// Removes the current Sudoku.
        /// </summary>
        public static void RemoveCurrent()
        {
            File.Delete(CurrentFileName);
        }

        /// <summary>
        /// Saves the given Sudoku as a puzzle (i.e. without any pencilmarks) under the given name.
        /// </summary>
        public static void SavePuzzle(Sudoku sudoku, string name)
        {
            // TODO handle names that are invalid paths
            var path = Path.Combine(PuzzleDirectory, name + ".json");
            File.WriteAllText(path, sudoku.ToJson());
        }
    }
}
