using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class JobDriver_ScanNeuralProfile : JobDriver
    {
        public Building_NeuralScanner Scanner => (Building_NeuralScanner)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (Building_NeuralScanner.IsConstruct(pawn))
            {
                JobFailReason.Is("Constructs cannot be scanned: Synthetic neural architecture incompatible with scanner");
                return false;
            }
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => Scanner == null || !Scanner.CanAcceptPawn || Building_NeuralScanner.IsConstruct(pawn));

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil enterToil = new Toil();
            enterToil.initAction = () =>
            {
                if (Building_NeuralScanner.IsConstruct(pawn))
                {
                    Messages.Message("Constructs cannot be scanned: Synthetic neural architecture incompatible with scanner.", pawn, MessageTypeDefOf.RejectInput, false);
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                Building_NeuralScanner scanner = Scanner;
                if (scanner != null && scanner.CanAcceptPawn)
                {
                    scanner.TryAcceptPawn(pawn);
                }
            };
            enterToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return enterToil;
        }
    }
}
