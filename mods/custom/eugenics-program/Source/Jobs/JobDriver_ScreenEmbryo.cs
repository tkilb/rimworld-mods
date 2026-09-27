using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    /// <summary>
    /// Job driver for the "Screen Embryo Genetics" operation.
    ///
    /// A colonist with Medical skill >= minScreeningSkill (default 8, XML-tunable)
    /// examines a HumanEmbryo at a Gene Assembler to reveal any hidden defects
    /// tracked by CompEmbryoQuality.
    ///
    /// On success:
    ///   - Sets CompEmbryoQuality.isScreened = true.
    ///   - If defects exist, reveals them via a message and gizmo report.
    ///   - If the room is dirty, there is an extra chance of introducing NEW defects.
    ///
    /// Job targets:
    ///   TargetIndex.A = Building_GeneAssembler
    ///   TargetIndex.B = HumanEmbryo (Thing)
    /// </summary>
    public class JobDriver_ScreenEmbryo : JobDriver
    {
        // Configurable via XML in JobDef (screeningDurationTicks default via JobProperties_ScreenEmbryo)
        private const int DefaultDurationTicks = 2000;

        private Building Assembler => (Building)job.GetTarget(TargetIndex.A).Thing;
        private Thing    Embryo    => job.GetTarget(TargetIndex.B).Thing;

        private CompEmbryoQuality EmbryoQuality =>
            Embryo?.TryGetComp<CompEmbryoQuality>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed)
                && pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Fail conditions
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnDespawnedOrNull(TargetIndex.B);
            this.FailOn(() => EmbryoQuality == null);
            this.FailOn(() => EmbryoQuality.isScreened);

            // 1. Walk to assembler
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            // 2. Haul embryo to assembler if not already adjacent
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: false);
            yield return Toils_Haul.CarryHauledThingToContainer();

            // 3. Diagnostic screening work
            Toil screenToil = ToilMaker.MakeToil();
            screenToil.initAction = () =>
            {
                pawn.pather.StopDead();
            };
            screenToil.tickAction = () =>
            {
                pawn.skills?.Learn(SkillDefOf.Medicine, 0.11f);
                PawnUtility.GainComfortFromCellIfPossible(pawn, 1);
            };
            screenToil.defaultDuration       = GetScreeningDuration();
            screenToil.defaultCompleteMode   = ToilCompleteMode.Delay;
            screenToil.WithProgressBarToilDelay(TargetIndex.A);
            screenToil.activeSkill           = () => SkillDefOf.Medicine;
            screenToil.handlingFacing        = true;
            yield return screenToil;

            // 4. Resolve screening
            Toil resolveToil = ToilMaker.MakeToil();
            resolveToil.initAction = () =>
            {
                CompEmbryoQuality comp = EmbryoQuality;
                if (comp == null) return;

                CompProperties_EmbryoQuality props = comp.Props;

                // Dirty room complication check
                float roomCleanlinessScore = pawn.GetRoom()?.GetStat(RoomStatDefOf.Cleanliness) ?? 1f;
                bool dirtyRoom = roomCleanlinessScore < 0f;

                if (dirtyRoom && !comp.hasDefects)
                {
                    float contamChance = (props?.dirtyRoomDefectMultiplier ?? 2f) * 0.03f;
                    if (Rand.Chance(contamChance))
                    {
                        comp.IntroduceComplication("Screening contamination (dirty lab environment)", 0.35f);
                    }
                }

                // Reveal results
                comp.PerformScreening(pawn);

                // Drop embryo back near assembler
                Thing embryoThing = Embryo;
                if (pawn.carryTracker?.CarriedThing == embryoThing)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
                }
            };
            resolveToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return resolveToil;
        }

        private int GetScreeningDuration()
        {
            // Allow jobs to carry a custom duration via driver fields if needed.
            return DefaultDurationTicks;
        }
    }
}
