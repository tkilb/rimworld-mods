using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OrganicConstructs
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
                        if (genes[i] != null) total += genes[i].biostatCpx;
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
                        if (genes[i] != null) total += genes[i].biostatMet;
                    }
                }
                return total;
            }
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            if (genes == null || genes.Count == 0)
            {
                InitializeDefaultTemplate();
            }
        }

        public void InitializeDefaultTemplate(string label = null)
        {
            if (genes == null) genes = new List<GeneDef>();
            genes.Clear();
            templateLabel = label ?? Props?.defaultTemplateLabel ?? "SecUnit Baseline Caste";
            string[] defaultGenes = new string[]
            {
                "Gene_ConstructPsychology",
                "Construct_MetabolicallyEfficient",
                "Gene_MandatorySterility",
                "RobustDigestion",
                "StrongStomach",
                "PsychicAbility_Deaf",
                "Beauty_VeryUgly",
                "Gene_ConstructHibernation",
                "LowSleep",
                "Learning_Slow"
            };
            foreach (string gName in defaultGenes)
            {
                GeneDef g = DefDatabase<GeneDef>.GetNamedSilentFail(gName);
                if (g != null) genes.Add(g);
            }
        }

        public void RecordFromPawn(Pawn pawn)
        {
            if (pawn?.genes == null) return;
            if (genes == null) genes = new List<GeneDef>();
            genes.Clear();
            templateLabel = $"{pawn.LabelShortCap} Caste";
            foreach (Gene g in pawn.genes.Endogenes)
            {
                if (g?.def != null)
                {
                    genes.Add(g.def);
                }
            }
            Messages.Message($"Genome Blueprint disc updated with genetic caste template from {pawn.LabelShortCap} ({genes.Count} genes).", parent, MessageTypeDefOf.PositiveEvent);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref templateLabel, "templateLabel", Props?.defaultTemplateLabel ?? "Construct Caste");
            Scribe_Collections.Look(ref genes, "genes", LookMode.Def);
            if (genes == null) genes = new List<GeneDef>();
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
                yield return g;

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

            if (parent.Spawned && parent.Map != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Record Construct Caste",
                    defaultDesc = "Record the endogenes of an active construct colonist onto this genome blueprint disc.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Copy", true) ?? parent?.def?.uiIcon,
                    action = () =>
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (Pawn p in parent.Map.mapPawns.FreeColonistsSpawned)
                        {
                            if (ConstructUtility.IsConstruct(p) && p.genes != null)
                            {
                                options.Add(new FloatMenuOption($"{p.LabelShortCap} ({p.genes.Endogenes.Count} genes)", () =>
                                {
                                    RecordFromPawn(p);
                                }));
                            }
                        }
                        if (options.Count == 0)
                        {
                            options.Add(new FloatMenuOption("No constructs available to record", null));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                };
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption opt in base.CompFloatMenuOptions(selPawn))
                yield return opt;

            if (!parent.Spawned || parent.Destroyed || parent.Map == null) yield break;

            if (selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Hauling))
            {
                yield return new FloatMenuOption("Cannot haul (Hauling disabled)", null);
                yield break;
            }

            if (!selPawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Some))
            {
                yield return new FloatMenuOption($"Cannot haul {parent.Label} (Unreachable)", null);
                yield break;
            }

            if (!selPawn.CanReserve(parent, 1, 1, null, false))
            {
                yield return new FloatMenuOption($"Cannot haul {parent.Label} (Reserved)", null);
                yield break;
            }

            Building_ConstructSynthesizer bench = (Building_ConstructSynthesizer)GenClosest.ClosestThingReachable(
                parent.Position,
                parent.Map,
                ThingRequest.ForDef(ConstructDefOf.ConstructSynthesizer),
                PathEndMode.Touch,
                TraverseParms.For(selPawn),
                validator: t => t is Building_ConstructSynthesizer b && b.CanAcceptDisc && selPawn.CanReserve(b)
            );

            if (bench != null)
            {
                yield return new FloatMenuOption($"Insert {parent.Label} into {bench.LabelShort}", () =>
                {
                    bench.targetDisc = parent;
                    Job job = JobMaker.MakeJob(ConstructDefOf.Construct_HaulDiscToContainer, parent, bench);
                    job.count = 1;
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
            }
            else
            {
                yield return new FloatMenuOption($"Cannot insert {parent.Label} into construct synthesizer (No machine available)", null);
            }
        }
    }
}
