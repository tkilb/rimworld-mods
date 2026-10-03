using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    [HarmonyPatch(typeof(LifeStageWorker_HumanlikeChild), nameof(LifeStageWorker_HumanlikeChild.Notify_LifeStageStarted))]
    public static class Patch_LifeStageWorker_HumanlikeChild
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn)
        {
            if (ConstructUtility.IsConstruct(pawn))
            {
                pawn.health.capacities.Notify_CapacityLevelsDirty();
                ConstructUtility.AssignConstructBackstories(pawn);
                pawn.Notify_DisabledWorkTypesChanged();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LifeStageWorker_HumanlikeAdult), nameof(LifeStageWorker_HumanlikeAdult.Notify_LifeStageStarted))]
    public static class Patch_LifeStageWorker_HumanlikeAdult
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn)
        {
            if (ConstructUtility.IsConstruct(pawn))
            {
                pawn.health.capacities.Notify_CapacityLevelsDirty();
                ConstructUtility.AssignConstructBackstories(pawn);
                pawn.Notify_DisabledWorkTypesChanged();
                return false;
            }
            return true;
        }
    }
}
