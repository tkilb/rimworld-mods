using Verse;

namespace TestCsharp
{
    public class TestCsharpMod : Mod
    {
        public TestCsharpMod(ModContentPack content) : base(content)
        {
            Log.Message("[Test Csharp] Initialized successfully.");
        }
    }
}
