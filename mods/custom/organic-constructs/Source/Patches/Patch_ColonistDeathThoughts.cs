using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OrganicConstructs
{
    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike")]
    public static class Patch_PawnDiedOrDownedThoughtsUtility
    {
        public static void Postfix(
            Pawn victim,
            DamageInfo? dinfo,
            PawnDiedOrDownedThoughtsKind thoughtsKind,
            List<IndividualThoughtToAdd> outIndividualThoughts,
            List<ThoughtToAddToAll> outAllColonistsThoughts)
        {
            if (victim == null || !ConstructUtility.IsConstruct(victim))
                return;

            bool hadColonistDiedThought = false;

            // Constructs do not trigger colonist grief or loss debuffs for normal colonists
            if (outAllColonistsThoughts != null)
            {
                hadColonistDiedThought = outAllColonistsThoughts.RemoveAll(t =>
                    t.thoughtDef == ThoughtDefOf.KnowColonistDied ||
                    (ThoughtDefOf.ColonistLost != null && t.thoughtDef == ThoughtDefOf.ColonistLost)) > 0;
            }

            if (outIndividualThoughts != null)
            {
                outIndividualThoughts.RemoveAll(t =>
                    t.thought?.def == ThoughtDefOf.KnowColonistDied ||
                    (ThoughtDefOf.ColonistLost != null && t.thought?.def == ThoughtDefOf.ColonistLost));
            }

            // Pawns with Kind trait or Kind Instinct gene mourn construct deaths
            if (hadColonistDiedThought && ConstructDefOf.Construct_KindPity != null && outIndividualThoughts != null)
            {
                List<Pawn> colonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
                if (colonists != null)
                {
                    for (int i = 0; i < colonists.Count; i++)
                    {
                        Pawn colonist = colonists[i];
                        if (colonist == null || colonist.Dead || colonist == victim)
                            continue;

                        if (ConstructUtility.IsConstruct(colonist))
                            continue;

                        if (IsKindColonist(colonist))
                        {
                            outIndividualThoughts.Add(new IndividualThoughtToAdd(
                                ConstructDefOf.Construct_KindPity,
                                colonist,
                                victim));
                        }
                    }
                }
            }
        }

        private static bool IsKindColonist(Pawn pawn)
        {
            if (pawn.story?.traits?.HasTrait(TraitDefOf.Kind) == true)
                return true;

            if (ModsConfig.BiotechActive && pawn.genes != null)
            {
                GeneDef kindInstinct = DefDatabase<GeneDef>.GetNamedSilentFail("KindInstinct");
                if (kindInstinct != null && pawn.genes.HasActiveGene(kindInstinct))
                    return true;
            }

            return false;
        }
    }
}
