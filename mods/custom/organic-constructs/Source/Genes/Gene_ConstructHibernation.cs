using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    public class Gene_ConstructHibernation : Gene
    {
        public int operatingTicks = 0;
        public int stasisTicks = 0;
        public bool inStasis = false;
        public bool maintenanceWarningFired = false;

        public const int MaxOperatingTicks = 1800000; // 30 days
        public const int WarningTicksRemaining = 75000;
        public const int MinStasisTicks = 30000; // 12 hours (1 hour = 2500 ticks)

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref operatingTicks, "operatingTicks", 0);
            Scribe_Values.Look(ref stasisTicks, "stasisTicks", 0);
            Scribe_Values.Look(ref inStasis, "inStasis", false);
            Scribe_Values.Look(ref maintenanceWarningFired, "maintenanceWarningFired", false);
        }

        public override void Tick()
        {
            base.Tick();

            if (!inStasis)
            {
                operatingTicks++;
                if (operatingTicks >= MaxOperatingTicks - WarningTicksRemaining && !maintenanceWarningFired)
                {
                    maintenanceWarningFired = true;
                    Find.LetterStack.ReceiveLetter("Construct Maintenance Required",
                        $"{pawn.NameShortColored} is approaching the 30-day operating limit. They must enter stasis soon or they will trigger an emergency shutdown.",
                        LetterDefOf.NegativeEvent, pawn);
                }

                if (operatingTicks >= MaxOperatingTicks && pawn.Spawned && !pawn.Downed)
                {
                    // Emergency shutdown (comatose)
                    pawn.health.AddHediff(ConstructAugmentDefOf.Construct_Assimilation);
                    Messages.Message($"{pawn.NameShortColored} has reached maximum operating age and entered emergency shutdown.", pawn, MessageTypeDefOf.NegativeEvent);
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (pawn.Faction == Faction.OfPlayer)
            {
                Command_Action enterStasis = new Command_Action
                {
                    defaultLabel = "Enter Stasis",
                    defaultDesc = "Direct this construct to enter hibernation stasis (requires a bed or ground). Must remain in stasis for at least 12 hours for a clean wake.",
                    icon = def?.Icon ?? ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Sleeping", false),
                    action = delegate
                    {
                        Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("Construct_EnterConstructStasis"), pawn);
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }
                };
                yield return enterStasis;
            }
        }
    }

    public class JobDriver_ConstructStasis : JobDriver
    {
        private Gene_ConstructHibernation HibernationGene => pawn.genes?.GetGene(ConstructAugmentDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoBedOrSpot = new Toil();
            gotoBedOrSpot.initAction = delegate
            {
                Building_Bed bed = RestUtility.FindBedFor(pawn);
                if (bed != null)
                {
                    pawn.pather.StartPath(bed, PathEndMode.OnCell);
                }
                else
                {
                    pawn.pather.StartPath(pawn.Position, PathEndMode.OnCell);
                }
            };
            gotoBedOrSpot.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            yield return gotoBedOrSpot;

            Toil stasisToil = new Toil();
            stasisToil.initAction = delegate
            {
                var gene = HibernationGene;
                if (gene != null)
                {
                    gene.inStasis = true;
                    gene.stasisTicks = 0;
                }
                asleep = true;
            };
            stasisToil.tickAction = delegate
            {
                var gene = HibernationGene;
                if (gene != null)
                {
                    gene.stasisTicks++;
                    if (gene.stasisTicks >= Gene_ConstructHibernation.MinStasisTicks)
                    {
                        ReadyForNextToil();
                    }
                }
            };
            stasisToil.AddFinishAction(delegate
            {
                var gene = HibernationGene;
                if (gene != null)
                {
                    gene.inStasis = false;

                    if (gene.stasisTicks < Gene_ConstructHibernation.MinStasisTicks)
                    {
                        // Interrupted Stasis
                        pawn.health.AddHediff(ConstructAugmentDefOf.Construct_InterruptedStasis);
                        Messages.Message($"{pawn.NameShortColored} was interrupted from stasis early and suffers from neural defragmentation failure.", pawn, MessageTypeDefOf.NegativeEvent);
                    }
                    else
                    {
                        // Clean wake
                        gene.operatingTicks = 0;
                        gene.maintenanceWarningFired = false;
                        if (pawn.needs != null && pawn.needs.rest != null)
                        {
                            pawn.needs.rest.CurLevel = 1f;
                        }

                        // Remove coma if they were in emergency shutdown
                        Hediff coma = pawn.health.hediffSet.GetFirstHediffOfDef(ConstructAugmentDefOf.Construct_Assimilation);
                        if (coma != null)
                        {
                            pawn.health.RemoveHediff(coma);
                        }

                        Messages.Message($"{pawn.NameShortColored} completed a full stasis cycle and is fully refreshed.", pawn, MessageTypeDefOf.PositiveEvent);
                    }
                }
            });
            stasisToil.defaultCompleteMode = ToilCompleteMode.Never;
            stasisToil.FailOnDespawnedOrNull(TargetIndex.A);

            yield return stasisToil;
        }

        public override string GetReport()
        {
            var gene = HibernationGene;
            if (gene != null && gene.inStasis)
            {
                int remaining = Mathf.Max(0, Gene_ConstructHibernation.MinStasisTicks - gene.stasisTicks);
                return $"in stasis ({(remaining / 2500f):F1}h remaining).";
            }
            return "in stasis.";
        }
    }
}
