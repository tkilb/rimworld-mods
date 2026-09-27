using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    /// <summary>
    /// WorkGiver for "Liquefy Embryo Biomass".
    ///
    /// Eligibility:
    ///   - Target embryo has CompEmbryoQuality AND (isScreened AND hasDefects) OR
    ///     the player has manually designated it for recycling via gizmo.
    ///   - A Gene Assembler must be reachable.
    ///
    /// Note: Auto-recycling only triggers for DEFECTIVE embryos that have been screened.
    /// Non-screened or pristine embryos must be manually designated.
    /// </summary>
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

            // Auto-recycle only defective screened embryos; pristine ones require manual designation
            bool autoEligible = comp.isScreened && comp.hasDefects;
            bool manualDesignated = comp.designatedForRecycling;

            if (!autoEligible && !manualDesignated) return false;
            if (embryo.IsForbidden(pawn)) return false;
            if (!pawn.CanReserve(embryo)) return false;

            Building assembler = FindAssembler(pawn);
            if (assembler == null)
            {
                JobFailReason.Is("No reachable Gene Assembler found.");
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building assembler = FindAssembler(pawn);
            if (assembler == null) return null;

            Job job = JobMaker.MakeJob(EugenicsDefOf.Eugenics_RecycleEmbryo, assembler, t);
            job.count = 1;
            return job;
        }

        private static Building FindAssembler(Pawn pawn)
        {
            return (Building)GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForDef(ThingDefOf.GeneAssembler),
                PathEndMode.InteractionCell,
                TraverseParms.For(pawn),
                validator: b => b is Building_GeneAssembler && !b.IsBurning());
        }
    }
}
