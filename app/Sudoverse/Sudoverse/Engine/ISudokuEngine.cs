namespace Sudoverse.Engine
{
    public interface ISudokuEngine
    {
        int Test();

        string GenDefault(int difficulty);

        bool CheckDefault(string json);
    }
}
