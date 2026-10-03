using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Aptitude), MethodType.Getter)]
    public static class Patch_SkillRecord_Aptitude
    {
        [HarmonyPostfix]
        public static void Postfix(SkillRecord __instance, Pawn ___pawn, ref int __result)
        {
            if (___pawn != null && __instance?.def != null)
            {
                __result += BiowareSkillUtility.GetSkillBonus(___pawn, __instance.def);
            }
        }
    }
}
