using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace OrganicConstructs
{
    public class CompEmbryoQuality : ThingComp
    {
        public CompProperties_EmbryoQuality Props => (CompProperties_EmbryoQuality)props;

        public int editCount = 0;
        public bool isConstruct = false;
        public bool designatedForRecycling = false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref editCount, "editCount", 0);
            Scribe_Values.Look(ref isConstruct, "isConstruct", false);
            Scribe_Values.Look(ref designatedForRecycling, "designatedForRecycling", false);
        }

        public override string TransformLabel(string label)
        {
            if (isConstruct)
            {
                return $"{label} (construct matrix)";
            }
            return base.TransformLabel(label);
        }

        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();

            if (isConstruct)
            {
                sb.AppendLine("Origin: Synthetic Construct Matrix");
            }

            if (editCount > 0)
            {
                sb.AppendLine($"Matrix Imprints: {editCount}");
            }

            if (designatedForRecycling)
            {
                sb.Append("[Designated for Biomass Recycling]");
            }

            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;

            // Manual recycle designation toggle
            yield return new Command_Toggle
            {
                defaultLabel = designatedForRecycling ? "Cancel Recycle Order" : "Designate for Recycling",
                defaultDesc = "Toggle manual designation for biomass liquefaction into Genetic Nutrient Paste.",
                icon = parent?.def?.uiIcon,
                isActive = () => designatedForRecycling,
                toggleAction = () => designatedForRecycling = !designatedForRecycling
            };
        }
    }
}
