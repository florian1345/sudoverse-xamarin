using System.Runtime.InteropServices;

namespace Engine
{
    public static class EngineSo
    {
        // TODO figure out how to link different SOs depending on architecture

        const string DllName = "Engine/lib/arm64-v8a/libengine.so";

        [DllImport(DllName, EntryPoint = "test")]
        public static extern int Test();
    }
}
