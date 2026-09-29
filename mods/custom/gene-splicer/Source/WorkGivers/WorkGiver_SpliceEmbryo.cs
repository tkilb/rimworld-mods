using RimWorld;
using Verse;
using Verse.AI;

namespace GeneSplicer
{
    public class WorkGiver_SpliceEmbryo : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(GeneSplicerDefOf.EmbryoSplicingBench);

        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Some;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_EmbryoSplicingBench bench)) return false;
            if (!bench.HasPendingSpliceOrder) return false;
            if (t.IsForbidden(pawn)) return false;

            if (!pawn.CanReserve(bench, 1, -1, null, forced)) return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_EmbryoSplicingBench bench)) return null;
            if (!bench.HasPendingSpliceOrder) return null;

            return JobMaker.MakeJob(GeneSplicerDefOf.SpliceEmbryo, bench);
        }
    }
}
