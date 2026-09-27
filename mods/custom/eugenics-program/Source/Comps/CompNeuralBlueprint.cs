using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace EugenicsProgram
{
    public class CompNeuralBlueprint : ThingComp
    {
        public CompProperties_NeuralBlueprint Props => (CompProperties_NeuralBlueprint)props;

        public string doctrineTitle;
        public string donorName;
        public Dictionary<SkillDef, int> skillLevels = new Dictionary<SkillDef, int>();
        public Dictionary<SkillDef, Passion> passions = new Dictionary<SkillDef, Passion>();

        public bool IsEncoded => (skillLevels != null && skillLevels.Count > 0) || !string.IsNullOrEmpty(donorName);

        public void EncodePawnProfile(Pawn donor, string customTitle = null)
        {
            if (donor == null || donor.skills == null) return;

            donorName = donor.LabelShortCap;
            doctrineTitle = !string.IsNullOrEmpty(customTitle)
                ? customTitle
                : $"{donor.LabelShortCap}'s Combat Doctrine";

            skillLevels.Clear();
            passions.Clear();

            int cap = Props?.defaultSkillCap ?? 14;
            int minSkill = Props?.minSkillToEncode ?? 6;

            foreach (SkillRecord skill in donor.skills.skills)
            {
                if (skill.Level >= minSkill)
                {
                    int encodedLevel = Mathf.Min(skill.Level, cap);
                    skillLevels[skill.def] = encodedLevel;
                    if (skill.passion != Passion.None)
                    {
                        passions[skill.def] = skill.passion;
                    }
                }
            }
        }

        private List<SkillDef> skillKeysWorkingList;
        private List<int> skillValuesWorkingList;
        private List<SkillDef> passionKeysWorkingList;
        private List<Passion> passionValuesWorkingList;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref doctrineTitle, "doctrineTitle", Props?.defaultDoctrineTitle ?? "Standard Combat Doctrine");
            Scribe_Values.Look(ref donorName, "donorName");
            Scribe_Collections.Look(ref skillLevels, "skillLevels", LookMode.Def, LookMode.Value, ref skillKeysWorkingList, ref skillValuesWorkingList);
            Scribe_Collections.Look(ref passions, "passions", LookMode.Def, LookMode.Value, ref passionKeysWorkingList, ref passionValuesWorkingList);

            if (skillLevels == null)
            {
                skillLevels = new Dictionary<SkillDef, int>();
            }
            if (passions == null)
            {
                passions = new Dictionary<SkillDef, Passion>();
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            string title = string.IsNullOrEmpty(doctrineTitle) ? (Props?.defaultDoctrineTitle ?? "Standard Combat Doctrine") : doctrineTitle;
            sb.Append("Doctrine: ").Append(title);
            if (!string.IsNullOrEmpty(donorName))
            {
                sb.Append(" (Source: ").Append(donorName).Append(")");
            }

            if (skillLevels != null && skillLevels.Count > 0)
            {
                sb.Append("\nEncoded Skills: ");
                int count = 0;
                foreach (var kvp in skillLevels)
                {
                    if (kvp.Value <= 0) continue;
                    if (count > 0) sb.Append(", ");
                    sb.Append(kvp.Key.label.CapitalizeFirst()).Append(" ").Append(kvp.Value);
                    if (passions.TryGetValue(kvp.Key, out Passion p) && p != Passion.None)
                    {
                        sb.Append(p == Passion.Major ? "++" : "+");
                    }
                    count++;
                    if (count >= 3 && skillLevels.Count > 3)
                    {
                        sb.Append($" (+{skillLevels.Count - 3} more)");
                        break;
                    }
                }
            }

            return sb.ToString();
        }

        public override string TransformLabel(string label)
        {
            if (!string.IsNullOrEmpty(doctrineTitle))
            {
                return $"{label} ({doctrineTitle})";
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
                defaultLabel = "Inspect Doctrine",
                defaultDesc = "View the skills, combat doctrines, and passions encoded on this neural imprinting disc.",
                icon = parent?.def?.uiIcon,
                action = () =>
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"--- {doctrineTitle ?? "Neural Imprint Doctrine"} ---");
                    if (!string.IsNullOrEmpty(donorName))
                    {
                        sb.AppendLine($"Mentor / Source: {donorName}");
                    }
                    sb.AppendLine();
                    sb.AppendLine("Imprinted Skills & Passions:");
                    if (skillLevels == null || skillLevels.Count == 0)
                    {
                        sb.AppendLine("  (No skills recorded)");
                    }
                    else
                    {
                        foreach (var kvp in skillLevels)
                        {
                            string passionStr = "";
                            if (passions.TryGetValue(kvp.Key, out Passion p))
                            {
                                if (p == Passion.Major) passionStr = " (Burning Passion 🔥🔥)";
                                else if (p == Passion.Minor) passionStr = " (Interested Passion 🔥)";
                            }
                            sb.AppendLine($"  • {kvp.Key.label.CapitalizeFirst()}: Level {kvp.Value}{passionStr}");
                        }
                    }

                    Find.WindowStack.Add(new Dialog_MessageBox(sb.ToString(), "Close", title: parent?.LabelCap ?? "Neural Blueprint Disc"));
                }
            };
        }
    }
}
