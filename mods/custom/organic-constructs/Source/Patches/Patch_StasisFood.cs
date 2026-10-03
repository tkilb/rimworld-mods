using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    /// <summary>
    /// Suspends food requirement and hunger decay for constructs while in hibernation stasis.
    /// </summary>
    [HarmonyPatch(typeof(Need_Food), "IsFrozen", MethodType.Getter)]
    public static class Patch_Need_Food_IsFrozen
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, Pawn ___pawn)
        {
            if (!__result && ___pawn != null)
            {
                var gene = ___pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && gene.inStasis)
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Need_Food), nameof(Need_Food.FoodFallPerTick), MethodType.Getter)]
    public static class Patch_Need_Food_FoodFallPerTick
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn != null)
            {
                var gene = ___pawn.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && gene.inStasis)
                {
                    __result = 0f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(FeedPatientUtility), "ShouldBeFed")]
    public static class Patch_FeedPatientUtility_ShouldBeFed
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref bool __result)
        {
            if (__result && p != null)
            {
                var gene = p.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && gene.inStasis)
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(FeedPatientUtility), "IsHungry")]
    public static class Patch_FeedPatientUtility_IsHungry
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref bool __result)
        {
            if (__result && p != null)
            {
                var gene = p.genes?.GetGene(ConstructDefOf.Gene_ConstructHibernation) as Gene_ConstructHibernation;
                if (gene != null && gene.inStasis)
                {
                    __result = false;
                }
            }
        }
    }
}
