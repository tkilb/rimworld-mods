using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace EugenicsProgram
{
    public class CompGenomeBlueprint : ThingComp
    {
        public CompProperties_GenomeBlueprint Props => (CompProperties_GenomeBlueprint)props;

        public string templateLabel;
        public List<GeneDef> genes = new List<GeneDef>();

        public int ComplexityTotal
        {
            get
            {
                int total = 0;
                if (genes != null)
                {
                    for (int i = 0; i < genes.Count; i++)
                    {
                        if (genes[i] != null)
                        {
                            total += genes[i].biostatCpx;
                        }
                    }
                }
                return total;
            }
        }

        public int MetabolicTotal
        {
            get
            {
                int total = 0;
                if (genes != null)
                {
                    for (int i = 0; i < genes.Count; i++)
                    {
                        if (genes[i] != null)
                        {
                            total += genes[i].biostatMet;
                        }
                    }
                }
                return total;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref templateLabel, "templateLabel", Props?.defaultTemplateLabel ?? "Construct Caste");
            Scribe_Collections.Look(ref genes, "genes", LookMode.Def);
            if (genes == null)
            {
                genes = new List<GeneDef>();
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            string label = string.IsNullOrEmpty(templateLabel) ? (Props?.defaultTemplateLabel ?? "Construct Caste") : templateLabel;
            sb.Append("Blueprint: ").Append(label);
            sb.Append("\nEncoded Genes: ").Append(genes != null ? genes.Count : 0);
            sb.Append(" (Complexity: ").Append(ComplexityTotal);
            sb.Append(", Net Metabolism: ").Append(MetabolicTotal >= 0 ? "+" + MetabolicTotal : MetabolicTotal.ToString()).Append(")");

            if (genes != null && genes.Count > 0)
            {
                sb.Append("\nGenes: ");
                for (int i = 0; i < genes.Count; i++)
                {
                    if (genes[i] == null) continue;
                    if (i > 0) sb.Append(", ");
                    sb.Append(genes[i].label.CapitalizeFirst());
                    if (i >= 4 && genes.Count > 5)
                    {
                        sb.Append($" (+{genes.Count - 5} more)");
                        break;
                    }
                }
            }
            return sb.ToString();
        }

        public override string TransformLabel(string label)
        {
            if (!string.IsNullOrEmpty(templateLabel))
            {
                return $"{label} ({templateLabel})";
            }
            return label;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            yield return new Command_Action
            {
                defaultLabel = "Inspect Blueprint",
                defaultDesc = "View the complete list of genes encoded on this physical genome blueprint disc.",
                icon = parent?.def?.uiIcon,
                action = () =>
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"--- {templateLabel ?? "Genome Blueprint"} ---");
                    sb.AppendLine($"Total Complexity: {ComplexityTotal}");
                    sb.AppendLine($"Net Metabolic Efficiency: {(MetabolicTotal >= 0 ? "+" + MetabolicTotal : MetabolicTotal.ToString())}");
                    sb.AppendLine();
                    sb.AppendLine("Encoded Genes:");
                    if (genes == null || genes.Count == 0)
                    {
                        sb.AppendLine("  (Empty disc)");
                    }
                    else
                    {
                        foreach (GeneDef gene in genes)
                        {
                            if (gene == null) continue;
                            sb.AppendLine($"  • {gene.label.CapitalizeFirst()} (Cpx: {gene.biostatCpx}, Met: {(gene.biostatMet >= 0 ? "+" + gene.biostatMet : gene.biostatMet.ToString())})");
                        }
                    }
                    Find.WindowStack.Add(new Dialog_MessageBox(sb.ToString(), "Close", title: parent?.LabelCap ?? "Genome Blueprint"));
                }
            };
        }
    }
}
