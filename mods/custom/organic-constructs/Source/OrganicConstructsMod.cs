using HarmonyLib;
using Verse;

namespace OrganicConstructs
{
    public class OrganicConstructsMod : Mod
    {
        public const string PackageId = "tyler.organicconstructs";

        public OrganicConstructsMod(ModContentPack content) : base(content)
        {
        }
    }

    [StaticConstructorOnStartup]
    public static class OrganicConstructsModInit
    {
        static OrganicConstructsModInit()
        {
            var harmony = new Harmony(OrganicConstructsMod.PackageId);
            harmony.PatchAll();
            Log.Message("[Organic Constructs] Mod loaded and Harmony patches initialized.");
        }
    }
}
