using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    /// <summary>
    /// Provides the right-click "Rest until healed" float menu option on beds and ConstructStasisPods
    /// for construct colonists (drafted or undrafted).
    /// </summary>
    public class FloatMenuOptionProvider_ConstructRestUntilHealed : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            Pawn pawn = context?.FirstSelectedPawn;
            if (pawn == null || !pawn.Spawned || pawn.Downed)
            {
                return false;
            }

            return ConstructUtility.IsConstruct(pawn);
        }

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            if (!(clickedThing is Building_ConstructStasisPod pod))
            {
                return null;
            }

            Pawn pawn = context?.FirstSelectedPawn;
            if (pawn == null)
            {
                return null;
            }

            if (!pawn.CanReach(pod, PathEndMode.OnCell, Danger.Deadly))
            {
                return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            }

            if (pod.IsForbidden(pawn))
            {
                return new FloatMenuOption("CannotPrioritizeForbidden".Translate(pod.Label), null);
            }

            if (pod.CompAssignableToPawn != null)
            {
                if (!pod.CompAssignableToPawn.CanAssignTo(pawn))
                {
                    return new FloatMenuOption("CannotUseReason".Translate(pod.CompAssignableToPawn.CanAssignTo(pawn).Reason), null);
                }

                if (!pod.CompAssignableToPawn.HasFreeSlot && !RestUtility.BedOwnerWillShare(pod, pawn, pawn.guest?.GuestStatus) && !pod.IsOwner(pawn))
                {
                    return new FloatMenuOption("CannotUseReason".Translate("SomeoneElseSleeping".Translate(pod.OwnersForReading.Select(p => p.LabelShort).ToCommaList())), null);
                }
            }

            if (!pod.IsOwner(pawn) && pod.CompAssignableToPawn != null && !pod.CompAssignableToPawn.HasFreeSlot)
            {
                return new FloatMenuOption("CannotUseReason".Translate("SomeoneElseSleeping".Translate(pod.OwnersForReading.Select(p => p.LabelShort).ToCommaList())), null);
            }

            if (!RestUtility.IsValidBedFor(pod, pawn, pawn, checkSocialProperness: false))
            {
                return new FloatMenuOption("Inaccessible".Translate(), null);
            }

            bool isInjured = HealthAIUtility.ShouldSeekMedicalRest(pawn)
                || pawn.health.hediffSet.HasTendableHediff()
                || pawn.health.hediffSet.BleedRateTotal > 0.001f
                || pawn.health.hediffSet.hediffs.Any(h => h is Hediff_Injury injury && injury.CanHealNaturally());

            if (!isInjured)
            {
                return new FloatMenuOption("UseMedicalBed".Translate() + " (" + "NotInjured".Translate() + ")", null);
            }

            return new FloatMenuOption("UseMedicalBed".Translate(), delegate
            {
                if (pawn.Drafted)
                {
                    pawn.drafter.Drafted = false;
                }
                Job job = JobMaker.MakeJob(JobDefOf.LayDown, pod);
                job.restUntilHealed = true;
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
        }
    }
}
