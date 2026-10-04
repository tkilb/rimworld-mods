using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    /// <summary>
    /// Fallback option: Allows the player to order a colonist to carry a construct who is in
    /// stasis or emergency shutdown to their assigned bed if they collapsed on the floor or are in the wrong bed.
    /// </summary>
    public class FloatMenuOptionProvider_CarryConstructToBed : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;
        protected override bool RequiresManipulation => true;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            Pawn pawn = context?.FirstSelectedPawn;
            return pawn != null && pawn.Spawned && !pawn.Downed && pawn.Faction == Faction.OfPlayer;
        }

        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            Pawn rescuer = context?.FirstSelectedPawn;
            if (rescuer == null || clickedPawn == null || !clickedPawn.Spawned || clickedPawn == rescuer)
            {
                return null;
            }

            var gene = clickedPawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
            if (gene == null)
            {
                return null;
            }

            // Applies if the construct is in stasis, in emergency shutdown, or has the InStasis hediff
            bool inStasisOrShutdown = gene.inStasis 
                || clickedPawn.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis)
                || gene.operatingTicks >= Gene_ConstructHibernation.MaxOperatingTicks;

            if (!inStasisOrShutdown)
            {
                return null;
            }

            // If already resting in their assigned non-medical bed, no need to carry
            Building_Bed currentBed = clickedPawn.CurrentBed();
            Building_Bed ownedBed = clickedPawn.ownership?.OwnedBed;
            if (currentBed != null && currentBed == ownedBed && !currentBed.Medical)
            {
                return null;
            }

            if (!rescuer.CanReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly))
            {
                return new FloatMenuOption("Cannot carry " + clickedPawn.LabelShort + " to bed: " + "CannotUseNoPath".Translate(), null);
            }

            // Prioritize their assigned bed, falling back to any valid non-medical bed
            Building_Bed targetBed = null;
            if (ownedBed != null && !ownedBed.Medical && RestUtility.IsValidBedFor(ownedBed, clickedPawn, rescuer, checkSocialProperness: false) && rescuer.CanReach(ownedBed, PathEndMode.OnCell, Danger.Deadly))
            {
                targetBed = ownedBed;
            }
            else
            {
                if (clickedPawn.Map != null)
                {
                    targetBed = clickedPawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>()
                        .FirstOrDefault(b => !b.Medical && b.def.building.bed_humanlike && RestUtility.IsValidBedFor(b, clickedPawn, rescuer, checkSocialProperness: false) && rescuer.CanReach(b, PathEndMode.OnCell, Danger.Deadly));
                }

                if (targetBed == null)
                {
                    targetBed = RestUtility.FindBedFor(clickedPawn, rescuer, checkSocialProperness: false);
                }
            }

            if (targetBed == null)
            {
                return new FloatMenuOption("Cannot carry " + clickedPawn.LabelShort + " to bed: " + "NoBed".Translate(), null);
            }

            return new FloatMenuOption("Carry " + clickedPawn.LabelShort + " to bed (" + targetBed.LabelShort + ")", delegate
            {
                Job job = JobMaker.MakeJob(JobDefOf.Rescue, clickedPawn, targetBed);
                job.count = 1;
                rescuer.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
        }
    }
}
