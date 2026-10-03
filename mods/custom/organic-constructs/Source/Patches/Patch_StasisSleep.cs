using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    /// <summary>
    /// Ensures stasis is a dead sleep with no disturbed sleep, no premature awakening,
    /// and no negative mood debuffs while hibernating.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "CheckForDisturbedSleep")]
    public static class Patch_Pawn_CheckForDisturbedSleep
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn __instance)
        {
            var gene = __instance?.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && gene.inStasis)
            {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Being downed calls Pawn_JobTracker.StopAll(ifLayingKeepLaying: true), which only spares the vanilla
    /// LayDown job when not in a bed. Treat the stasis job as a laying job so it survives being downed.
    /// </summary>
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.IsLayingForJobCleanup))]
    public static class Patch_RestUtility_IsLayingForJobCleanup
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref bool __result)
        {
            if (!__result && p?.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis)
            {
                var gene = p.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && gene.inStasis && p.GetPosture().Laying())
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.ShouldWakeUp))]
    public static class Patch_RestUtility_ShouldWakeUp
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, ref bool __result)
        {
            var gene = pawn?.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && gene.inStasis)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.CanFallAsleep))]
    public static class Patch_RestUtility_CanFallAsleep
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, ref bool __result)
        {
            var gene = pawn?.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && gene.inStasis)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.TimetablePreventsLayDown))]
    public static class Patch_RestUtility_TimetablePreventsLayDown
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, ref bool __result)
        {
            var gene = pawn?.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && gene.inStasis)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.WakeUp))]
    public static class Patch_RestUtility_WakeUp
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn p)
        {
            var gene = p?.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && gene.inStasis)
            {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Prevents ThinkTree jobs (e.g. dawn Work schedule or Joy checks) from interrupting stasis.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_JobTracker), "ShouldStartJobFromThinkTree")]
    public static class Patch_Pawn_JobTracker_ShouldStartJobFromThinkTree
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn ___pawn, ref bool __result)
        {
            if (___pawn?.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis)
            {
                var gene = ___pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && gene.inStasis)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// When carried or rescued to a bed while in stasis, assign Construct_EnterConstructStasis
    /// rather than vanilla LayDown (matching Deathrest parity).
    /// </summary>
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.Notify_TuckedIntoBed))]
    public static class Patch_Pawn_JobTracker_Notify_TuckedIntoBed
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn_JobTracker __instance, Pawn ___pawn, Building_Bed bed)
        {
            var gene = ___pawn?.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && gene.inStasis)
            {
                ___pawn.Position = RestUtility.GetBedSleepingSlotPosFor(___pawn, bed);
                ___pawn.Notify_Teleported(endCurrentJob: false);
                ___pawn.stances.CancelBusyStanceHard();
                Job job = JobMaker.MakeJob(ConstructDefOf.Construct_EnterConstructStasis, bed);
                job.forceSleep = true;
                __instance.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, JobTag.TuckedIntoBed, fromQueue: false, canReturnCurJobToPool: false, null, continueSleeping: true);
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// If an idle/downed construct is in stasis, give them Construct_EnterConstructStasis
    /// instead of Wait_Downed so they stay in stasis.
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_IdleForever), "TryGiveJob")]
    public static class Patch_JobGiver_IdleForever_TryGiveJob
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            var gene = pawn?.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && gene.inStasis)
            {
                Building_Bed bed = pawn.CurrentBed();
                Job job = bed != null
                    ? JobMaker.MakeJob(ConstructDefOf.Construct_EnterConstructStasis, bed)
                    : JobMaker.MakeJob(ConstructDefOf.Construct_EnterConstructStasis, pawn.Position);
                job.forceSleep = true;
                __result = job;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Freezes bleeding while in hibernation stasis (matching Deathrest parity).
    /// </summary>
    [HarmonyPatch(typeof(HediffSet), "CalculateBleedRate")]
    public static class Patch_HediffSet_CalculateBleedRate
    {
        [HarmonyPostfix]
        public static void Postfix(HediffSet __instance, ref float __result)
        {
            if (__result > 0f && __instance?.pawn != null)
            {
                var gene = __instance.pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && gene.inStasis)
                {
                    __result = 0f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new[] { typeof(Thought_Memory), typeof(Pawn) })]
    public static class Patch_MemoryThoughtHandler_TryGainMemory
    {
        [HarmonyPrefix]
        public static bool Prefix(MemoryThoughtHandler __instance, Thought_Memory newThought)
        {
            if (__instance?.pawn != null && newThought?.def != null)
            {
                var gene = __instance.pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && gene.inStasis)
                {
                    // Suppress any negative mood memories while in hibernation stasis
                    if (newThought.MoodOffset() < 0f)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Need_Mood), nameof(Need_Mood.NeedInterval))]
    public static class Patch_Need_Mood_NeedInterval
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn ___pawn)
        {
            if (___pawn != null)
            {
                var gene = ___pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && gene.inStasis)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Stops the pawn's timetable (e.g. the Sleep -> Anything/Work transition at dawn), rest level, hunger or
    /// disturbances from pulling it out of stasis. While in stasis the pawn must always keep lying down.
    /// </summary>
    [HarmonyPatch(typeof(ThinkNode_ConditionalMustKeepLyingDown), "Satisfied")]
    public static class Patch_ThinkNode_ConditionalMustKeepLyingDown
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (__result || pawn?.CurJob == null || pawn.CurJobDef != ConstructDefOf.Construct_EnterConstructStasis)
            {
                return;
            }
            var gene = pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && gene.inStasis && pawn.GetPosture().Laying())
            {
                __result = true;
            }
        }
    }
}
