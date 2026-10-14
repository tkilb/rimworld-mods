using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    /// <summary>
    /// Integrates construct hibernation stasis into Biotech's native Deathrest pipeline.
    /// By returning true for Pawn.Deathresting during stasis, the engine natively handles:
    /// - Locking sleeper facing direction to South (Rot4.South) without sideways rotation.
    /// - Proper blanket tucking over the sleeper body.
    /// - Disturbed sleep suppression.
    /// - Freezing bleeding and food consumption.
    /// - Suppressing drafting and work/joy interruptions.
    /// - Preventing doctors from hauling pawns out of their assigned bed to medical beds.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Deathresting), MethodType.Getter)]
    public static class Patch_Pawn_Deathresting
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, ref bool __result)
        {
            if (__result || __instance == null) return;
            var gene = __instance.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && (gene.inStasis || __instance.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis)))
            {
                __result = true;
            }
        }
    }

    /// <summary>
    /// Tracks active Pawn.Strip execution so that intentional stripping by the player or enemies
    /// is never blocked by stasis equipment drop prevention.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Strip))]
    public static class Patch_Pawn_Strip
    {
        internal static bool isStripping = false;

        [HarmonyPrefix]
        public static void Prefix()
        {
            isStripping = true;
        }

        [HarmonyPostfix]
        [HarmonyFinalizer]
        public static void Postfix()
        {
            isStripping = false;
        }
    }

    /// <summary>
    /// Prevents constructs from dropping and forbidding their equipped weapons when entering hibernation stasis.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DropAndForbidEverything))]
    public static class Patch_Pawn_DropAndForbidEverything
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn __instance)
        {
            if (__instance == null || __instance.Dead || Patch_Pawn_Strip.isStripping)
            {
                return true;
            }
            if (__instance.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis)
            {
                return false;
            }
            var gene = __instance.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && (gene.inStasis || __instance.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis)))
            {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Prevents constructs from dropping equipped weapons due to manipulation capacity loss
    /// in Pawn_HealthTracker.CheckForStateChange when entering or remaining in hibernation stasis.
    /// Preserves normal drop behavior if the pawn is dead or actively being stripped.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment), new[] { typeof(ThingWithComps), typeof(ThingWithComps), typeof(IntVec3), typeof(bool) }, new[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal })]
    public static class Patch_Pawn_EquipmentTracker_TryDropEquipment
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn_EquipmentTracker __instance, ThingWithComps eq, ref ThingWithComps resultingEq, ref bool __result)
        {
            if (Patch_Pawn_Strip.isStripping)
            {
                return true;
            }

            Pawn pawn = __instance?.pawn;
            if (pawn != null && !pawn.Dead)
            {
                if (pawn.CurJobDef == JobDefOf.DropEquipment)
                {
                    return true;
                }
                if (pawn.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis)
                {
                    resultingEq = null;
                    __result = false;
                    return false;
                }
                var gene = pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && (gene.inStasis || pawn.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis)))
                {
                    resultingEq = null;
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Prevents constructs from dropping all equipped items when entering or remaining in hibernation stasis.
    /// Preserves normal drop behavior if the pawn is dead or actively being stripped.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.DropAllEquipment))]
    public static class Patch_Pawn_EquipmentTracker_DropAllEquipment
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn_EquipmentTracker __instance)
        {
            if (Patch_Pawn_Strip.isStripping)
            {
                return true;
            }

            Pawn pawn = __instance?.pawn;
            if (pawn != null && !pawn.Dead)
            {
                if (pawn.CurJobDef == JobDefOf.DropEquipment)
                {
                    return true;
                }
                if (pawn.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis)
                {
                    return false;
                }
                var gene = pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && (gene.inStasis || pawn.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis)))
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Safeguard: Prevents ThinkTree jobs (such as dawn Work or Joy schedules) from interrupting stasis.
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
    /// When carried or rescued to a bed while in stasis or emergency shutdown, assign Construct_EnterConstructStasis
    /// rather than vanilla LayDown or Deathrest.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.Notify_TuckedIntoBed))]
    public static class Patch_Pawn_JobTracker_Notify_TuckedIntoBed
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn_JobTracker __instance, Pawn ___pawn, Building_Bed bed)
        {
            var gene = ___pawn?.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && (gene.inStasis || ___pawn.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis) || gene.operatingTicks >= Gene_ConstructHibernation.MaxOperatingTicks))
            {
                ___pawn.Position = RestUtility.GetBedSleepingSlotPosFor(___pawn, bed);
                ___pawn.Notify_Teleported(endCurrentJob: false);
                ___pawn.stances.CancelBusyStanceHard();
                gene.inStasis = true;
                if (gene.stasisStartTick < 0)
                {
                    gene.stasisStartTick = Find.TickManager.TicksGame;
                    gene.stasisTicks = 0;
                    gene.notifiedWakeOK = false;
                }
                // Clear emergency shutdown coma if present so construct rests cleanly in stasis
                Hediff coma = ___pawn.health.hediffSet.GetFirstHediffOfDef(ConstructDefOf.Construct_Assimilation);
                if (coma != null)
                {
                    ___pawn.health.RemoveHediff(coma);
                }
                Job job = JobMaker.MakeJob(ConstructDefOf.Construct_EnterConstructStasis, bed);
                job.forceSleep = true;
                __instance.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, JobTag.TuckedIntoBed, fromQueue: false, canReturnCurJobToPool: false, null, continueSleeping: true);
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Suppresses any negative mood memories while in hibernation stasis.
    /// </summary>
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
                    if (newThought.pawn == null)
                    {
                        newThought.pawn = __instance.pawn;
                    }
                    if (newThought.MoodOffset() < 0f)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Freezes mood need decay while in hibernation stasis.
    /// </summary>
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
    /// Ensures Building_Bed recognizes a construct in stasis at a sleeping slot as its current occupant.
    /// </summary>
    [HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.GetCurOccupant))]
    public static class Patch_Building_Bed_GetCurOccupant
    {
        [HarmonyPostfix]
        public static void Postfix(Building_Bed __instance, int slotIndex, ref Pawn __result)
        {
            if (__result == null && __instance.Spawned)
            {
                IntVec3 slotPos = __instance.GetSleepingSlotPos(slotIndex);
                var things = slotPos.GetThingList(__instance.Map);
                for (int i = 0; i < things.Count; i++)
                {
                    if (things[i] is Pawn p)
                    {
                        var gene = p.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                        if (gene != null && (gene.inStasis || p.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis) || p.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis))
                        {
                            __result = p;
                            return;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Ensures that pawns in stasis occupying a bed are recognized as being in bed by RestUtility.CurrentBed.
    /// </summary>
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.CurrentBed), new[] { typeof(Pawn) })]
    public static class Patch_RestUtility_CurrentBed
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref Building_Bed __result)
        {
            if (__result == null && p != null && p.Spawned)
            {
                var gene = p.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && (gene.inStasis || p.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis) || p.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis))
                {
                    Building_Bed bed = p.CurJob?.GetTarget(TargetIndex.A).Thing as Building_Bed;
                    if (bed != null && bed.OccupiedRect().Contains(p.Position))
                    {
                        __result = bed;
                    }
                    else
                    {
                        __result = p.Position.GetThingList(p.Map).OfType<Building_Bed>().FirstOrDefault();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.CurrentBed), new[] { typeof(Pawn), typeof(int?) }, new[] { ArgumentType.Normal, ArgumentType.Out })]
    public static class Patch_RestUtility_CurrentBed_Slot
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref int? sleepingSlot, ref Building_Bed __result)
        {
            if (__result == null && p != null && p.Spawned)
            {
                var gene = p.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && (gene.inStasis || p.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis) || p.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis))
                {
                    Building_Bed bed = p.CurJob?.GetTarget(TargetIndex.A).Thing as Building_Bed;
                    if (bed != null && !bed.OccupiedRect().Contains(p.Position))
                    {
                        bed = null;
                    }
                    if (bed == null)
                    {
                        bed = p.Position.GetThingList(p.Map).OfType<Building_Bed>().FirstOrDefault();
                    }
                    if (bed != null)
                    {
                        __result = bed;
                        for (int i = 0; i < bed.SleepingSlotsCount; i++)
                        {
                            if (bed.GetSleepingSlotPos(i) == p.Position)
                            {
                                sleepingSlot = i;
                                return;
                            }
                        }
                        sleepingSlot = 0;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Ensures RestUtility.InBed returns true for constructs resting in any bed or sleeping spot.
    /// </summary>
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.InBed))]
    public static class Patch_RestUtility_InBed
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref bool __result)
        {
            if (__result || p == null || !p.Spawned) return;
            var gene = p.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && (gene.inStasis || p.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis)))
            {
                if (p.CurrentBed() != null || p.Position.GetThingList(p.Map).Any(t => t is Building_Bed))
                {
                    __result = true;
                }
            }
        }
    }

    /// <summary>
    /// Prevents constructs resting in stasis in a bed from indicating they want to be rescued.
    /// Downed constructs in the field outside of any bed will still return true and seek rescue.
    /// </summary>
    [HarmonyPatch(typeof(HealthAIUtility), nameof(HealthAIUtility.WantsToBeRescued))]
    public static class Patch_HealthAIUtility_WantsToBeRescued
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (!__result || pawn == null || !pawn.Spawned) return;
            var gene = pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && (gene.inStasis || pawn.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis)))
            {
                if (pawn.InBed() || pawn.CurrentBed() != null || pawn.Position.GetThingList(pawn.Map).Any(t => t is Building_Bed))
                {
                    __result = false;
                }
            }
        }
    }

    /// <summary>
    /// Ensures constructs seek medical rest when they have any untreated or healing injuries,
    /// tendable conditions, or missing health, even if their reduced pain and superclotting
    /// would otherwise cause vanilla's pain/bleed thresholds to evaluate to false.
    /// </summary>
    [HarmonyPatch(typeof(HealthAIUtility), nameof(HealthAIUtility.ShouldSeekMedicalRest))]
    public static class Patch_HealthAIUtility_ShouldSeekMedicalRest
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (__result || pawn == null) return;
            if (ConstructUtility.IsConstruct(pawn))
            {
                if (pawn.health.hediffSet.HasTendableHediff()
                    || pawn.health.hediffSet.BleedRateTotal > 0.001f
                    || pawn.health.hediffSet.hediffs.Any(h => h is Hediff_Injury injury && injury.CanHealNaturally()))
                {
                    __result = true;
                }
            }
        }
    }

    /// <summary>
    /// Suppresses the "Colonist needs rescue" alert banner when the construct is already in a bed in stasis.
    /// </summary>
    [HarmonyPatch(typeof(Alert_ColonistNeedsRescuing), "NeedsRescue")]
    public static class Patch_Alert_ColonistNeedsRescuing_NeedsRescue
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref bool __result)
        {
            if (!__result || p == null || !p.Spawned) return;
            var gene = p.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && (gene.inStasis || p.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis)))
            {
                if (p.InBed() || p.CurrentBed() != null || p.Position.GetThingList(p.Map).Any(t => t is Building_Bed))
                {
                    __result = false;
                }
            }
        }
    }

    /// <summary>
    /// Prevents doctors from issuing Rescue jobs on constructs already resting in a bed in stasis.
    /// </summary>
    [HarmonyPatch(typeof(WorkGiver_RescueDowned), nameof(WorkGiver_RescueDowned.HasJobOnThing))]
    public static class Patch_WorkGiver_RescueDowned_HasJobOnThing
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Thing t, ref bool __result)
        {
            if (!__result) return;
            if (t is Pawn patient && patient.Spawned)
            {
                var gene = patient.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && (gene.inStasis || patient.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis)))
                {
                    if (patient.InBed() || patient.CurrentBed() != null || patient.Position.GetThingList(patient.Map).Any(b => b is Building_Bed))
                    {
                        __result = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// When taking/rescuing a construct in stasis or emergency shutdown to bed, route them to their
    /// assigned personal bed rather than an unrelated hospital or medical bed.
    /// </summary>
    [HarmonyPatch(typeof(WorkGiver_TakeToBed), "FindBed")]
    public static class Patch_WorkGiver_TakeToBed_FindBed
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Pawn patient, ref Building_Bed __result)
        {
            if (patient != null)
            {
                var gene = patient.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && (gene.inStasis || patient.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis) || gene.operatingTicks >= Gene_ConstructHibernation.MaxOperatingTicks))
                {
                    Building_Bed ownedBed = patient.ownership?.OwnedBed;
                    if (ownedBed != null && !ownedBed.Medical && RestUtility.IsValidBedFor(ownedBed, patient, pawn, checkSocialProperness: false))
                    {
                        __result = ownedBed;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Dynamically regenerates rest need during hibernation stasis at standard bed rest rate
    /// until full (100%), and prevents rest decay or exhaustion while in stasis.
    /// </summary>
    [HarmonyPatch(typeof(Need_Rest), nameof(Need_Rest.NeedInterval))]
    public static class Patch_Need_Rest_NeedInterval
    {
        [HarmonyPrefix]
        public static bool Prefix(Need_Rest __instance, Pawn ___pawn)
        {
            if (___pawn == null || !___pawn.Spawned) return true;
            var gene = ___pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && (gene.inStasis || ___pawn.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis) || ___pawn.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis))
            {
                // Only gain rest if they are occupying a bed or sleeping spot
                if (___pawn.InBed() || ___pawn.CurrentBed() != null || ___pawn.Position.GetThingList(___pawn.Map).Any(t => t is Building_Bed))
                {
                    Building_Bed bed = ___pawn.CurrentBed() ?? ___pawn.Position.GetThingList(___pawn.Map).OfType<Building_Bed>().FirstOrDefault();
                    float restEffectiveness = ((bed == null || !bed.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness))
                        ? StatDefOf.BedRestEffectiveness.valueIfMissing
                        : bed.GetStatValue(StatDefOf.BedRestEffectiveness, applyPostProcess: true, 15));

                    float rate = restEffectiveness * ___pawn.GetStatValue(StatDefOf.RestRateMultiplier);
                    if (rate > 0f)
                    {
                        __instance.CurLevel = Mathf.Min(1f, __instance.CurLevel + (0.005714286f * rate));
                    }
                }
                // Skip vanilla NeedInterval during stasis to prevent rest decay and ticksAtZero accumulation
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Ensures Need_Rest.Resting reports true for constructs in stasis occupying a bed,
    /// so the UI change indicator shows active resting/recovery (arrow pointing up).
    /// </summary>
    [HarmonyPatch(typeof(Need_Rest), nameof(Need_Rest.Resting), MethodType.Getter)]
    public static class Patch_Need_Rest_Resting
    {
        [HarmonyPostfix]
        public static void Postfix(Need_Rest __instance, Pawn ___pawn, ref bool __result)
        {
            if (__result) return;
            if (___pawn == null || !___pawn.Spawned) return;
            var gene = ___pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene != null && (gene.inStasis || ___pawn.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis) || ___pawn.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis))
            {
                if (___pawn.InBed() || ___pawn.CurrentBed() != null || ___pawn.Position.GetThingList(___pawn.Map).Any(t => t is Building_Bed))
                {
                    __result = true;
                }
            }
        }
    }

    /// <summary>
    /// Restricts Building_ConstructStasisPod to constructs only. Natural humans cannot claim or use them.
    /// </summary>
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.CanUseBedEver))]
    public static class Patch_RestUtility_CanUseBedEver
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ThingDef bedDef, ref bool __result)
        {
            if (__result && bedDef?.defName == "ConstructStasisPod")
            {
                if (!ConstructUtility.IsConstruct(p))
                {
                    __result = false;
                }
            }
        }
    }

    /// <summary>
    /// Restricts candidate assignment list for ConstructStasisPod so only biological constructs can be selected.
    /// </summary>
    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), nameof(CompAssignableToPawn_Bed.AssigningCandidates), MethodType.Getter)]
    public static class Patch_CompAssignableToPawn_Bed_AssigningCandidates
    {
        [HarmonyPostfix]
        public static void Postfix(CompAssignableToPawn_Bed __instance, ref IEnumerable<Pawn> __result)
        {
            if (__result != null && __instance?.parent?.def?.defName == "ConstructStasisPod")
            {
                __result = __result.Where(ConstructUtility.IsConstruct);
            }
        }
    }

    /// <summary>
    /// Explicitly rejects assigning any non-construct pawn to a ConstructStasisPod.
    /// </summary>
    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), nameof(CompAssignableToPawn_Bed.CanAssignTo))]
    public static class Patch_CompAssignableToPawn_Bed_CanAssignTo
    {
        [HarmonyPostfix]
        public static void Postfix(CompAssignableToPawn_Bed __instance, Pawn pawn, ref AcceptanceReport __result)
        {
            if (__instance?.parent?.def?.defName == "ConstructStasisPod")
            {
                if (!ConstructUtility.IsConstruct(pawn))
                {
                    __result = new AcceptanceReport("Only biological constructs may use stasis pods.");
                }
            }
        }
    }

    /// <summary>
    /// Ensures ConstructStasisPod is never considered a valid bed for non-construct pawns (medical, rescue, or sleep).
    /// </summary>
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.IsValidBedFor), new[] { typeof(Thing), typeof(Pawn), typeof(Pawn), typeof(bool), typeof(bool), typeof(bool), typeof(GuestStatus?) })]
    public static class Patch_RestUtility_IsValidBedFor
    {
        [HarmonyPostfix]
        public static void Postfix(Thing bedThing, Pawn sleeper, ref bool __result)
        {
            if (__result && bedThing?.def?.defName == "ConstructStasisPod")
            {
                if (!ConstructUtility.IsConstruct(sleeper))
                {
                    __result = false;
                }
            }
        }
    }

    /// <summary>
    /// Shifts the root render matrix when resting in directional stasis pods to align with the glass aperture.
    /// In RimWorld 1.6, PawnRenderTree.ParallelPreDraw applies the root PawnDrawParms.matrix to all render nodes.
    /// Uses a single symmetrical distance D for all 4 orientations.
    /// </summary>
    [HarmonyPatch(typeof(PawnRenderTree), nameof(PawnRenderTree.ParallelPreDraw))]
    public static class Patch_PawnRenderTree_ParallelPreDraw_StasisPod
    {
        // Symmetrical distance from center of 1x1 pod tile to the glass aperture
        public const float PodWindowOffset = 0.20f;

        [HarmonyPrefix]
        public static void Prefix(ref PawnDrawParms parms)
        {
            if (parms.Portrait || (parms.flags & PawnRenderFlags.Portrait) != 0 || (parms.flags & PawnRenderFlags.Cache) != 0) return;

            Pawn pawn = parms.pawn;
            if (pawn == null || !pawn.Spawned) return;

            Building_Bed bed = pawn.CurrentBed();
            if (bed is Building_ConstructStasisPod pod)
            {
                float scale = pawn.ageTracker?.CurLifeStage?.bodySizeFactor ?? 1f;
                float offset = (scale < 0.95f) ? 0.12f : PodWindowOffset;

                if (pod.Rotation == Rot4.North)
                {
                    parms.matrix.m23 -= offset;
                }
                else if (pod.Rotation == Rot4.South)
                {
                    parms.matrix.m23 += offset;
                }
                else if (pod.Rotation == Rot4.East)
                {
                    parms.matrix.m03 -= offset;
                }
                else if (pod.Rotation == Rot4.West)
                {
                    parms.matrix.m03 += offset;
                }
            }
        }
    }

    /// <summary>
    /// Normalizes head offset to Vector3.zero across all body types, body sizes, and life stages
    /// while inside a ConstructStasisPod on the map, ensuring every construct centers identically at (0,0)
    /// before applying the symmetrical aperture offset.
    /// Excludes portrait rendering.
    /// </summary>
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt))]
    public static class Patch_PawnRenderer_BaseHeadOffsetAt_StasisPod
    {
        [HarmonyPostfix]
        public static void Postfix(PawnRenderer __instance, ref Vector3 __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn == null || !pawn.Spawned) return;

            Building_Bed bed = pawn.CurrentBed();
            if (bed is Building_ConstructStasisPod)
            {
                __result = Vector3.zero;
            }
        }
    }
}

