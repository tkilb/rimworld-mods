using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    /// <summary>
    /// Prevents apparel worn by constructs from becoming tainted (WornByCorpse) upon death.
    /// </summary>
    [HarmonyPatch(typeof(Apparel), nameof(Apparel.Notify_PawnKilled))]
    public static class Patch_Apparel_Notify_PawnKilled
    {
        [HarmonyPrefix]
        public static bool Prefix(Apparel __instance)
        {
            Pawn wearer = __instance.Wearer;
            if (wearer == null && __instance.ParentHolder is Corpse corpse)
            {
                wearer = corpse.InnerPawn;
            }
            else if (wearer == null && __instance.ParentHolder is Pawn_ApparelTracker tracker)
            {
                wearer = tracker.pawn;
            }

            if (wearer != null && ConstructUtility.IsConstruct(wearer))
            {
                // Still notify comps that the wearer died (e.g. biocoded gear, death effects)
                foreach (ThingComp comp in __instance.AllComps)
                {
                    comp.Notify_WearerDied();
                }
                // Skip marking wornByCorpseInt = true
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Safeguard to ensure all worn apparel on a construct corpse remains untainted.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_PawnKilled))]
    public static class Patch_Pawn_ApparelTracker_Notify_PawnKilled
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_ApparelTracker __instance)
        {
            if (__instance?.pawn != null && ConstructUtility.IsConstruct(__instance.pawn))
            {
                foreach (Apparel ap in __instance.WornApparel)
                {
                    ap.WornByCorpse = false;
                }
            }
        }
    }
}
