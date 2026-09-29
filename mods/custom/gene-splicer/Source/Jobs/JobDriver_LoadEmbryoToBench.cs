using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GeneSplicer
{
    public class JobDriver_LoadEmbryoToBench : JobDriver
    {
        private const TargetIndex EmbryoInd = TargetIndex.A;
        private const TargetIndex BenchInd = TargetIndex.B;

        protected Thing Embryo => job.GetTarget(EmbryoInd).Thing;
        protected Building_EmbryoSplicingBench Bench => job.GetTarget(BenchInd).Thing as Building_EmbryoSplicingBench;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(Embryo, job, 1, 1, null, errorOnFailed))
                return false;

            if (!pawn.Reserve(Bench, job, 1, -1, null, errorOnFailed))
                return false;

            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(EmbryoInd);
            this.FailOnDestroyedOrNull(BenchInd);
            this.FailOn(() => Bench == null || !Bench.CanAcceptEmbryo);

            yield return Toils_Goto.GotoThing(EmbryoInd, PathEndMode.Touch);

            yield return Toils_Haul.StartCarryThing(EmbryoInd, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);

            yield return Toils_Goto.GotoThing(BenchInd, PathEndMode.InteractionCell);

            Toil insertToil = ToilMaker.MakeToil("InsertEmbryo");
            insertToil.initAction = () =>
            {
                if (pawn.carryTracker.CarriedThing != null)
                {
                    Thing carried = pawn.carryTracker.CarriedThing;
                    if (Bench.TryAcceptEmbryo(carried))
                    {
                        pawn.carryTracker.innerContainer.Remove(carried);
                    }
                }
            };
            insertToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return insertToil;
        }
    }
}
