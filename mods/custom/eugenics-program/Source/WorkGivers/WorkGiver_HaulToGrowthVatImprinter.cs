using RimWorld;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    public class WorkGiver_HaulToGrowthVatImprinter : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(ThingDefOf.GrowthVat);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return pawn.WorkTypeIsDisabled(WorkTypeDefOf.Hauling);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is not ThingWithComps twc) return false;
            CompGrowthVatImprinter imprinter = twc.GetComp<CompGrowthVatImprinter>();
            if (imprinter == null || imprinter.LoadedDisc != null || imprinter.targetDisc == null) return false;
            Thing disc = imprinter.targetDisc;
            if (!disc.Spawned || disc.Destroyed || disc.IsForbidden(pawn)) return false;
            if (!pawn.CanReserve(twc, 1, -1, null, forced)) return false;
            if (!pawn.CanReserve(disc, 1, 1, null, forced)) return false;
            if (!pawn.CanReach(disc, PathEndMode.ClosestTouch, Danger.Some)) return false;
            if (!pawn.CanReach(twc, PathEndMode.Touch, Danger.Some)) return false;
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is not ThingWithComps twc) return null;
            CompGrowthVatImprinter imprinter = twc.GetComp<CompGrowthVatImprinter>();
            if (imprinter == null) return null;
            Thing disc = imprinter.targetDisc;
            if (disc == null || !disc.Spawned) return null;
            Job job = JobMaker.MakeJob(EugenicsDefOf.Eugenics_HaulDiscToContainer, disc, twc);
            job.count = 1;
            return job;
        }
    }
}
