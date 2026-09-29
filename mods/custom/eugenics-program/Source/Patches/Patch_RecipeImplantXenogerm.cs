using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EugenicsProgram
{
    [HarmonyPatch(typeof(Recipe_ImplantXenogerm), "AvailableReport")]
    public static class Patch_RecipeImplantXenogerm_AvailableReport
    {
        public static void Postfix(Thing thing, ref AcceptanceReport __result)
        {
            if (!__result.Accepted) return;

            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                bool isConstruct = pawn.story?.traits?.HasTrait(ConstructAugmentDefOf.Trait_ConstructAsset) == true ||
                                   pawn.genes?.HasActiveGene(ConstructAugmentDefOf.Gene_ConstructPsychology) == true ||
                                   pawn.genes?.HasActiveGene(ConstructAugmentDefOf.Gene_ConstructHibernation) == true;

                if (isConstruct)
                {
                    __result = new AcceptanceReport("Cannot implant xenogerm: Synthetic construct genetic architecture is permanently locked");
                }
            }
        }
    }

    [HarmonyPatch(typeof(Recipe_ImplantXenogerm), "ApplyOnPawn")]
    public static class Patch_RecipeImplantXenogerm_ApplyOnPawn
    {
        public static bool Prefix(Pawn pawn)
        {
            bool isConstruct = pawn.story?.traits?.HasTrait(ConstructAugmentDefOf.Trait_ConstructAsset) == true ||
                               pawn.genes?.HasActiveGene(ConstructAugmentDefOf.Gene_ConstructPsychology) == true ||
                               pawn.genes?.HasActiveGene(ConstructAugmentDefOf.Gene_ConstructHibernation) == true;

            if (isConstruct)
            {
                Messages.Message("Cannot implant xenogerm: Synthetic construct genetic architecture is permanently locked", pawn, MessageTypeDefOf.RejectInput, false);
                return false; // Prevent execution
            }

            return true;
        }
    }
}
