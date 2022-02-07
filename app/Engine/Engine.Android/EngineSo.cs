using System.Runtime.InteropServices;

namespace Engine
{
    public static class EngineSo
    {
        const string DllName = "libengine.so";

        [DllImport(DllName, EntryPoint = "test")]
        public static extern int Test();

        [DllImport(DllName, EntryPoint = "gen")]
        public static extern string Gen(int constraint, int difficulty);

        [DllImport(DllName, EntryPoint = "check")]
        public static extern string Check(string json);
    }
}
