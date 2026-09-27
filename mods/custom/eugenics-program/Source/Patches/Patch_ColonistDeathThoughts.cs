using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EugenicsProgram
{
    /// <summary>
    /// Postfix patch for AppendThoughts_ForHumanlike (RimWorld 1.6 signature).
    /// Strips all colonist grief/loss thoughts when the dead pawn is a Construct.
    /// </summary>
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

            // Property constructs do not trigger colonist grief or loss debuffs
            outAllColonistsThoughts?.RemoveAll(t =>
                t.thoughtDef == ThoughtDefOf.KnowColonistDied ||
                (ThoughtDefOf.ColonistLost != null && t.thoughtDef == ThoughtDefOf.ColonistLost));

            outIndividualThoughts?.RemoveAll(t =>
                t.thought?.def == ThoughtDefOf.KnowColonistDied ||
                (ThoughtDefOf.ColonistLost != null && t.thought?.def == ThoughtDefOf.ColonistLost));
        }
    }
}
