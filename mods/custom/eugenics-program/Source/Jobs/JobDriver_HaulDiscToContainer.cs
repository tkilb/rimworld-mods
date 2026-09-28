using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    public class JobDriver_HaulDiscToContainer : JobDriver
    {
        private const TargetIndex DiscInd = TargetIndex.A;
        private const TargetIndex DestInd = TargetIndex.B;

        public Thing Disc => job.GetTarget(DiscInd).Thing;
        public Thing Dest => job.GetTarget(DestInd).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(DiscInd), job, 1, 1, null, errorOnFailed)
                && pawn.Reserve(job.GetTarget(DestInd), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(DiscInd);
            this.FailOnDestroyedNullOrForbidden(DestInd);
            this.FailOn(() => !CanAcceptDisc(Dest, Disc));

            yield return Toils_Goto.GotoThing(DiscInd, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(DiscInd)
                .FailOnSomeonePhysicallyInteracting(DiscInd);

            Toil carryToil = Toils_Haul.StartCarryThing(DiscInd, putRemainderInQueue: false, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: true);
            carryToil.AddFinishAction(() =>
            {
                Thing carried = pawn.carryTracker?.CarriedThing;
                if (carried != null)
                {
                    UpdateTargetDisc(Dest, carried);
                }
            });
            yield return carryToil;

            yield return Toils_Goto.GotoThing(DestInd, PathEndMode.Touch);

            Toil deposit = ToilMaker.MakeToil("DepositDisc");
            deposit.initAction = () =>
            {
                Thing carried = pawn.carryTracker?.CarriedThing;
                if (carried == null) return;
                TryDepositDisc(Dest, carried);
            };
            deposit.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return deposit;
        }

        public static void UpdateTargetDisc(Thing dest, Thing carried)
        {
            if (dest == null || carried == null) return;

            if (dest is Building_NeuralScanner scanner)
            {
                scanner.targetDisc = carried;
            }
            else if (dest is Building_EmbryoSplicingBench bench)
            {
                bench.targetDisc = carried;
            }
            else if (dest is ThingWithComps twc)
            {
                CompGrowthVatImprinter imprinter = twc.GetComp<CompGrowthVatImprinter>();
                if (imprinter != null) imprinter.targetDisc = carried;
            }
        }

        public static bool CanAcceptDisc(Thing dest, Thing disc)
        {
            if (dest == null || disc == null) return false;

            if (dest is Building_NeuralScanner scanner)
            {
                return scanner.CanAcceptDisc &&
                    (scanner.targetDisc == null || scanner.targetDisc == disc || disc.holdingOwner?.Owner is Pawn_CarryTracker);
            }
            if (dest is Building_EmbryoSplicingBench bench)
            {
                return bench.CanAcceptDisc &&
                    (bench.targetDisc == null || bench.targetDisc == disc || disc.holdingOwner?.Owner is Pawn_CarryTracker);
            }
            if (dest is ThingWithComps twc)
            {
                CompGrowthVatImprinter imprinter = twc.GetComp<CompGrowthVatImprinter>();
                if (imprinter != null)
                {
                    return imprinter.CanAcceptDisc &&
                        (imprinter.targetDisc == null || imprinter.targetDisc == disc || disc.holdingOwner?.Owner is Pawn_CarryTracker);
                }
            }
            return false;
        }

        public static bool TryDepositDisc(Thing dest, Thing disc)
        {
            if (dest == null || disc == null) return false;

            if (dest is Building_NeuralScanner scanner)
            {
                return scanner.TryAcceptDisc(disc);
            }
            if (dest is Building_EmbryoSplicingBench bench)
            {
                return bench.TryAcceptDisc(disc);
            }
            if (dest is ThingWithComps twc)
            {
                CompGrowthVatImprinter imprinter = twc.GetComp<CompGrowthVatImprinter>();
                if (imprinter != null)
                {
                    return imprinter.TryLoadDisc(disc);
                }
            }
            return false;
        }
    }
}
