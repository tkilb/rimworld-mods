using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    public static class BiowareSkillUtility
    {
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

            // Medical: Medicine (+2 / +4 / +6)
            if (skill == SkillDefOf.Medicine)
            {
                if (defName == "Construct_AdvancedMedicalPackage") return 6;
                if (defName == "Construct_IntermediateMedicalPackage") return 4;
                if (defName == "Construct_BasicMedicalPackage") return 2;
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
