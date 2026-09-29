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
            if (victim == null || !Gene_ConstructPsychology.IsConstruct(victim))
                return;

            // Constructs do not trigger colonist grief or loss debuffs
            outAllColonistsThoughts?.RemoveAll(t =>
                t.thoughtDef == ThoughtDefOf.KnowColonistDied ||
                (ThoughtDefOf.ColonistLost != null && t.thoughtDef == ThoughtDefOf.ColonistLost));

            outIndividualThoughts?.RemoveAll(t =>
                t.thought?.def == ThoughtDefOf.KnowColonistDied ||
                (ThoughtDefOf.ColonistLost != null && t.thought?.def == ThoughtDefOf.ColonistLost));
        }
    }
}
