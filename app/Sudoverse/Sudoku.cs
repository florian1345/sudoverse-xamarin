namespace Sudoverse
{
    internal class Sudoku
    {
        internal int BlockWidth { get; private set; }
        internal int BlockHeight { get; private set; }
        internal int Size { get; private set; }

        private int[] cells;

        internal Sudoku(int blockWidth, int blockHeight)
        {
            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
            Size = BlockWidth * BlockHeight;
            cells = new int[Size * Size];
        }

        internal int GetCell(int x, int y)
        {
            return cells[y * BlockWidth * BlockHeight + x];
        }
    }
}
