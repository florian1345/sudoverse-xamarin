using System.Runtime.InteropServices;

namespace Engine
{
    public static class EngineDll
    {
        #if X64
            const string DllName = "Engine/lib/x86_64/engine.dll";
        #else
            const string DllName = "Engine/lib/x86/engine.dll";
        #endif

        [DllImport(DllName, EntryPoint = "test")]
        public static extern int Test();

        [DllImport(DllName, EntryPoint = "gen_default")]
        public static extern string GenDefault();

        [DllImport(DllName, EntryPoint = "check_default")]
        public static extern bool CheckDefault(string json);
    }
}
