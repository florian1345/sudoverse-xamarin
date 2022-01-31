using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Sudoverse.Constraint
{
    /// <summary>
    /// An enumeration of the different preferences with regards to placement a
    /// <see cref="FrameLine"/> can have.
    /// </summary>
    public enum FrameLinePlacementPreference
    {
        /// <summary>
        /// Place the line as close to the Sudoku grid as possible.
        /// </summary>
        Inner,

        /// <summary>
        /// Place the line as far away from the Sudoku grid as possible.
        /// </summary>
        Outer,

        /// <summary>
        /// Place the line wherever fits best.
        /// </summary>
        NoPreference
    }

    /// <summary>
    /// A line of information outside the grid, such as the sandwich clues in Sandwich Sudoku or
    /// mini killer sums. This contains only information about one line, i.e. a column to the
    /// left/right or a row at the top/bottom of the grid. The position itself is not stored within
    /// this class.
    ///
    /// Multiple frame lines constitute a <see cref="Frame"/>.
    /// </summary>
    public sealed class FrameLine
    {
        /// <summary>
        /// If this is true, the line requires a field on the corner outside the Sudoku grid, which
        /// is placed at the lower coordinates in the direction of the line.
        /// </summary>
        public bool WithStartCorner { get; }

        /// <summary>
        /// If this is true, the line requires a field on the corner outside the Sudoku grid, which
        /// is placed at the higher coordinates in the direction of the line.
        /// </summary>
        public bool WithEndCorner { get; }

        /// <summary>
        /// Indicates the preference this line has as to how close to the grid it should be placed.
        /// </summary>
        public FrameLinePlacementPreference PlacementPreference { get; }

        /// <summary>
        /// An array containing the Xamarin.Forms <see cref="View"/>s that constitute the frame
        /// line. If any position should not have any view in it, its position is occupied by
        /// <tt>null</tt> in this array.
        /// </summary>
        public ReadOnlyCollection<View> Views { get; }

        /// <summary>
        /// Creates a new frame line with the specified parameters.
        /// </summary>
        /// <param name="withStartCorner">If this is true, the line requires a field on the corner
        /// outside the Sudoku grid, which s placed at the lower coordinates in the direction of
        /// the line.</param>
        /// <param name="withEndCorner">If this is true, the line requires a field on the corner
        /// outside the Sudoku grid, which s placed at the higher coordinates in the direction of
        /// the line.</param>
        /// <param name="placementPreference">Indicates the preference this line has as to how
        /// close to the grid it should be placed.</param>
        /// <param name="views">An array containing the Xamarin.Forms <see cref="View"/>s that
        /// constitute the frame line. If any position should not have any view in it,
        /// <tt>null</tt> must occupy its position in this array.</param>
        public FrameLine(bool withStartCorner, bool withEndCorner, FrameLinePlacementPreference placementPreference, View[] views)
        {
            WithStartCorner = withStartCorner;
            WithEndCorner = withEndCorner;
            PlacementPreference = placementPreference;
            Views = Array.AsReadOnly(views);
        }
    }

    /// <summary>
    /// An exception raised if <see cref="Frame.Builder.Build"/> is called twice on the same
    /// instance.
    /// </summary>
    internal sealed class FrameAlreadyBuiltException : Exception
    {
        public FrameAlreadyBuiltException()
            : base("Frame construction has already finished.") { }
    }

    /// <summary>
    /// An exception raised if the size of a frame line is invalid, that is, it would surround a
    /// Sudoku of size 0 or less.
    /// </summary>
    internal sealed class InvalidFrameLineSizeException : Exception
    {
        public InvalidFrameLineSizeException()
            : base("Frame line has invalid size.") { }
    }

    /// <summary>
    /// An exception raised if the sizes of at least two frame lines are not consistent for a
    /// square Sudoku grid they surround.
    /// </summary>
    internal sealed class FrameLineSizeInconsistencyException : Exception
    {
        public FrameLineSizeInconsistencyException()
            : base("At least two frame lines have inconsistent sizes.") { }
    }

    /// <summary>
    /// An exception raised if at least two frame lines want to occupy the same corner, e.g. both
    /// the left and top line want to have their starting corner (which would be the top-left
    /// corner).
    /// </summary>
    internal sealed class FrameLineCollisionException : Exception
    {
        public FrameLineCollisionException()
            : base("At least two frame lines collide in a corner.") { }
    }

    /// <summary>
    /// A frame around the Sudoku grid, which provides additional information in
    /// <see cref="FrameLine"/>s, such as sandwich clues in a Sandwich Sudoku or mini killer sums.
    /// A frame consists of at most four frame lines - one to each side - but each side may or may
    /// not have a line. In the other extreme, a frame can be empty, which is the default for any
    /// constraint that does not have extra information to place outside the grid.
    /// </summary>
    public sealed class Frame
    {
        /// <summary>
        /// The line to put on top of the grid or <tt>null</tt>, if there is no such line.
        /// </summary>
        public FrameLine TopLine { get; private set; } = null;

        /// <summary>
        /// The line to put to the left of the grid or <tt>null</tt>, if there is no such line.
        /// </summary>
        public FrameLine LeftLine { get; private set; } = null;

        /// <summary>
        /// The line to put to the right of the grid or <tt>null</tt>, if there is no such line.
        /// </summary>
        public FrameLine RightLine { get; private set; } = null;

        /// <summary>
        /// The line to put below of the grid or <tt>null</tt>, if there is no such line.
        /// </summary>
        public FrameLine BottomLine { get; private set; } = null;

        /// <summary>
        /// The required size of the Sudoku that can be surrounded by this frame. If 0, there is no
        /// size requirement.
        /// </summary>
        public int SudokuSize { get; private set; }

        private Frame() { }

        public static Frame Empty() =>
            new Builder().Build();

        public sealed class Builder
        {
            private Frame frame;
            private bool topLeftOccupied;
            private bool topRightOccupied;
            private bool bottomLeftOccupied;
            private bool bottomRightOccupied;

            public Builder()
            {
                frame = new Frame();
                topLeftOccupied = false;
                topRightOccupied = false;
                bottomLeftOccupied = false;
                bottomRightOccupied = false;
            }

            private Builder WithLine(FrameLine frameLine, ref bool startCorner, ref bool endCorner)
            {
                int sudokuSize = frameLine.Views.Count;

                if (frameLine.WithStartCorner)
                {
                    if (startCorner) throw new FrameLineCollisionException();
                    else startCorner = true;

                    sudokuSize -= 1;
                }

                if (frameLine.WithEndCorner)
                {
                    if (endCorner) throw new FrameLineCollisionException();
                    else endCorner = true;

                    sudokuSize -= 1;
                }

                if (sudokuSize < 1)
                    throw new InvalidFrameLineSizeException();

                if (frame.SudokuSize > 0)
                {
                    if (frame.SudokuSize != sudokuSize)
                        throw new FrameLineSizeInconsistencyException();
                }
                else
                {
                    frame.SudokuSize = sudokuSize;
                }

                return this;
            }

            public Builder WithTopLine(FrameLine frameLine) =>
                WithLine(frameLine, ref topLeftOccupied, ref topRightOccupied);

            public Builder WithLeftLine(FrameLine frameLine) =>
                WithLine(frameLine, ref topLeftOccupied, ref bottomLeftOccupied);

            public Builder WithRightLine(FrameLine frameLine) =>
                WithLine(frameLine, ref topRightOccupied, ref bottomRightOccupied);

            public Builder WithBottomLine(FrameLine frameLine) =>
                WithLine(frameLine, ref bottomLeftOccupied, ref bottomRightOccupied);

            public Frame Build()
            {
                if (frame == null)
                    throw new FrameAlreadyBuiltException();

                var result = frame;
                frame = null;
                return result;
            }
        }
    }
}
