namespace Sudoverse.Engine
{
    public interface ISudokuEngine
    {
        int Test();

        string GenDefault();

        bool CheckDefault(string json);
    }
}
