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
        public bool isBurned = false;

        public bool IsBlank => !isBurned && (genes == null || genes.Count == 0);

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
                // Previously initialized default template here
            }
        }

        public void InitializeDefaultTemplate(string label = null)
        {
            if (genes == null) genes = new List<GeneDef>();
            genes.Clear();
            templateLabel = label ?? Props?.defaultTemplateLabel ?? "Base Construct Template";
            string[] defaultGenes = new string[]
            {
                "Instability_Major",
                "Gene_MandatorySterility",
                "Gene_ConstructHibernation",
                "Immunity_SuperStrong",
                "Pain_Reduced",
                "Robust",
                "MeleeDamage_Strong",
                "MoveSpeed_Quick",
                "WoundHealing_Fast",
                "Superclotting",
                "RobustDigestion",
                "StrongStomach",
                "PsychicAbility_Deaf",
                "Beauty_VeryUgly",
                "LowSleep",
                "Learning_Slow"
            };
            foreach (string gName in defaultGenes)
            {
                GeneDef g = DefDatabase<GeneDef>.GetNamedSilentFail(gName);
                if (g != null) genes.Add(g);
            }
            isBurned = true;
        }

        public void BurnGenome(string label, List<GeneDef> newGenes)
        {
            if (isBurned) return;
            templateLabel = label;
            genes = new List<GeneDef>(newGenes);
            isBurned = true;
        }

        public void RecordFromPawn(Pawn pawn)
        {
            if (isBurned) return;
            if (pawn?.genes == null) return;
            if (genes == null) genes = new List<GeneDef>();
            genes.Clear();
            templateLabel = $"{pawn.LabelShortCap} Genome";
            foreach (Gene g in pawn.genes.Endogenes)
            {
                if (g?.def != null)
                {
                    genes.Add(g.def);
                }
            }
            isBurned = true;
            Messages.Message($"Genome Blueprint disc burned with genetic template from {pawn.LabelShortCap} ({genes.Count} genes).", parent, MessageTypeDefOf.PositiveEvent);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref templateLabel, "templateLabel", Props?.defaultTemplateLabel ?? "Base Construct Template");
            Scribe_Values.Look(ref isBurned, "isBurned", false);
            Scribe_Collections.Look(ref genes, "genes", LookMode.Def);
            if (genes == null) genes = new List<GeneDef>();
        }

        public override string CompInspectStringExtra()
        {
            if (IsBlank)
            {
                return "Blank Genome Disc (Requires burning at Genome Architect)";
            }

            StringBuilder sb = new StringBuilder();
            string label = string.IsNullOrEmpty(templateLabel) ? (Props?.defaultTemplateLabel ?? "Base Construct Template") : templateLabel;
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

            if (Prefs.DevMode)
            {
                if (!isBurned)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Burn Default Template",
                        defaultDesc = "Immediately burns the standard Base Construct genetic template onto this disc for testing.",
                        action = () =>
                        {
                            InitializeDefaultTemplate("Standard Construct Template");
                            Messages.Message("DEV: Disc burned with Standard Construct Template.", parent, MessageTypeDefOf.PositiveEvent);
                        }
                    };
                }
                else
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Clear Disc",
                        defaultDesc = "Resets this disc back to a blank unburned state.",
                        action = () =>
                        {
                            genes?.Clear();
                            templateLabel = null;
                            isBurned = false;
                            Messages.Message("DEV: Disc reset to blank state.", parent, MessageTypeDefOf.NeutralEvent);
                        }
                    };
                }
            }

            if (parent.Spawned && parent.Map != null && !isBurned)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Record Construct Genome",
                    defaultDesc = "Record the endogenes of an active construct colonist onto this genome blueprint disc.",
                    icon = TexButton.Copy ?? parent?.def?.uiIcon,
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
