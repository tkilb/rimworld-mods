using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

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

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption opt in base.CompFloatMenuOptions(selPawn))
            {
                yield return opt;
            }

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

            if (!IsEncoded)
            {
                Building_NeuralScanner scanner = (Building_NeuralScanner)GenClosest.ClosestThingReachable(
                    parent.Position,
                    parent.Map,
                    ThingRequest.ForDef(EugenicsDefOf.NeuralScanner),
                    PathEndMode.Touch,
                    TraverseParms.For(selPawn),
                    validator: t => t is Building_NeuralScanner s && s.CanAcceptDisc && selPawn.CanReserve(s)
                );

                if (scanner != null)
                {
                    yield return new FloatMenuOption($"Insert {parent.Label} into {scanner.LabelShort}", () =>
                    {
                        scanner.targetDisc = parent;
                        Job job = JobMaker.MakeJob(EugenicsDefOf.Eugenics_HaulDiscToContainer, parent, scanner);
                        job.count = 1;
                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    });
                }
                else
                {
                    yield return new FloatMenuOption($"Cannot insert {parent.Label} into neural scanner (No machine available)", null);
                }
            }
            else
            {
                Thing vat = GenClosest.ClosestThingReachable(
                    parent.Position,
                    parent.Map,
                    ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                    PathEndMode.Touch,
                    TraverseParms.For(selPawn),
                    validator: t =>
                    {
                        if (t is ThingWithComps twc)
                        {
                            CompGrowthVatImprinter imprinter = twc.GetComp<CompGrowthVatImprinter>();
                            return imprinter != null && imprinter.CanAcceptDisc && selPawn.CanReserve(t);
                        }
                        return false;
                    }
                );

                if (vat != null)
                {
                    yield return new FloatMenuOption($"Insert {parent.Label} into {vat.LabelShort}", () =>
                    {
                        CompGrowthVatImprinter imprinter = ((ThingWithComps)vat).GetComp<CompGrowthVatImprinter>();
                        if (imprinter != null) imprinter.targetDisc = parent;
                        Job job = JobMaker.MakeJob(EugenicsDefOf.Eugenics_HaulDiscToContainer, parent, vat);
                        job.count = 1;
                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    });
                }
                else
                {
                    yield return new FloatMenuOption($"Cannot insert {parent.Label} into growth vat (No machine available)", null);
                }
            }
        }
    }
}
