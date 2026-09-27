using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace EugenicsProgram
{
    public class CompEmbryoQuality : ThingComp
    {
        public CompProperties_EmbryoQuality Props => (CompProperties_EmbryoQuality)props;

        public bool isScreened = false;
        public bool hasDefects = false;
        public string defectDescription = null;
        public float defectSeverity = 0f;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isScreened, "isScreened", false);
            Scribe_Values.Look(ref hasDefects, "hasDefects", false);
            Scribe_Values.Look(ref defectDescription, "defectDescription");
            Scribe_Values.Look(ref defectSeverity, "defectSeverity", 0f);
        }

        public void IntroduceComplication(string reason, float severity)
        {
            hasDefects = true;
            defectDescription = reason;
            defectSeverity = Mathf.Clamp01(severity);
        }

        public void PerformScreening(Pawn screener)
        {
            isScreened = true;
            // High-skill screening reveals defects accurately
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
            StringBuilder sb = new StringBuilder();
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

            return sb.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            if (isScreened && hasDefects)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Defect Report",
                    defaultDesc = "Review diagnostic notes on detected embryonic genomic defects.",
                    icon = parent?.def?.uiIcon,
                    action = () =>
                    {
                        string msg = $"Embryo Genetic Diagnostic Report:\n\n" +
                                     $"Status: Defective\n" +
                                     $"Diagnosis: {defectDescription ?? "Genomic instability"}\n" +
                                     $"Severity Factor: {defectSeverity:P0}\n\n" +
                                     $"Gestating this embryo carries high risk of stillbirth, severe organ degradation, or fatal mutation.\n" +
                                     $"Recommendation: Liquefy biomass into Genetic Nutrient Paste.";
                        Find.WindowStack.Add(new Dialog_MessageBox(msg, "Dismiss", title: parent?.LabelCap ?? "Defect Diagnostic"));
                    }
                };
            }
        }
    }
}
