using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EugenicsProgram
{
    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), nameof(PawnDiedOrDownedThoughtsUtility.AppendThoughts_ForHumanlike))]
    public static class Patch_PawnDiedOrDownedThoughtsUtility
    {
        public static void Postfix(
            Pawn victim,
            List<IndividualThoughtToAdd> outIndividualThoughts,
            List<ThoughtToAddToAll> outAllColonistsThoughts)
        {
            if (victim == null || !Gene_ConstructPsychology.IsConstruct(victim))
            {
                return;
            }

            // Property constructs do not trigger grief or colonist loss mood debuffs
            if (outAllColonistsThoughts != null)
            {
                outAllColonistsThoughts.RemoveAll(t =>
                    t.thoughtDef == ThoughtDefOf.KnowColonistDied ||
                    (ThoughtDefOf.ColonistLost != null && t.thoughtDef == ThoughtDefOf.ColonistLost));
            }

            if (outIndividualThoughts != null)
            {
                outIndividualThoughts.RemoveAll(t =>
                    t.thoughtDef == ThoughtDefOf.KnowColonistDied ||
                    (ThoughtDefOf.ColonistLost != null && t.thoughtDef == ThoughtDefOf.ColonistLost));
            }
        }
    }
}
