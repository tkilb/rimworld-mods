using RimWorld;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class WorkGiver_RecycleEmbryo : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(ThingDefOf.HumanEmbryo);

        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is not Thing embryo) return false;

            CompEmbryoQuality comp = embryo.TryGetComp<CompEmbryoQuality>();
            if (comp == null) return false;

            if (!comp.designatedForRecycling) return false;
            if (embryo.IsForbidden(pawn)) return false;
            if (!pawn.CanReserve(embryo)) return false;

            Building workstation = FindWorkstation(pawn);
            if (workstation == null)
            {
                JobFailReason.Is("No reachable Construct Synthesizer or Gene Assembler found.");
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building workstation = FindWorkstation(pawn);
            if (workstation == null) return null;

            Job job = JobMaker.MakeJob(ConstructDefOf.Construct_RecycleEmbryo, workstation, t);
            job.count = 1;
            return job;
        }

        private static Building FindWorkstation(Pawn pawn)
        {
            return (Building)GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                PathEndMode.InteractionCell,
                TraverseParms.For(pawn),
                validator: b => (b is Building_ConstructSynthesizer || b is Building_GeneAssembler) && !b.IsBurning() && pawn.CanReserve(b));
        }
    }
}
