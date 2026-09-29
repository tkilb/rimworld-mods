using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    public static class Patch_RecipeImplantXenogerm
    {
        public static bool IsConstruct(Pawn pawn)
        {
            return ConstructUtility.IsConstruct(pawn);
        }
    }

    [HarmonyPatch(typeof(RecipeWorker), nameof(RecipeWorker.AvailableReport))]
    public static class Patch_RecipeWorker_AvailableReport
    {
        public static void Postfix(RecipeWorker __instance, Thing thing, ref AcceptanceReport __result)
        {
            if (!(__instance is Recipe_ImplantXenogerm)) return;

            Pawn pawn = thing as Pawn;
            if (pawn != null && Patch_RecipeImplantXenogerm.IsConstruct(pawn))
            {
                __result = new AcceptanceReport("Cannot implant xenogerm: Synthetic construct genetic architecture is permanently locked");
            }
        }
    }

    [HarmonyPatch(typeof(Recipe_ImplantXenogerm), nameof(Recipe_ImplantXenogerm.AvailableOnNow))]
    public static class Patch_RecipeImplantXenogerm_AvailableOnNow
    {
        public static void Postfix(Thing thing, ref bool __result)
        {
            if (!__result) return;

            Pawn pawn = thing as Pawn;
            if (pawn != null && Patch_RecipeImplantXenogerm.IsConstruct(pawn))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Recipe_ImplantXenogerm), nameof(Recipe_ImplantXenogerm.ApplyOnPawn))]
    public static class Patch_RecipeImplantXenogerm_ApplyOnPawn
    {
        public static bool Prefix(Pawn pawn)
        {
            if (Patch_RecipeImplantXenogerm.IsConstruct(pawn))
            {
                Messages.Message("Cannot implant xenogerm: Synthetic construct genetic architecture is permanently locked", pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }
    }
}
