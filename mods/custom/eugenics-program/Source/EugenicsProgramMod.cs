using HarmonyLib;
using Verse;

namespace EugenicsProgram
{
    public class EugenicsProgramMod : Mod
    {
        public const string PackageId = "tylerkilburn.eugenicsprogram";

        public EugenicsProgramMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(PackageId);
            harmony.PatchAll();
            Log.Message("[Eugenics Program] Mod loaded and Harmony patches initialized.");
        }
    }
}
