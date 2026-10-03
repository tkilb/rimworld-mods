using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    public static class BiowareSkillUtility
    {
        public static readonly HashSet<string> AllBiowareHediffNames = new HashSet<string>
        {
            "Construct_BasicCombatPackage",
            "Construct_IntermediateCombatPackage",
            "Construct_AdvancedCombatPackage",
            "Construct_BasicMedicalPackage",
            "Construct_IntermediateMedicalPackage",
            "Construct_AdvancedMedicalPackage",
            "Construct_BasicIndustrialPackage",
            "Construct_IntermediateIndustrialPackage",
            "Construct_AdvancedIndustrialPackage",
            "Construct_BasicLaborerPackage",
            "Construct_IntermediateLaborerPackage",
            "Construct_AdvancedLaborerPackage"
        };

        public static bool IsBiowareAugment(HediffDef def)
        {
            if (def == null) return false;
            return AllBiowareHediffNames.Contains(def.defName);
        }

        public static int GetSkillBonus(Pawn pawn, SkillDef skill)
        {
            if (pawn?.health?.hediffSet == null || skill == null)
                return 0;

            int bonus = 0;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff h = hediffs[i];
                if (h.def == null) continue;

                bonus += GetBonusForHediff(h.def.defName, skill);
            }

            return bonus;
        }

        private static int GetBonusForHediff(string defName, SkillDef skill)
        {
            // Combat: Shooting, Melee (+4 / +6 / +8)
            if (skill == SkillDefOf.Shooting || skill == SkillDefOf.Melee)
            {
                if (defName == "Construct_AdvancedCombatPackage") return 8;
                if (defName == "Construct_IntermediateCombatPackage") return 6;
                if (defName == "Construct_BasicCombatPackage") return 4;
            }

            // Biomedical: Medicine, Crafting (+3 / +5 / +7)
            if (skill == SkillDefOf.Medicine || skill == SkillDefOf.Crafting)
            {
                if (defName == "Construct_AdvancedMedicalPackage") return 7;
                if (defName == "Construct_IntermediateMedicalPackage") return 5;
                if (defName == "Construct_BasicMedicalPackage") return 3;
            }

            // Industrial: Construction, Mining, Crafting (+2 / +4 / +6)
            if (skill == SkillDefOf.Construction || skill == SkillDefOf.Mining || skill == SkillDefOf.Crafting)
            {
                if (defName == "Construct_AdvancedIndustrialPackage") return 6;
                if (defName == "Construct_IntermediateIndustrialPackage") return 4;
                if (defName == "Construct_BasicIndustrialPackage") return 2;
            }

            // Laborer: Cooking, Plants, Animals (+2 / +4 / +6)
            if (skill == SkillDefOf.Cooking || skill == SkillDefOf.Plants || skill == SkillDefOf.Animals)
            {
                if (defName == "Construct_AdvancedLaborerPackage") return 6;
                if (defName == "Construct_IntermediateLaborerPackage") return 4;
                if (defName == "Construct_BasicLaborerPackage") return 2;
            }

            return 0;
        }
    }
}
