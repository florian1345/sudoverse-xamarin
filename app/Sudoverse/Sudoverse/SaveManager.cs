using Sudoverse.SudokuModel;
using Sudoverse.Util;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sudoverse
{
    /// <summary>
    /// An exception raised whenever the file loader encounters a file name with an invalid escape
    /// sequence.
    /// </summary>
    public sealed class FilenameEscapingException : Exception
    {
        public FilenameEscapingException()
            : base("Filename contained invalid escape sequence.") { }
    }

    /// <summary>
    /// Handles saving and loading of Sudoku to allow them to be continued later.
    /// </summary>
    public static class SaveManager
    {
        private static readonly string CurrentFileName = GetLocalPath("current.json");
        private static readonly string PuzzleDirectory = GetLocalPath("puzzles");

        private const char ESCAPE_CHAR = '$';

        private static readonly BitArray EscapedChars = GetEscapedChars();

        private static string GetLocalPath(string path) =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path);

        private static BitArray GetEscapedChars()
        {
            char[] escapedChars =
                Path.GetInvalidFileNameChars().Concat(new char[] { ESCAPE_CHAR });
            int max = escapedChars.Max();
            var bitArray = new BitArray(max + 1);

            foreach (char c in Path.GetInvalidFileNameChars())
                bitArray[c] = true;

            bitArray[ESCAPE_CHAR] = true;

            return bitArray;
        }

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

        private static string Escape(string name)
        {
            if (name.Any(c => EscapedChars[c]))
            {
                var escapedNameBuilder = new StringBuilder();

                foreach (char c in name)
                {
                    if (EscapedChars[c])
                    {
                        escapedNameBuilder.Append(ESCAPE_CHAR);
                        escapedNameBuilder.Append(((int)c).ToString("X4"));
                    }
                    else escapedNameBuilder.Append(c);
                }

                return escapedNameBuilder.ToString();
            }
            else return name;
        }

        private static string Unescape(string filename)
        {
            if (filename.Contains(ESCAPE_CHAR))
            {
                var unescapedNameBuilder = new StringBuilder();

                for (int i = 0; i < filename.Length; i++)
                {
                    var c = filename[i];

                    if (c == ESCAPE_CHAR)
                    {
                        if (i >= filename.Length - 4)
                            throw new FilenameEscapingException();

                        var hex = filename.Substring(i + 1, 4);

                        if (!int.TryParse(hex, NumberStyles.HexNumber,
                                NumberFormatInfo.InvariantInfo, out int codepoint))
                            throw new FilenameEscapingException();

                        unescapedNameBuilder.Append((char)codepoint);
                        i += 4;
                    }
                    else unescapedNameBuilder.Append(c);
                }

                return unescapedNameBuilder.ToString();
            }
            else return filename;
        }

        private static string GetPuzzlePath(string name) =>
            string.Format(Path.Combine(PuzzleDirectory, Escape(name) + ".json"));

        /// <summary>
        /// Returns a list of the puzzle names of all stored Sudoku puzzles. These can be loaded
        /// using <see cref="LoadPuzzle(string, PencilmarkType)"/>.
        /// </summary>
        /// <exception cref="FilenameEscapingException">If any file in the puzzle directory
        /// contains an invalid escape sequence.</exception>
        public static string[] ListPuzzles()
        {
            var list = new List<string>();

            foreach (string path in Directory.EnumerateFiles(PuzzleDirectory))
            {
                if (Path.GetExtension(path) == ".json")
                    list.Add(Unescape(Path.GetFileNameWithoutExtension(path)));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Saves the given Sudoku as a puzzle (i.e. without any pencilmarks) under the given name.
        /// </summary>
        public static void SavePuzzle(Sudoku sudoku, string name)
        {
            File.WriteAllText(GetPuzzlePath(name), sudoku.ToJson());
        }

        /// <summary>
        /// Loads the Sudoku puzzle (i.e. without any pencilmarks) stored under the given name.
        /// </summary>
        public static Sudoku LoadPuzzle(string name, PencilmarkType pencilmarkType, bool locked) =>
            Sudoku.ParseJson(File.ReadAllText(GetPuzzlePath(name)), pencilmarkType, locked);
    }
}
