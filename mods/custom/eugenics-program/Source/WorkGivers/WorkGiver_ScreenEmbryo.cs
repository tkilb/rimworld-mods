using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    /// <summary>
    /// WorkGiver that assigns "Screen Embryo Genetics" jobs.
    ///
    /// Eligibility:
    ///   - Pawn must have Medical skill >= CompProperties_EmbryoQuality.minScreeningSkill (default 8).
    ///   - Target embryo must have CompEmbryoQuality and NOT yet be screened.
    ///   - A Gene Assembler must be available and reachable.
    /// </summary>
    public class WorkGiver_ScreenEmbryo : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(ThingDefOf.HumanEmbryo);

        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            // Quick-out: pawn must be capable of Medical work
            return pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is not Thing embryo) return false;

            CompEmbryoQuality comp = embryo.TryGetComp<CompEmbryoQuality>();
            if (comp == null || comp.isScreened) return false;

            // Skill gate: enforced here so it shows a reason tooltip in the work tab
            int minSkill = comp.Props?.minScreeningSkill ?? 8;
            if (pawn.skills?.GetSkill(SkillDefOf.Medicine)?.Level < minSkill)
            {
                JobFailReason.Is($"Requires Medical {minSkill}+ to screen embryo.");
                return false;
            }

            // Find a reachable Gene Assembler
            Building assembler = FindAssembler(pawn);
            if (assembler == null)
            {
                JobFailReason.Is("No reachable Gene Assembler found.");
                return false;
            }

            return !embryo.IsForbidden(pawn) && pawn.CanReserve(embryo);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building assembler = FindAssembler(pawn);
            if (assembler == null) return null;

            Job job = JobMaker.MakeJob(EugenicsDefOf.Eugenics_ScreenEmbryo, assembler, t);
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
