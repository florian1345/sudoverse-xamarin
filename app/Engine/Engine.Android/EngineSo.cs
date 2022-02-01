using System.Runtime.InteropServices;

namespace Engine
{
    public static class EngineSo
    {
        #if ARM64
            const string DllName = "Engine/lib/arm64-v8a/libengine.so";
        #elif ARM
            const string DllName = "Engine/lib/armeabi-v7a/libengine.so";
        #elif X64
            const string DllName = "Engine/lib/x86_64/libengine.so";
        #else
            const string DllName = "Engine/lib/x86/libengine.so";
        #endif

        [DllImport(DllName, EntryPoint = "test")]
        public static extern int Test();

        [DllImport(DllName, EntryPoint = "gen_default")]
        public static extern string GenDefault();

        [DllImport(DllName, EntryPoint = "check_default")]
        public static extern bool CheckDefault(string json);
    }
}
