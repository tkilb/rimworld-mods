using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GeneSplicer
{
    public class CompEmbryoQuality : ThingComp
    {
        public CompProperties_EmbryoQuality Props => (CompProperties_EmbryoQuality)props;

        public int editCount = 0;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref editCount, "editCount", 0);
        }

        public override string CompInspectStringExtra()
        {
            return $"Genetic Edits: {editCount}/2";
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;

            // If the embryo is on the ground/storage and not inside a bench, offer quick carry action
            if (parent.Spawned && parent.Map != null)
            {
                Building_EmbryoSplicingBench bench = FindAvailableBench();
                if (bench != null && editCount < 2)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Carry to Splicing Bench",
                        defaultDesc = "Order a colonist to carry this embryo to an available Embryo Splicing Bench.",
                        icon = bench.def.uiIcon,
                        action = () =>
                        {
                            bench.targetEmbryo = parent;
                            Messages.Message("Designated embryo to be carried to Embryo Splicing Bench.", bench, MessageTypeDefOf.NeutralEvent);
                        }
                    };
                }
            }
        }

        private Building_EmbryoSplicingBench FindAvailableBench()
        {
            if (parent?.MapHeld == null) return null;

            return (Building_EmbryoSplicingBench)GenClosest.ClosestThingReachable(
                parent.PositionHeld,
                parent.MapHeld,
                ThingRequest.ForDef(GeneSplicerDefOf.EmbryoSplicingBench),
                PathEndMode.InteractionCell,
                TraverseParms.For(TraverseMode.NoPassClosedDoors),
                validator: b => b is Building_EmbryoSplicingBench bench && bench.CanAcceptEmbryo);
        }
    }
}
