using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OrganicConstructs
{
    /// <summary>
    /// A 1x1 vertical stasis pod designed exclusively for biological constructs.
    /// Provides accelerated neural purging (36h vs 48h) when powered and buffers early wake shock.
    /// </summary>
    public class Building_ConstructStasisPod : Building_Bed
    {
        private CompPowerTrader powerComp;

        public bool IsPowered => (powerComp == null || powerComp.PowerOn);

        public Pawn PrimaryOccupant
        {
            get
            {
                if (!Spawned || Map == null) return null;
                for (int i = 0; i < SleepingSlotsCount; i++)
                {
                    Pawn occupant = GetCurOccupant(i);
                    if (occupant != null)
                        return occupant;
                }
                return null;
            }
        }

        public bool IsOccupied => PrimaryOccupant != null;

        public bool IsInActiveStasis
        {
            get
            {
                Pawn occupant = PrimaryOccupant;
                if (occupant == null) return false;

                var gene = occupant.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                return (gene != null && gene.inStasis)
                    || occupant.health.hediffSet.HasHediff(ConstructDefOf.Construct_InStasis)
                    || occupant.CurJobDef == ConstructDefOf.Construct_EnterConstructStasis;
            }
        }

        public const float PowerConsumptionActiveStasis = 100f;
        public const float PowerConsumptionStandby = 10f;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            UpdatePowerState();
        }

        protected override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(30))
            {
                Pawn occupant = PrimaryOccupant;
                if (occupant != null && !ConstructUtility.IsConstruct(occupant))
                {
                    occupant.jobs?.EndCurrentJob(JobCondition.Incompletable);
                }
                UpdatePowerState();
            }
        }

        public void UpdatePowerState()
        {
            if (!Spawned) return;

            bool inActiveStasis = IsInActiveStasis;

            if (powerComp != null)
            {
                float targetPowerOutput = inActiveStasis ? -PowerConsumptionActiveStasis : -PowerConsumptionStandby;
                if (powerComp.PowerOutput != targetPowerOutput)
                {
                    powerComp.PowerOutput = targetPowerOutput;
                }
            }
        }

        public override string GetInspectString()
        {
            string s = base.GetInspectString();
            if (IsInActiveStasis)
            {
                s += "\n" + "Status: Active construct stasis cycle (Synaptic defragmentation in progress)";
                s += "\n" + (IsPowered 
                    ? "Stasis purge rate: 133% (Powered: 36h cycle, buffered shock dampening)" 
                    : "Stasis purge rate: 100% (Unpowered: 48h cycle, standard shock)");
            }
            else if (IsOccupied)
            {
                Pawn occupant = PrimaryOccupant;
                if (occupant?.CurJob != null && occupant.CurJob.restUntilHealed)
                {
                    s += "\n" + "Status: Medical rest (Bio-monitoring & accelerated recovery active)";
                }
                else
                {
                    s += "\n" + "Status: Routine sleep (Standby: 10 W power drain)";
                }
            }
            else
            {
                s += "\n" + (IsPowered 
                    ? "Standby: 10 W power drain (Consumes 100 W during active stasis)" 
                    : "Unpowered: Connect to power grid to enable accelerated 36h stasis cycles");
            }
            return s;
        }
    }
}
