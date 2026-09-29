using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EugenicsProgram
{
    public class CompEmbryoQuality : ThingComp
    {
        public CompProperties_EmbryoQuality Props => (CompProperties_EmbryoQuality)props;

        public int editCount = 0;
        public bool isConstruct = false;

        /// <summary>Player has manually designated this embryo for biomass recycling.</summary>
        public bool designatedForRecycling = false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref editCount, "editCount", 0);
            Scribe_Values.Look(ref isConstruct, "isConstruct", false);
            Scribe_Values.Look(ref designatedForRecycling, "designatedForRecycling", false);

            // Clean ExposeData backwards compatibility for stripped screening/defect fields
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                bool dummyBool = false;
                string dummyStr = null;
                float dummyFloat = 0f;
                Scribe_Values.Look(ref dummyBool, "isScreened", false);
                Scribe_Values.Look(ref dummyBool, "hasDefects", false);
                Scribe_Values.Look(ref dummyStr, "defectDescription");
                Scribe_Values.Look(ref dummyFloat, "defectSeverity", 0f);
            }
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

            sb.Append($"Genetic Edits: {editCount}/2");

            if (designatedForRecycling)
            {
                sb.Append("\n[Designated for Biomass Recycling]");
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
                defaultDesc  = "Toggle manual designation for biomass liquefaction into Genetic Nutrient Paste.",
                icon         = parent?.def?.uiIcon,
                isActive     = () => designatedForRecycling,
                toggleAction = () => designatedForRecycling = !designatedForRecycling
            };

            // Open gene splicing dialog
            Command_Action editGenesCmd = new Command_Action
            {
                defaultLabel = "Edit Genes",
                defaultDesc  = "Open the embryonic gene splicing interface to add, remove, or overwrite genes using connected Gene Banks and Blueprint Discs.",
                icon         = parent?.def?.uiIcon,
                action       = () =>
                {
                    Building assembler = FindNearestAssembler();
                    if (assembler == null)
                    {
                        Messages.Message(
                            "No accessible Gene Assembler found. A Gene Assembler connected to Gene Banks is required for embryo gene splicing.",
                            parent,
                            MessageTypeDefOf.RejectInput);
                        return;
                    }

                    if (editCount >= 2)
                    {
                        Messages.Message(
                            "Cannot edit embryo genetics: Maximum genetic edit operations (2) reached for this embryo.",
                            parent,
                            MessageTypeDefOf.RejectInput);
                        return;
                    }

                    Find.WindowStack.Add(new Dialog_EditEmbryoGenes(parent, assembler));
                }
            };

            if (editCount >= 2)
            {
                editGenesCmd.Disable("Maximum genetic edits (2) reached.");
            }

            yield return editGenesCmd;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private Building FindNearestAssembler()
        {
            if (parent?.MapHeld == null) return null;

            return (Building)GenClosest.ClosestThingReachable(
                parent.PositionHeld,
                parent.MapHeld,
                ThingRequest.ForDef(ThingDefOf.GeneAssembler),
                PathEndMode.ClosestTouch,
                TraverseParms.For(TraverseMode.NoPassClosedDoors),
                validator: b => b is Building_GeneAssembler);
        }
    }
}
