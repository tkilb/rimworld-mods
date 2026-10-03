using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    [HarmonyPatch(typeof(LifeStageWorker_HumanlikeChild), nameof(LifeStageWorker_HumanlikeChild.Notify_LifeStageStarted))]
    public static class Patch_LifeStageWorker_HumanlikeChild
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn)
        {
            if (ConstructUtility.IsConstruct(pawn))
            {
                ConstructUtility.AssignConstructBackstories(pawn);
            }
        }
    }

    [HarmonyPatch(typeof(LifeStageWorker_HumanlikeAdult), nameof(LifeStageWorker_HumanlikeAdult.Notify_LifeStageStarted))]
    public static class Patch_LifeStageWorker_HumanlikeAdult
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn)
        {
            if (ConstructUtility.IsConstruct(pawn))
            {
                ConstructUtility.AssignConstructBackstories(pawn);
            }
        }
    }
}
