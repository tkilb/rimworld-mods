using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    [HarmonyPatch(typeof(ThoughtWorker_Expectations), "CurrentStateInternal")]
    public static class Patch_ThoughtWorker_Expectations
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref ThoughtState __result)
        {
            if (p != null && ConstructUtility.IsConstruct(p))
            {
                __result = ThoughtState.Inactive;
            }
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_ExpectationsSlave), "CurrentStateInternal")]
    public static class Patch_ThoughtWorker_ExpectationsSlave
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref ThoughtState __result)
        {
            if (p != null && ConstructUtility.IsConstruct(p))
            {
                __result = ThoughtState.Inactive;
            }
        }
    }
}
