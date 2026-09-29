using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class JobDriver_RecycleEmbryo : JobDriver
    {
        private const int DefaultLiquefactionTicks = 1200;
        private const int BaseNutrientYield = 1;

        private Building Workstation => (Building)job.GetTarget(TargetIndex.A).Thing;
        private Thing Embryo => job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed)
                && pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnDespawnedOrNull(TargetIndex.B);

            // 1. Walk to embryo
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);

            // 2. Pick up embryo
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: false);

            // 3. Walk to workstation
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            // 4. Liquefaction work
            Toil workToil = ToilMaker.MakeToil();
            workToil.tickAction = () =>
            {
                pawn.skills?.Learn(SkillDefOf.Medicine, 0.08f);
            };
            workToil.defaultDuration = DefaultLiquefactionTicks;
            workToil.defaultCompleteMode = ToilCompleteMode.Delay;
            workToil.WithProgressBarToilDelay(TargetIndex.A);
            workToil.activeSkill = () => SkillDefOf.Medicine;
            yield return workToil;

            // 4. Spawn output and destroy embryo
            Toil resolveToil = ToilMaker.MakeToil();
            resolveToil.initAction = () =>
            {
                Thing embryoThing = Embryo;
                if (embryoThing == null) return;

                CompEmbryoQuality comp = embryoThing.TryGetComp<CompEmbryoQuality>();
                int yield = BaseNutrientYield;

                // Drop carried embryo to destroy it
                if (pawn.carryTracker?.CarriedThing == embryoThing)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Thing dropped);
                    embryoThing = dropped ?? embryoThing;
                }

                // Spawn GeneticNutrientPaste
                Thing paste = ThingMaker.MakeThing(ConstructDefOf.GeneticNutrientPaste);
                paste.stackCount = yield;
                GenPlace.TryPlaceThing(paste, pawn.Position, pawn.Map, ThingPlaceMode.Near);

                // Destroy embryo
                if (!embryoThing.Destroyed)
                    embryoThing.Destroy(DestroyMode.Vanish);

                Messages.Message(
                    $"Embryo biomass liquefied: {yield}x Genetic Nutrient Paste recovered.",
                    new LookTargets(pawn),
                    MessageTypeDefOf.NeutralEvent);
            };
            resolveToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return resolveToil;
        }
    }
}
