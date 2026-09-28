using RimWorld;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    public class WorkGiver_HaulToEmbryoSplicingBench : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(EugenicsDefOf.EmbryoSplicingBench);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return pawn.WorkTypeIsDisabled(WorkTypeDefOf.Hauling);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is not Building_EmbryoSplicingBench bench) return false;
            if (bench.LoadedDisc != null || bench.targetDisc == null) return false;
            Thing disc = bench.targetDisc;
            if (!disc.Spawned || disc.Destroyed || disc.IsForbidden(pawn)) return false;
            if (!pawn.CanReserve(bench, 1, -1, null, forced)) return false;
            if (!pawn.CanReserve(disc, 1, 1, null, forced)) return false;
            if (!pawn.CanReach(disc, PathEndMode.ClosestTouch, Danger.Some)) return false;
            if (!pawn.CanReach(bench, PathEndMode.Touch, Danger.Some)) return false;
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is not Building_EmbryoSplicingBench bench) return null;
            Thing disc = bench.targetDisc;
            if (disc == null || !disc.Spawned) return null;
            Job job = JobMaker.MakeJob(EugenicsDefOf.Eugenics_HaulDiscToContainer, disc, bench);
            job.count = 1;
            return job;
        }
    }
}
