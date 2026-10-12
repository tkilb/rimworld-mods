using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    /// <summary>
    /// Constructs prioritize insect meat, insect corpses, and meals cooked with insect meat over standard colonist food.
    /// Combined with RobustDigestion and StrongStomach, constructs digest raw or cooked insect food without penalties.
    /// </summary>
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality))]
    public static class Patch_FoodUtility_FoodOptimality
    {
        public const float InsectFoodOptimalityBonus = 150f;

        [HarmonyPostfix]
        public static void Postfix(ref float __result, Pawn eater, Thing foodSource, ThingDef foodDef)
        {
            if (eater == null || !ConstructUtility.IsConstruct(eater))
                return;

            if (IsInsectFood(foodSource, foodDef))
            {
                __result += InsectFoodOptimalityBonus;
            }
        }

        public static bool IsInsectFood(Thing foodSource, ThingDef foodDef)
        {
            if (foodSource != null)
            {
                if (FoodUtility.IsInsectCorpseOrInsectMeatIngredient(foodSource))
                    return true;

                CompIngredients comp = foodSource.TryGetComp<CompIngredients>();
                if (comp != null && comp.ingredients != null)
                {
                    for (int i = 0; i < comp.ingredients.Count; i++)
                    {
                        ThingDef ing = comp.ingredients[i];
                        if (ing != null && IsRawInsectMeat(ing))
                            return true;
                    }
                }
            }

            if (foodDef != null && IsRawInsectMeat(foodDef))
            {
                return true;
            }

            return false;
        }

        private static bool IsRawInsectMeat(ThingDef def)
        {
            if (def == null) return false;
            if (def.IsMeat && def.ingestible?.sourceDef?.race?.FleshType == FleshTypeDefOf.Insectoid)
                return true;
            if (def.defName == "Meat_Megaspider" || (def.defName.StartsWith("Meat_") && def.label != null && def.label.IndexOf("insect", System.StringComparison.OrdinalIgnoreCase) >= 0))
                return true;
            return false;
        }
    }
}
