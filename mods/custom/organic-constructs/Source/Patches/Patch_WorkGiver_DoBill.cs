using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    [HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
    public static class Patch_WorkGiver_DoBill
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Thing thing, bool forced, ref Job __result)
        {
            if (__result != null && thing is Building_ConstructSynthesizer synth)
            {
                Bill bill = __result.bill;
                if (bill?.recipe?.workerClass == typeof(Recipe_SynthesizeComplexEmbryo) && !synth.HasLoadedBlueprint)
                {
                    __result = null;
                    JobFailReason.Is("No genome blueprint disc loaded in synthesizer.");
                }
            }
        }
    }
}
