using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace Sudoverse.Constraint
{
    /// <summary>
    /// An enumeration of the different preferences with regards to placement a <see cref="Frame"/>
    /// can have.
    /// </summary>
    public enum FramePlacementPreference
    {
        /// <summary>
        /// Place the frame as close to the Sudoku grid as possible.
        /// </summary>
        Inner = 0,

        /// <summary>
        /// Place the frame as far away from the Sudoku grid as possible.
        /// </summary>
        Outer = 2,

        /// <summary>
        /// Place the frame wherever fits best.
        /// </summary>
        NoPreference = 1
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
    /// An exception raised if the size of a frame line is invalid, that is, it is less than 1.
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
    /// A frame around the Sudoku grid, which provides additional information, such as sandwich
    /// clues in a Sandwich Sudoku or mini killer sums. A frame consists of up to four lines, each
    /// containing a list of <see cref="View"/>s to put in that line. In addition, there are up to
    /// four corners each containing one <see cref="View"/>.
    /// </summary>
    public sealed class Frame : IComparable<Frame>
    {
        private View[] topLine;
        private View[] leftLine;
        private View[] rightLine;
        private View[] bottomLine;

        /// <summary>
        /// The line to put on top of the grid, from left to right. Empty if there is no such line.
        /// </summary>
        public ReadOnlyCollection<View> TopLine => new ReadOnlyCollection<View>(topLine);

        /// <summary>
        /// The line to put to the left of the grid, from top to bottom. Empty if there is no such
        /// line.
        /// </summary>
        public ReadOnlyCollection<View> LeftLine => new ReadOnlyCollection<View>(leftLine);

        /// <summary>
        /// The lines to put to the right of the grid, from top to bottom. Empty if there is no
        /// such line.
        /// </summary>
        public ReadOnlyCollection<View> RightLine => new ReadOnlyCollection<View>(rightLine);

        /// <summary>
        /// The lines to put below of the grid, from left to right. Empty if there is no such line.
        /// </summary>
        public ReadOnlyCollection<View> BottomLine => new ReadOnlyCollection<View>(bottomLine);

        /// <summary>
        /// The <see cref="View"/> to put in the top-left outside corner of the grid, or
        /// <tt>null</tt> if there is no such view.
        /// </summary>
        public View TopLeftCorner { get; private set; }

        /// <summary>
        /// The <see cref="View"/> to put in the top-right outside corner of the grid, or
        /// <tt>null</tt> if there is no such view.
        /// </summary>
        public View TopRightCorner { get; private set; }

        /// <summary>
        /// The <see cref="View"/> to put in the bottom-left outside corner of the grid, or
        /// <tt>null</tt> if there is no such view.
        /// </summary>
        public View BottomLeftCorner { get; private set; }

        /// <summary>
        /// The <see cref="View"/> to put in the bottom-right outside corner of the grid, or
        /// <tt>null</tt> if there is no such view.
        /// </summary>
        public View BottomRightCorner { get; private set; }

        /// <summary>
        /// Indicates whether this frame requires space above the Sudoku.
        /// </summary>
        public bool RequiresTopSpace =>
            topLine.Length > 0 || TopLeftCorner != null || TopRightCorner != null;

        /// <summary>
        /// Indicates whether this frame requires space to the left of the Sudoku.
        /// </summary>
        public bool RequiresLeftSpace =>
            leftLine.Length > 0 || TopLeftCorner != null || BottomLeftCorner != null;

        /// <summary>
        /// Indicates whether this frame requires space to the right of the Sudoku.
        /// </summary>
        public bool RequiresRightSpace =>
            rightLine.Length > 0 || TopRightCorner != null || BottomRightCorner != null;

        /// <summary>
        /// Indicates whether this frame requires space below the Sudoku.
        /// </summary>
        public bool RequiresBottomSpace =>
            bottomLine.Length > 0 || BottomLeftCorner != null || BottomRightCorner != null;

        /// <summary>
        /// The required size of the Sudoku that can be surrounded by this frame. If 0, there is no
        /// size requirement.
        /// </summary>
        public int SudokuSize { get; private set; }

        public FramePlacementPreference PlacementPreference { get; private set; }

        private Frame()
        {
            topLine = new View[0];
            leftLine = new View[0];
            rightLine = new View[0];
            bottomLine = new View[0];
            TopLeftCorner = null;
            TopRightCorner = null;
            BottomLeftCorner = null;
            BottomRightCorner = null;
            SudokuSize = 0;
            PlacementPreference = FramePlacementPreference.NoPreference;
        }

        public int CompareTo(Frame other) =>
            PlacementPreference.CompareTo(other.PlacementPreference);

        /// <summary>
        /// A builder for frames.
        /// </summary>
        public sealed class Builder
        {
            private Frame frame;

            /// <summary>
            /// Creates a new builder that initially has an empty frame.
            /// </summary>
            public Builder()
            {
                frame = new Frame();
            }

            private Builder WithLine(View[] line, ref View[] toSet)
            {
                if (line.Length < 1)
                    throw new InvalidFrameLineSizeException();

                if (frame.SudokuSize > 0)
                {
                    if (frame.SudokuSize != line.Length)
                        throw new FrameLineSizeInconsistencyException();
                }
                else
                {
                    frame.SudokuSize = line.Length;
                }

                toSet = line;
                return this;
            }

            /// <summary>
            /// Sets the views to be displayed in the top line of the constructed frame. Gaps are
            /// represented by <tt>null</tt> entries. To not display a top line, simply do not call
            /// this method. The length of the line must be non-zero and equal to the length of all
            /// previously specified lines.
            /// </summary>
            public Builder WithTopLine(View[] line) => WithLine(line, ref frame.topLine);

            /// <summary>
            /// Sets the views to be displayed in the left line of the constructed frame. Gaps are
            /// represented by <tt>null</tt> entries. To not display a left line, simply do not
            /// call this method. The length of the line must be non-zero and equal to the length
            /// of all previously specified lines.
            /// </summary>
            public Builder WithLeftLine(View[] line) => WithLine(line, ref frame.leftLine);

            /// <summary>
            /// Sets the views to be displayed in the right line of the constructed frame. Gaps are
            /// represented by <tt>null</tt> entries. To not display a right line, simply do not
            /// call this method. The length of the line must be non-zero and equal to the length
            /// of all previously specified lines.
            /// </summary>
            public Builder WithRightLine(View[] line) => WithLine(line, ref frame.rightLine);

            /// <summary>
            /// Sets the views to be displayed in the bottom line of the constructed frame. Gaps
            /// are represented by <tt>null</tt> entries. To not display a bottom line, simply do
            /// not call this method. The length of the line must be non-zero and equal to the
            /// length of all previously specified lines.
            /// </summary>
            public Builder WithBottomLine(View[] line) => WithLine(line, ref frame.bottomLine);

            /// <summary>
            /// Sets the view to be displayed in the top-left corner of the constructed frame. To
            /// not display a top-left corner, simply do not call this method.
            /// </summary>
            public Builder WithTopLeftCorner(View corner)
            {
                frame.TopLeftCorner = corner;
                return this;
            }

            /// <summary>
            /// Sets the view to be displayed in the top-right corner of the constructed frame. To
            /// not display a top-right corner, simply do not call this method.
            /// </summary>
            public Builder WithTopRightCorner(View corner)
            {
                frame.TopRightCorner = corner;
                return this;
            }

            /// <summary>
            /// Sets the view to be displayed in the bottom-left corner of the constructed frame.
            /// To not display a bottom-left corner, simply do not call this method.
            /// </summary>
            public Builder WithBottomLeftCorner(View corner)
            {
                frame.BottomLeftCorner = corner;
                return this;
            }

            /// <summary>
            /// Sets the view to be displayed in the bottom-right corner of the constructed frame.
            /// To not display a bottom-right corner, simply do not call this method.
            /// </summary>
            public Builder WithBottomRightCorner(View corner)
            {
                frame.BottomRightCorner = corner;
                return this;
            }

            /// <summary>
            /// Sets the constructed frame's <see cref="FramePlacementPreference"/>, which
            /// determines whether it will be more towards the inside or towards the outside of a
            /// grid surrounded by multiple frames.
            /// </summary>
            public Builder WithPlacementPreference(FramePlacementPreference placementPreference)
            {
                frame.PlacementPreference = placementPreference;
                return this;
            }

            /// <summary>
            /// Builds the frame.
            /// </summary>
            /// <returns>A frame constructed from the previously specified arguments.</returns>
            /// <exception cref="FrameAlreadyBuiltException">If this method has already been called
            /// on this builder.</exception>
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

    /// <summary>
    /// A group of <see cref="Frame"/>s to surround a Sudoku, which is by invariant sorted by
    /// placement preference.
    /// </summary>
    public sealed class FrameGroup : IEnumerable<Frame>
    {
        private Frame[] frames;

        /// <summary>
        /// The frames sorted from inner to outer.
        /// </summary>
        public ReadOnlyCollection<Frame> Frames => new ReadOnlyCollection<Frame>(frames);
        
        /// <summary>
        /// The amount of space required above the Sudoku in cells.
        /// </summary>
        public int TopSpace { get; }

        /// <summary>
        /// The amount of space required to the left of the Sudoku in cells.
        /// </summary>
        public int LeftSpace { get; }

        /// <summary>
        /// The amount of space required to the right of the Sudoku in cells.
        /// </summary>
        public int RightSpace { get; }

        /// <summary>
        /// The amount of space required below the Sudoku in cells.
        /// </summary>
        public int BottomSpace { get; }

        private FrameGroup(Frame[] frames)
        {
            this.frames = frames;

            TopSpace = frames.Where(f => f.RequiresTopSpace).Count();
            LeftSpace = frames.Where(f => f.RequiresLeftSpace).Count();
            RightSpace = frames.Where(f => f.RequiresRightSpace).Count();
            BottomSpace = frames.Where(f => f.RequiresBottomSpace).Count();
        }

        /// <summary>
        /// Constructs a new frame group without frames.
        /// </summary>
        public static FrameGroup Empty() => new FrameGroup(new Frame[0]);

        /// <summary>
        /// Constructs a new frame group that contains only the provided frame.
        /// </summary>
        public static FrameGroup Singleton(Frame frame) => new FrameGroup(new Frame[1] { frame });

        /// <summary>
        /// Constructs a new frame group that contains all the frames present in the provided frame
        /// groups. They are re-sorted to maintain inner-to-outer order.
        /// </summary>
        public static FrameGroup Combine(FrameGroup a, FrameGroup b)
        {
            // TODO simplify computation of required space

            int aFrameCount = a.frames.Length;
            int bFrameCount = b.frames.Length;
            int frameCount = aFrameCount + bFrameCount;
            var frames = new Frame[frameCount];
            int aIndex = 0;
            int bIndex = 0;
            int i = 0;

            for (; i < frameCount && aIndex < aFrameCount && bIndex < bFrameCount; i++)
            {
                if (a.frames[aIndex].CompareTo(b.frames[bIndex]) <= 0)
                {
                    frames[i] = a.frames[aIndex];
                    aIndex++;
                }
                else
                {
                    frames[i] = b.frames[bIndex];
                    bIndex++;
                }
            }

            if (aIndex < aFrameCount)
                Array.Copy(a.frames, aIndex, frames, i, aFrameCount - aIndex);
            else if (bIndex < bFrameCount)
                Array.Copy(b.frames, bIndex, frames, i, bFrameCount - bIndex);

            return new FrameGroup(frames);
        }

        public IEnumerator<Frame> GetEnumerator() =>
            frames.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            frames.GetEnumerator();
    }
}
