using System.Runtime.InteropServices;

namespace Engine
{
    public static class EngineSo
    {
        const string DllName = "libengine.so";

        [DllImport(DllName, EntryPoint = "test")]
        public static extern int Test();

        [DllImport(DllName, EntryPoint = "gen_default")]
        public static extern string GenDefault();

        [DllImport(DllName, EntryPoint = "check_default")]
        public static extern bool CheckDefault(string json);
    }
}
