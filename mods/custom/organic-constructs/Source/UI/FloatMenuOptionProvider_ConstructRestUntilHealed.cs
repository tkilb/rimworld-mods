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
            if (!(clickedThing is Building_Bed bed) || !bed.def.building.bed_humanlike)
            {
                return null;
            }

            Pawn pawn = context?.FirstSelectedPawn;
            if (pawn == null)
            {
                return null;
            }

            // If the bed is already a vanilla medical bed and pawn is undrafted, vanilla FloatMenuMakerMap
            // will natively provide the option via Patch_HealthAIUtility_ShouldSeekMedicalRest.
            if (bed.Medical && !pawn.Drafted)
            {
                return null;
            }

            if (!pawn.CanReach(bed, PathEndMode.OnCell, Danger.Deadly))
            {
                return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            }

            if (bed.IsForbidden(pawn))
            {
                return new FloatMenuOption("CannotPrioritizeForbidden".Translate(bed.Label), null);
            }

            if (bed.CompAssignableToPawn != null)
            {
                if (!bed.CompAssignableToPawn.CanAssignTo(pawn))
                {
                    return new FloatMenuOption("CannotUseReason".Translate(bed.CompAssignableToPawn.CanAssignTo(pawn).Reason), null);
                }

                if (!bed.CompAssignableToPawn.HasFreeSlot && !RestUtility.BedOwnerWillShare(bed, pawn, pawn.guest?.GuestStatus) && !bed.IsOwner(pawn))
                {
                    return new FloatMenuOption("CannotUseReason".Translate("SomeoneElseSleeping".Translate(bed.OwnersForReading.Select(p => p.LabelShort).ToCommaList())), null);
                }
            }

            if (!bed.Medical && !bed.IsOwner(pawn) && bed.CompAssignableToPawn != null && !bed.CompAssignableToPawn.HasFreeSlot)
            {
                return new FloatMenuOption("CannotUseReason".Translate("SomeoneElseSleeping".Translate(bed.OwnersForReading.Select(p => p.LabelShort).ToCommaList())), null);
            }

            if (!RestUtility.IsValidBedFor(bed, pawn, pawn, checkSocialProperness: false))
            {
                return new FloatMenuOption("Inaccessible".Translate(), null);
            }

            bool isInjured = HealthAIUtility.ShouldSeekMedicalRest(pawn)
                || pawn.health.hediffSet.HasTendableHediff()
                || pawn.health.hediffSet.BleedRateTotal > 0.001f
                || pawn.health.summaryHealth.SummaryHealthPercent < 0.999f
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
                Job job = JobMaker.MakeJob(JobDefOf.LayDown, bed);
                job.restUntilHealed = true;
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
        }
    }
}
