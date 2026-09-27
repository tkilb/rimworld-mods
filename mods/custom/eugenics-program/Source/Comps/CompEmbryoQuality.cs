using System.Collections.Generic;
using System.Linq;
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

        public bool isScreened            = false;
        public bool hasDefects            = false;
        public string defectDescription   = null;
        public float defectSeverity       = 0f;
        /// <summary>Player has manually designated this embryo for biomass recycling.</summary>
        public bool designatedForRecycling = false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isScreened,             "isScreened",             false);
            Scribe_Values.Look(ref hasDefects,             "hasDefects",             false);
            Scribe_Values.Look(ref defectDescription,      "defectDescription");
            Scribe_Values.Look(ref defectSeverity,         "defectSeverity",         0f);
            Scribe_Values.Look(ref designatedForRecycling, "designatedForRecycling", false);
        }

        public void IntroduceComplication(string reason, float severity)
        {
            hasDefects        = true;
            defectDescription = reason;
            defectSeverity    = Mathf.Clamp01(severity);
        }

        public void PerformScreening(Pawn screener)
        {
            isScreened = true;
            if (hasDefects)
            {
                Messages.Message(
                    $"Genomic screening complete: Defects identified ({defectDescription ?? "Genomic instability"}). Recommended for biomass liquefaction.",
                    parent,
                    MessageTypeDefOf.CautionInput);
            }
            else
            {
                Messages.Message(
                    "Genomic screening complete: Genetic integrity pristine. Cleared for vat gestation.",
                    parent,
                    MessageTypeDefOf.PositiveEvent);
            }
        }

        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();

            if (isScreened)
            {
                if (hasDefects)
                {
                    sb.Append("Genomic Screening: DEFECTS DETECTED (");
                    sb.Append(string.IsNullOrEmpty(defectDescription) ? "Cellular instability" : defectDescription);
                    sb.Append(")");
                }
                else
                {
                    sb.Append("Genomic Screening: Pristine (No defects detected)");
                }
            }
            else
            {
                sb.Append("Genomic Screening: Unscreened (Risks undetected complications)");
            }

            if (designatedForRecycling)
                sb.Append("\n[Designated for Biomass Recycling]");

            return sb.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;

            // Defect diagnostic report (shown only after screening reveals defects)
            if (isScreened && hasDefects)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Defect Report",
                    defaultDesc  = "Review diagnostic notes on detected embryonic genomic defects.",
                    icon         = parent?.def?.uiIcon,
                    action       = () =>
                    {
                        string msg = $"Embryo Genetic Diagnostic Report:\n\n"
                                   + $"Status: Defective\n"
                                   + $"Diagnosis: {defectDescription ?? "Genomic instability"}\n"
                                   + $"Severity Factor: {defectSeverity:P0}\n\n"
                                   + "Gestating this embryo carries high risk of stillbirth, severe organ degradation, or fatal mutation.\n"
                                   + "Recommendation: Liquefy biomass into Genetic Nutrient Paste.";
                        Find.WindowStack.Add(new Dialog_MessageBox(msg, "Dismiss",
                            title: parent?.LabelCap ?? "Defect Diagnostic"));
                    }
                };
            }

            // Manual recycle designation toggle (for pristine/unscreened embryos the player wants to cull)
            yield return new Command_Toggle
            {
                defaultLabel = designatedForRecycling ? "Cancel Recycle Order" : "Designate for Recycling",
                defaultDesc  = "Toggle manual designation for biomass liquefaction. Defective embryos are queued automatically after screening.",
                icon         = parent?.def?.uiIcon,
                isActive     = () => designatedForRecycling,
                toggleAction = () => designatedForRecycling = !designatedForRecycling
            };

            // Open gene splicing dialog
            yield return new Command_Action
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
                    Find.WindowStack.Add(new Dialog_EditEmbryoGenes(parent, assembler));
                }
            };
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
