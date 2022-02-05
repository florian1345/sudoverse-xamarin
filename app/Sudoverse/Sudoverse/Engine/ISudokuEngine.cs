namespace Sudoverse.Engine
{
    public interface ISudokuEngine
    {
        int Test();

        string Gen(int constraint, int difficulty);

        bool Check(string json);
    }
}
