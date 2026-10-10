using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
    public static class Patch_PawnGenerator_GeneratePawn
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __result)
        {
            if (__result != null && ConstructUtility.IsConstruct(__result))
            {
                ConstructUtility.ApplyConstructPhysiology(__result);
                if (__result.Faction == Faction.OfPlayer || __result.kindDef?.defName == "Construct_Colonist")
                {
                    CompGrowthVatImprinter.ApplyBaselineSkills(__result);
                    CompGrowthVatImprinter.WipePassions(__result);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_NamePawn), MethodType.Constructor, new[] {
        typeof(Pawn),
        typeof(NameFilter),
        typeof(NameFilter),
        typeof(Dictionary<NameFilter, List<string>>),
        typeof(string),
        typeof(string),
        typeof(string),
        typeof(string)
    })]
    public static class Patch_Dialog_NamePawn
    {
        [HarmonyPostfix]
        public static void Postfix(Dialog_NamePawn __instance, Pawn pawn)
        {
            if (pawn != null && ConstructUtility.IsConstruct(pawn))
            {
                Traverse trav = Traverse.Create(__instance);
                trav.Field("renameText").SetValue(new TaggedString("Designate Construct Callsign"));
                string unitId = (pawn.Name is NameTriple nt) ? nt.First : pawn.ThingID;
                trav.Field("descriptionText").SetValue(new TaggedString($"Manufactured biological human construct.\nUnit Identifier: {unitId}"));
                trav.Field("renameHeight").SetValue((float?)null);
                trav.Field("descriptionHeight").SetValue((float?)null);
            }
        }
    }
}
