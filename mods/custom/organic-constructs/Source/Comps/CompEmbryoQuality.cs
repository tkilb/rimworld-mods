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
        public string blueprintLabel = null;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref editCount, "editCount", 0);
            Scribe_Values.Look(ref isConstruct, "isConstruct", false);
            Scribe_Values.Look(ref designatedForRecycling, "designatedForRecycling", false);
            Scribe_Values.Look(ref blueprintLabel, "blueprintLabel");
        }

        public override string TransformLabel(string label)
        {
            if (isConstruct)
            {
                if (!string.IsNullOrEmpty(blueprintLabel))
                {
                    return $"{label} (construct: {blueprintLabel})";
                }
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
                if (!string.IsNullOrEmpty(blueprintLabel))
                {
                    sb.AppendLine($"Blueprint Caste: {blueprintLabel}");
                }
            }

            if (editCount > 0)
            {
                sb.AppendLine($"Matrix Imprints: {editCount} / 2");
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

            if (parent is GeneSetHolderBase holder && holder.GeneSet != null && holder.GeneSet.GenesListForReading.Count > 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "View Genes",
                    defaultDesc = "View the complete genetic composition of this construct embryo matrix.",
                    icon = GeneSetHolderBase.GeneticInfoTex.Texture ?? parent?.def?.uiIcon,
                    action = () =>
                    {
                        StringBuilder sb = new StringBuilder();
                        string caste = !string.IsNullOrEmpty(blueprintLabel) ? blueprintLabel : "Construct Matrix";
                        sb.AppendLine($"--- {caste} ---");
                        sb.AppendLine($"Origin: Synthetic Construct Matrix");
                        sb.AppendLine($"Genetic Edits: {editCount} / 2");
                        sb.AppendLine($"Total Complexity: {holder.GeneSet.ComplexityTotal}");
                        sb.AppendLine($"Net Metabolism: {(holder.GeneSet.MetabolismTotal >= 0 ? "+" + holder.GeneSet.MetabolismTotal : holder.GeneSet.MetabolismTotal.ToString())}");
                        if (holder.GeneSet.ArchitesTotal > 0)
                        {
                            sb.AppendLine($"Archite Capsules: {holder.GeneSet.ArchitesTotal}");
                        }
                        sb.AppendLine();
                        sb.AppendLine("Encoded Endogenes:");
                        foreach (GeneDef gene in holder.GeneSet.GenesListForReading)
                        {
                            if (gene == null) continue;
                            sb.AppendLine($"  • {gene.label.CapitalizeFirst()} (Cpx: {gene.biostatCpx}, Met: {(gene.biostatMet >= 0 ? "+" + gene.biostatMet : gene.biostatMet.ToString())})");
                        }
                        Find.WindowStack.Add(new Dialog_MessageBox(sb.ToString(), "Close", title: parent.LabelCap));
                    }
                };
            }

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
