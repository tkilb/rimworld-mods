using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GeneSplicer
{
    public class JobDriver_SpliceEmbryo : JobDriver
    {
        private const TargetIndex BenchInd = TargetIndex.A;
        private const int DurationTicks = 1500;

        protected Building_EmbryoSplicingBench Bench => job.GetTarget(BenchInd).Thing as Building_EmbryoSplicingBench;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Bench, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(BenchInd);
            this.FailOn(() => Bench == null || !Bench.HasEmbryo || !Bench.HasPendingSpliceOrder);

            yield return Toils_Goto.GotoThing(BenchInd, PathEndMode.InteractionCell);

            Toil workToil = ToilMaker.MakeToil("PerformSplicing");
            workToil.initAction = () =>
            {
                pawn.pather.StopDead();
            };
            workToil.tickAction = () =>
            {
                pawn.rotationTracker.FaceCell(Bench.Position);
                pawn.skills?.Learn(SkillDefOf.Medicine, 0.05f);
            };
            workToil.defaultCompleteMode = ToilCompleteMode.Delay;
            workToil.defaultDuration = DurationTicks;
            workToil.WithProgressBarToilDelay(BenchInd);
            workToil.activeSkill = () => SkillDefOf.Medicine;

            workToil.AddFinishAction(() =>
            {
                if (workToil.actor.jobs.curDriver.ticksLeftThisToil <= 0)
                {
                    Bench.CompleteSpliceOperation(pawn);
                }
            });

            yield return workToil;
        }
    }
}
